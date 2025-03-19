namespace Devplus.Messaging.Interfaces;
public interface IMessagingPublisher
{
    Task PublishAsync<T>(string queueName, T message, string typeEvent, string source);
}
