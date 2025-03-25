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
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqHostedService> _logger;

        public RabbitMqHostedService(IServiceProvider serviceProvider, IConnectionFactory connectionFactory, ILogger<RabbitMqHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var consumers = scope.ServiceProvider.GetServices<IMessagingConsumer>().ToList();

            foreach (var consumer in consumers)
            {
                var consumerType = consumer.GetType();
                var queueName = (string)consumerType.GetProperty("QueueName")?.GetValue(consumer);
                var exchangeName = (string)consumerType.GetProperty("ExchangeName")?.GetValue(consumer);
                var routingKey = (string)consumerType.GetProperty("RoutingKey")?.GetValue(consumer);

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

                _channel.ExchangeDeclare(exchange: exchangeName,
                                         type: "topic", // ou outro tipo conforme a necessidade
                                         durable: true,
                                         autoDelete: false,
                                         arguments: null);

                _channel.QueueDeclare(queue: queueName,
                                      durable: true,
                                      exclusive: false,
                                      autoDelete: false);

                _channel.QueueBind(queue: queueName,
                                   exchange: exchangeName,
                                   routingKey: routingKey);

                var eventConsumer = new EventingBasicConsumer(_channel);
                eventConsumer.Received += async (sender, args) =>
                {
                    var body = args.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

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
                            using var messageScope = _serviceProvider.CreateScope();
                            var scopedConsumer = messageScope.ServiceProvider.GetRequiredService(consumerType);

                            var handleMethod = consumerType.GetMethod("HandleMessageAsync");
                            if (handleMethod != null)
                            {
                                await (Task)handleMethod.Invoke(scopedConsumer, new[] { cloudEvent, stoppingToken });
                            }
                        }

                        _channel.BasicAck(args.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem");
                        _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                _channel.BasicConsume(queue: queueName, autoAck: false, consumer: eventConsumer);
            }

            await Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();
            return base.StopAsync(cancellationToken);
        }
    }
}