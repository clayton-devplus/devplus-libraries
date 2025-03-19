using System.Text;
using System.Text.Json;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;
using RabbitMQ.Client;

namespace Devplus.Messaging.Services;

public class RabbitMqPublisher : IMessagingPublisher
{
    private readonly IConnection _connection;

    public RabbitMqPublisher(IConnection connection)
    {
        _connection = connection;
    }

    public Task PublishAsync<T>(string queueName, T message, string typeEvent, string source)
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var cloudEvent = new CloudEvent<T>
        {
            SpecVersion = "1.0",
            Id = Guid.NewGuid().ToString(),
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

        channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);

        return Task.CompletedTask;
    }
}