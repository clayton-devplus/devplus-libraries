using Devplus.Messaging.Models;

namespace Devplus.Messaging.Interfaces;
public interface IMessagingConsumer
{
    /// <summary>
    /// Fila de consumo
    /// </summary>
    string QueueName { get; }
    /// <summary>
    /// Exchange da qual a mensagem será recebida
    /// </summary>
    string ExchangeName { get; }
    /// <summary>
    /// Chave de roteamento para o binding
    /// </summary>
    string RoutingKey { get; }
    /// <summary>
    /// Total de tentativas de reprocessamento
    /// </summary>
    int MaxRetry { get; }
    /// <summary>
    /// Método de consumo 
    /// </summary>
    /// <param name="cloudEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken);
}