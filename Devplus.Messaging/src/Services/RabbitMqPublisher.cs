using System.Text;
using System.Text.Json;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Devplus.Messaging.Services;

public class RabbitMqPublisher : IMessagingPublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;
    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        _connection = connection;
    }

    public Task PublishAsync<T>(string exchangeName, T message, string typeEvent,
                                                                string source,
                                                                string messageId = "",
                                                                string routingKey = "")
    {

        if (string.IsNullOrEmpty(messageId))
        {
            messageId = Guid.NewGuid().ToString();
        }


        try
        {
            using var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName,
                                        type: "topic",
                                        durable: true,
                                        autoDelete: false,
                                        arguments: null);

            var cloudEvent = new CloudEvent<T>
            {
                SpecVersion = "1.0",
                Id = messageId.ToString(),
                Source = source,
                Type = typeEvent,
                Time = DateTimeOffset.UtcNow,
                DataContentType = "application/json",
                Data = message
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cloudEvent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            }));

            channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ");
            throw;
        }

        return Task.CompletedTask;
    }
}