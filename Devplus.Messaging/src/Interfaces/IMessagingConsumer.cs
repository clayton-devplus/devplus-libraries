using Devplus.Messaging.Models;

namespace Devplus.Messaging.Interfaces;
public interface IMessagingConsumer
{
    /// <summary>
    /// Exchange da qual a mensagem será recebida
    /// </summary>
    string ExchangeName { get; }
    /// <summary>
    /// Fila de consumo
    /// </summary>
    string QueueName => $"{ExchangeName.Replace("-exchange", "").Replace(".exchange", "")}-queue";
    /// <summary>
    /// Chave de roteamento para o binding
    /// </summary>
    string RoutingKey => "";
    /// <summary>
    /// Total de tentativas de reprocessamento
    /// </summary>
    int MaxRetry => 5;
    /// <summary>
    /// Quantidade máxima de mensagens simultâneas não processadas (QoS)
    /// </summary>
    ushort PrefetchCount => 3;
    /// <summary>
    /// Método de consumo 
    /// </summary>
    /// <param name="cloudEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken);
}