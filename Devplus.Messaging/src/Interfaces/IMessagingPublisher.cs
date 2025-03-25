namespace Devplus.Messaging.Interfaces;
public interface IMessagingPublisher
{
    Task PublishAsync<T>(string exchangeName, T message, string typeEvent,
                                                                string source,
                                                                string messageId = "",
                                                                string routingKey = "");
}
