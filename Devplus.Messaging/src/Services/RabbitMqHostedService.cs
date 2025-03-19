using System.Text;
using System.Text.Json;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Devplus.Messaging.Services
{
    public class RabbitMqHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqHostedService(IServiceProvider serviceProvider, IConnectionFactory connectionFactory)
        {
            _serviceProvider = serviceProvider;
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

                if (queueName == null)
                {
                    Console.WriteLine($"Consumidor {consumerType.Name} não possui uma fila definida.");
                    continue;
                }

                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

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
                        Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
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