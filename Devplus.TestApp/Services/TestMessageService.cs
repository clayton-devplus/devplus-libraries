namespace Devplus.TestApp.Services;

using Devplus.Messaging.Interfaces;
using Devplus.TestApp.Interfaces;

public class TestMessageService : ITestMessageService
{
    private readonly IMessagingPublisher _messagingPublisher;
    public TestMessageService(IMessagingPublisher messagingProducer)
    {
        _messagingPublisher = messagingProducer;
    }
    public async Task SendMessage()
    {
        await _messagingPublisher.PublishAsync(exchangeName: "devplus-test-exchange",
                                                message: "{responseData: \"Test message from TestMessageService\"}",
                                                source: "devplus.test.app",
                                                typeEvent: "test-event");
    }
}