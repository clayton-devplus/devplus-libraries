using Devplus.Messaging.Models;

namespace Devplus.Messaging.Interfaces;
public interface IMessagingConsumer
{
    /// <summary>
    /// Exchange da qual a mensagem será recebida
    /// </summary>
    string ExchangeName { get; }
    /// <summary>
    /// Fila de consumo da mensagem. (padrao: $"{ExchangeName.Replace("-exchange", "").Replace(".exchange", "")}-queue")
    /// </summary>
    string QueueName => $"{ExchangeName.Replace("-exchange", "").Replace(".exchange", "")}-queue";
    /// <summary>
    /// Chave de roteamento para o binding. (Ex: "devplus-test-routing-key")
    /// </summary>
    string RoutingKey => "";
    /// <summary>
    /// Total de tentativas de reprocessamento. 
    /// </summary>
    int MaxRetry => 5;
    /// <summary>
    /// Quantidade máxima de mensagens simultâneas não processadas (QoS). (default: 3)
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