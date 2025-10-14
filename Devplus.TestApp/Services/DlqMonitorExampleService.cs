using Devplus.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Devplus.TestApp.Services;

/// <summary>
/// Serviço de exemplo para demonstrar o uso do método GetDeadLetterQueueMessageCountAsync
/// </summary>
public class DlqMonitorExampleService
{
    private readonly IMessagingRedrive _redrive;
    private readonly ILogger<DlqMonitorExampleService> _logger;

    public DlqMonitorExampleService(
        IMessagingRedrive redrive,
        ILogger<DlqMonitorExampleService> logger)
    {
        _redrive = redrive;
        _logger = logger;
    }

    /// <summary>
    /// Exemplo de verificação rápida de DLQ
    /// </summary>
    public async Task<bool> CheckIfDlqHasMessages(string queueName)
    {
        try
        {
            _logger.LogInformation("🔍 Verificando DLQ da fila {QueueName}...", queueName);

            // ⚡ Operação ultra-rápida - apenas consulta metadados
            var count = await _redrive.GetDeadLetterQueueMessageCountAsync(queueName);

            if (count == 0)
            {
                _logger.LogInformation("✅ DLQ {QueueName} está vazia", queueName);
                return false;
            }
            else
            {
                _logger.LogWarning("⚠️  DLQ {QueueName} contém {Count} mensagens", queueName, count);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao verificar DLQ da fila {QueueName}", queueName);
            return false;
        }
    }

    /// <summary>
    /// Exemplo de monitoramento de múltiplas DLQs
    /// </summary>
    public async Task<Dictionary<string, uint>> MonitorMultipleDlqs()
    {
        var queues = new[]
        {
            "test-queue",
            "pedidos-queue",
            "pagamentos-queue"
        };

        var results = new Dictionary<string, uint>();

        _logger.LogInformation("📊 Monitorando {Count} DLQs...", queues.Length);

        foreach (var queue in queues)
        {
            try
            {
                var count = await _redrive.GetDeadLetterQueueMessageCountAsync(queue);
                results[queue] = count;

                var status = count switch
                {
                    0 => "🟢 Saudável",
                    <= 10 => "🟡 Atenção",
                    _ => "🔴 Crítico"
                };

                _logger.LogInformation("{Status} {Queue}: {Count} mensagens",
                    status, queue, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar DLQ {Queue}", queue);
                results[queue] = 0;
            }
        }

        var totalMessages = results.Values.Sum(x => (int)x);
        _logger.LogInformation("📈 Total de mensagens em DLQs: {Total}", totalMessages);

        return results;
    }

    /// <summary>
    /// Exemplo de verificação com threshold de alerta
    /// </summary>
    public async Task<bool> CheckWithAlert(string queueName, uint alertThreshold = 50)
    {
        var count = await _redrive.GetDeadLetterQueueMessageCountAsync(queueName);

        if (count >= alertThreshold)
        {
            _logger.LogError("🚨 ALERTA: DLQ {QueueName} excedeu o limite! {Count}/{Threshold} mensagens",
                queueName, count, alertThreshold);

            // Aqui você pode integrar com sistemas de alerta
            // await SendAlert($"DLQ {queueName} crítica: {count} mensagens");

            return true;
        }

        return false;
    }
}