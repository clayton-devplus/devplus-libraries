using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;

namespace Devplus.TestApp.Consumers;
public class TestConsumer : IMessagingConsumer
{
    public string QueueName => "devplus-test-queue";
    private readonly ILogger<TestConsumer> _logger;
    public TestConsumer(ILogger<TestConsumer> logger)
    {
        _logger = logger;
    }
    public Task HandleMessageAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message: {Message}", cloudEvent.Data);
        return Task.CompletedTask;
    }
}