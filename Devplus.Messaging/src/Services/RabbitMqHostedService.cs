using System.Text;
using System.Text.Json;
using Devplus.Messaging.Configuration;
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
        private readonly RabbitMqConfig _rabbitMqConfig;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqHostedService(IServiceProvider serviceProvider,
                                    IConnectionFactory connectionFactory,
                                    RabbitMqConfig rabbitMqConfig,
                                    IServiceScopeFactory scopeFactory,
                                    ILogger<RabbitMqHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _rabbitMqConfig = rabbitMqConfig;
            _scopeFactory = scopeFactory;
            _connection = connectionFactory.CreateConnection();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var consumers = scope.ServiceProvider.GetServices<IMessagingConsumer>().ToList();

            foreach (var consumer in consumers)
            {
                RegisterConsumer(consumer, stoppingToken);
            }

            await Task.CompletedTask;
        }
        private void RegisterConsumer(IMessagingConsumer consumer, CancellationToken stoppingToken)
        {
            var consumerType = consumer.GetType();
            var queueName = consumer.QueueName;
            var exchangeName = consumer.ExchangeName;
            var routingKey = consumer.RoutingKey;
            var maxRetry = consumer.MaxRetry;
            var prefetchCount = consumer.PrefetchCount;

            if (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Cancelado registro do consumidor {Consumer} por shutdown da aplicação.", consumerType.Name);
                return;
            }

            if (string.IsNullOrEmpty(queueName))
            {
                _logger.LogWarning("Consumidor {0} não possui uma fila definida.", consumerType.Name);
                return;
            }
            if (string.IsNullOrEmpty(exchangeName))
            {
                _logger.LogWarning("Consumidor {0} não possui uma Exchange definida.", consumerType.Name);
                return;
            }

            var dlxExchange = $"{exchangeName}-dlx";
            var dlqQueue = $"{queueName}-dlq";

            var channel = _connection.CreateModel();
            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogError(ea.Exception,
                    "Erro no canal do consumidor {Consumer}. Queue: {Queue}, Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                    consumerType.Name, queueName, exchangeName, routingKey);
            };

            channel.ModelShutdown += (sender, args) =>
            {
                _logger.LogWarning("Canal do consumidor {Consumer} foi encerrado. Reason: {Reason}", consumerType.Name, args.ReplyText);

                Task.Run(async () =>
                {
                    var delay = TimeSpan.FromSeconds(5);
                    var maxDelay = TimeSpan.FromMinutes(5);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            await Task.Delay(delay, stoppingToken);

                            RegisterConsumer(consumer, stoppingToken);

                            _logger.LogInformation("Consumidor {Consumer} recriado com sucesso após falha.", consumerType.Name);
                            break; // saiu do loop ao conseguir registrar
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Falha ao tentar recriar o consumidor {Consumer}. Nova tentativa em {Delay}...", consumerType.Name, delay);
                            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelay.TotalSeconds));
                        }
                    }
                });
            };

            var effectivePrefetch = _rabbitMqConfig.UseGlobalPrefetch
                ? _rabbitMqConfig.GlobalPrefetchCount
                : prefetchCount;

            channel.BasicQos(0, effectivePrefetch, global: _rabbitMqConfig.UseGlobalPrefetch);
            var queueArguments = consumer.QueueType == Enum.QueueType.Quorum
                                ? new Dictionary<string, object> { { "x-queue-type", "quorum" } }
                                : null;

            channel.ExchangeDeclare(exchange: exchangeName, type: "topic", durable: true, autoDelete: false);
            channel.ExchangeDeclare(exchange: dlxExchange, type: "fanout", durable: true, autoDelete: false);

            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);
            channel.QueueDeclare(queue: dlqQueue, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments);

            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
            channel.QueueBind(queue: dlqQueue, exchange: dlxExchange, routingKey: "");

            var eventConsumer = new EventingBasicConsumer(channel);

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
                        _logger.LogInformation("Message received Queue: {QueueName} Consumer: {Consumer}", queueName, consumerType.Name);

                        using var messageScope = _scopeFactory.CreateScope();
                        var scopedConsumer = messageScope.ServiceProvider.GetRequiredService(consumerType);

                        if (scopedConsumer is IMessagingConsumer typedConsumer)
                        {
                            await typedConsumer.ConsumeAsync((CloudEvent<object>)cloudEvent, stoppingToken);
                        }
                    }

                    channel.BasicAck(args.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}", queueName);

                    int retryCount = 0;
                    if (props.Headers != null && props.Headers.TryGetValue("x-retry-count", out var headerValue))
                    {
                        if (headerValue is byte[] bytes &&
                            int.TryParse(Encoding.UTF8.GetString(bytes), out var parsedRetry))
                            retryCount = parsedRetry;
                    }

                    if (retryCount >= maxRetry)
                    {
                        _logger.LogWarning("Tentativas excedidas para mensagem. Enviando para DLQ: {DlqQueue}", dlqQueue);

                        var dlqProps = channel.CreateBasicProperties();
                        dlqProps.Persistent = true;
                        dlqProps.Headers = props.Headers;

                        if (dlqProps.Headers == null)
                            dlqProps.Headers = new Dictionary<string, object>();

                        dlqProps.Headers["x-send-dlq"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));

                        channel.BasicPublish(exchange: dlxExchange, routingKey: "", basicProperties: dlqProps, body: body);
                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    else
                    {
                        retryCount++;

                        var retryProps = channel.CreateBasicProperties();
                        retryProps.Persistent = true;
                        retryProps.Headers = props.Headers ?? new Dictionary<string, object>();
                        retryProps.Headers["x-retry-count"] = Encoding.UTF8.GetBytes(retryCount.ToString());
                        retryProps.Headers["x-last-process"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));

                        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: retryProps, body: body);
                        channel.BasicAck(args.DeliveryTag, false);
                    }

                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: eventConsumer);

        }
    }
}