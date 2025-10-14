using Devplus.Messaging.Enum;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;

namespace Devplus.TestApp.Consumers;

public class TestConsumer : IMessagingConsumer
{
    public string ExchangeName => "devplus-test-exchange";

    private readonly ILogger<TestConsumer> _logger; public TestConsumer(ILogger<TestConsumer> logger)
    {
        _logger = logger;
    }
    public Task ConsumeAsync(CloudEvent<object> cloudEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received message: {Message}", cloudEvent.Data);

        throw new Exception("Erro ao processar a mensagem");

        return Task.CompletedTask;
    }
}