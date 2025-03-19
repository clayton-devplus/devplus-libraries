using Devplus.Messaging.Models;

namespace Devplus.Messaging.Interfaces;
public interface IMessagingConsumer
{
    string QueueName { get; } // Nome da fila que este consumidor processa
    Task HandleMessageAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken);
}