using System.Text;
using System.Text.Json;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Devplus.Messaging.Services
{
    public class RabbitMqHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMqHostedService> _logger;

        public RabbitMqHostedService(IServiceProvider serviceProvider, IConnectionFactory connectionFactory, ILogger<RabbitMqHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _connection = connectionFactory.CreateConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var consumers = scope.ServiceProvider.GetServices<IMessagingConsumer>().ToList();

            foreach (var consumer in consumers)
            {
                var consumerType = consumer.GetType();
                var queueName = consumer.QueueName;
                var exchangeName = consumer.ExchangeName;
                var routingKey = consumer.RoutingKey;
                var maxRetry = consumer.MaxRetry;
                var prefetchCount = consumer.PrefetchCount;

                if (string.IsNullOrEmpty(queueName))
                {
                    _logger.LogWarning("Consumidor {0} não possui uma fila definida.", consumerType.Name);
                    continue;
                }
                if (string.IsNullOrEmpty(exchangeName))
                {
                    _logger.LogWarning("Consumidor {0} não possui uma Exchange definida.", consumerType.Name);
                    continue;
                }

                var dlxExchange = $"{exchangeName}-dlx";
                var dlqQueue = $"{queueName}-dlq";

                var _channel = _connection.CreateModel();

                _channel.BasicQos(0, prefetchCount, global: false);

                _channel.ExchangeDeclare(exchange: exchangeName, type: "topic", durable: true, autoDelete: false);
                _channel.ExchangeDeclare(exchange: dlxExchange, type: "fanout", durable: true, autoDelete: false);

                var queueArguments = consumer.QueueType == Enum.QueueType.Quorum
                    ? new Dictionary<string, object> { { "x-queue-type", "quorum" } }
                    : null;

                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);
                _channel.QueueDeclare(queue: dlqQueue, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);

                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
                _channel.QueueBind(queue: dlqQueue, exchange: dlxExchange, routingKey: "");

                var eventConsumer = new EventingBasicConsumer(_channel);
                eventConsumer.Received += async (sender, args) =>
                {
                    var body = args.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    var props = args.BasicProperties;

                    try
                    {
                        var cloudEvent = JsonSerializer.Deserialize(messageJson,
                            typeof(CloudEvent<object>),
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                PropertyNameCaseInsensitive = true
                            });

                        if (cloudEvent != null)
                        {
                            await consumer.ConsumeAsync((CloudEvent<object>)cloudEvent, stoppingToken);
                        }

                        _channel.BasicAck(args.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}", queueName);

                        int retryCount = 0;
                        if (props.Headers != null && props.Headers.TryGetValue("x-retry-count", out var headerValue))
                        {
                            if (headerValue is byte[] bytes)
                                retryCount = int.Parse(Encoding.UTF8.GetString(bytes));
                        }

                        if (retryCount >= maxRetry)
                        {
                            _logger.LogWarning("Tentativas excedidas para mensagem. Enviando para DLQ: {DlqQueue}", dlqQueue);

                            var dlqProps = _channel.CreateBasicProperties();
                            dlqProps.Persistent = true;
                            dlqProps.Headers = props.Headers;

                            if (dlqProps.Headers == null)
                                dlqProps.Headers = new Dictionary<string, object>();

                            dlqProps.Headers["x-send-dlq"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));

                            _channel.BasicPublish(exchange: dlxExchange, routingKey: "", basicProperties: dlqProps, body: body);
                            _channel.BasicAck(args.DeliveryTag, false);
                        }
                        else
                        {
                            retryCount++;

                            var retryProps = _channel.CreateBasicProperties();
                            retryProps.Persistent = true;
                            retryProps.Headers = props.Headers ?? new Dictionary<string, object>();
                            retryProps.Headers["x-retry-count"] = Encoding.UTF8.GetBytes(retryCount.ToString());
                            retryProps.Headers["x-last-process"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));

                            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: retryProps, body: body);
                            _channel.BasicAck(args.DeliveryTag, false);
                        }

                    }
                };

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: eventConsumer);
            }

            await Task.CompletedTask;
        }
    }
}