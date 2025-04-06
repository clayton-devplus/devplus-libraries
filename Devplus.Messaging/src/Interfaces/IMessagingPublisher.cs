namespace Devplus.Messaging.Interfaces;
public interface IMessagingPublisher
{
    /// <summary>
    /// Publica uma mensagem no exchange especificado.
    /// </summary>
    /// <typeparam name="T">O tipo da mensagem a ser publicada.</typeparam>
    /// <param name="exchangeName">O nome do exchange onde a mensagem será publicada.</param>
    /// <param name="message">A mensagem a ser publicada.</param>
    /// <param name="typeEvent">O tipo do evento associado à mensagem.</param>
    /// <param name="source">A origem da mensagem.</param>
    /// <param name="messageId">O identificador único da mensagem (opcional).</param>
    /// <param name="routingKey">A chave de roteamento para a mensagem (opcional).</param>
    /// <returns>Uma tarefa que representa a operação assíncrona de publicação.</returns>
    Task PublishAsync<T>(string exchangeName, T message, string typeEvent,
                                                                string source,
                                                                string messageId = "",
                                                                string routingKey = "");
}
