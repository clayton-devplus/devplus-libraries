using Devplus.Messaging.Models;

namespace Devplus.Messaging.Interfaces;
public interface IMessagingConsumer
{
    string QueueName { get; } // Fila de consumo
    string ExchangeName { get; } // Exchange da qual a mensagem será recebida
    string RoutingKey { get; } // Chave de roteamento para o binding
    Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken);
}