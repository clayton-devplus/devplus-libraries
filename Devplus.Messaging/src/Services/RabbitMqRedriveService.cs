using System.Globalization;
using System.Text;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Devplus.Messaging.Services;

/// <summary>
/// Implementação das funcionalidades de redrive para mensagens
/// </summary>
public class RabbitMqRedriveService : IMessagingRedrive
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqRedriveService> _logger;

    public RabbitMqRedriveService(
        IConnection connection,
        ILogger<RabbitMqRedriveService> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<bool> RedriveMessageAsync(string queueName, string messageId)
    {
        using var channel = _connection.CreateModel();
        var dlqQueue = $"{queueName}-dlq";
        var messagesToRequeue = new List<(ulong deliveryTag, byte[] body, IBasicProperties props)>();

        try
        {
            _logger.LogInformation("Iniciando redrive da mensagem {MessageId} da DLQ {DlqQueue} para {QueueName}",
                messageId, dlqQueue, queueName);

            bool messageFound = false;
            BasicGetResult? targetMessage = null;

            // Percorrer todas as mensagens na DLQ até encontrar a específica
            while (true)
            {
                var result = channel.BasicGet(dlqQueue, false);
                if (result == null)
                {
                    break; // Não há mais mensagens na DLQ
                }

                var currentMessageId = GetMessageIdFromHeaders(result.BasicProperties?.Headers);

                if (currentMessageId == messageId)
                {
                    // Mensagem encontrada!
                    targetMessage = result;
                    messageFound = true;
                    _logger.LogDebug("Mensagem {MessageId} encontrada na DLQ {DlqQueue}", messageId, dlqQueue);
                    break;
                }
                else
                {
                    // Não é a mensagem que queremos, armazenar para recolocar na fila
                    messagesToRequeue.Add((result.DeliveryTag, result.Body.ToArray(), result.BasicProperties));
                    _logger.LogDebug("Mensagem {CurrentId} não é a procurada ({RequestedId}), armazenando para requeue",
                        currentMessageId, messageId);
                }
            }

            // Recolocar as mensagens que não eram o target de volta na DLQ
            foreach (var (deliveryTag, body, props) in messagesToRequeue)
            {
                try
                {
                    // Republicar a mensagem na DLQ
                    channel.BasicPublish(exchange: "", routingKey: dlqQueue, basicProperties: props, body: body);

                    // Confirmar remoção da versão original
                    channel.BasicAck(deliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao recolocar mensagem na DLQ {DlqQueue}", dlqQueue);
                    // Em caso de erro, fazer NACK para não perder a mensagem
                    channel.BasicNack(deliveryTag, false, true);
                }
            }

            if (!messageFound || targetMessage == null)
            {
                _logger.LogWarning("Mensagem {MessageId} não encontrada na DLQ {DlqQueue}", messageId, dlqQueue);
                return false;
            }

            // Fazer redrive da mensagem encontrada
            await RedriveMessage(channel, queueName, targetMessage);

            // Confirmar remoção da DLQ
            channel.BasicAck(targetMessage.DeliveryTag, false);

            _logger.LogInformation("Mensagem {MessageId} reenviada com sucesso da DLQ para {QueueName}",
                messageId, queueName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reenviar mensagem {MessageId} da DLQ {DlqQueue}", messageId, dlqQueue);

            // Em caso de erro, tentar fazer NACK das mensagens que ainda não foram processadas
            foreach (var (deliveryTag, _, _) in messagesToRequeue)
            {
                try
                {
                    channel.BasicNack(deliveryTag, false, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            return false;
        }
    }

    public async Task<int> RedriveAllMessagesAsync(string queueName, int maxMessages = 0)
    {
        using var channel = _connection.CreateModel();
        var dlqQueue = $"{queueName}-dlq";
        var redriveCount = 0;

        try
        {
            _logger.LogInformation("Iniciando redrive de todas as mensagens da DLQ {DlqQueue} para {QueueName}. Máximo: {MaxMessages}",
                dlqQueue, queueName, maxMessages == 0 ? "Todas" : maxMessages.ToString());

            while (maxMessages == 0 || redriveCount < maxMessages)
            {
                var result = channel.BasicGet(dlqQueue, false);
                if (result == null)
                {
                    break; // Não há mais mensagens na DLQ
                }

                try
                {
                    // Reenviar para a fila principal
                    await RedriveMessage(channel, queueName, result);

                    // Confirmar remoção da DLQ
                    channel.BasicAck(result.DeliveryTag, false);
                    redriveCount++;

                    _logger.LogDebug("Mensagem {MessageCount} reenviada da DLQ {DlqQueue}", redriveCount, dlqQueue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao reenviar mensagem individual da DLQ {DlqQueue}. Pulando...", dlqQueue);
                    channel.BasicNack(result.DeliveryTag, false, true);
                }
            }

            _logger.LogInformation("Redrive completo: {Count} mensagens reenviadas da DLQ {DlqQueue} para {QueueName}",
                redriveCount, dlqQueue, queueName);
            return redriveCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante redrive em lote da DLQ {DlqQueue}", dlqQueue);
            return redriveCount;
        }
    }

    public async Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName, int maxMessages = 100)
    {
        using var channel = _connection.CreateModel();
        var dlqQueue = $"{queueName}-dlq";
        var messages = new List<DeadLetterMessage>();
        var processedMessages = new List<(ulong deliveryTag, BasicGetResult result)>();

        try
        {
            _logger.LogInformation("Listando mensagens da DLQ {DlqQueue}. Máximo: {MaxMessages}", dlqQueue, maxMessages);

            // Primeira fase: coletar todas as mensagens sem fazer ACK/NACK
            for (int i = 0; i < maxMessages; i++)
            {
                var result = channel.BasicGet(dlqQueue, false);
                if (result == null)
                {
                    break; // Não há mais mensagens
                }

                try
                {
                    var deadLetterMessage = MapToDeadLetterMessage(result, dlqQueue);
                    messages.Add(deadLetterMessage);

                    // Armazenar para fazer NACK depois
                    processedMessages.Add((result.DeliveryTag, result));

                    _logger.LogDebug("Mensagem {MessageId} coletada da DLQ {DlqQueue}", deadLetterMessage.MessageId, dlqQueue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao mapear mensagem da DLQ {DlqQueue}", dlqQueue);
                    // Fazer NACK imediatamente em caso de erro
                    channel.BasicNack(result.DeliveryTag, false, true);
                }
            }

            // Segunda fase: devolver todas as mensagens para a DLQ em ordem reversa
            // (para manter a ordem original)
            for (int i = processedMessages.Count - 1; i >= 0; i--)
            {
                var (deliveryTag, _) = processedMessages[i];
                channel.BasicNack(deliveryTag, false, true);
            }

            _logger.LogInformation("Listadas {Count} mensagens da DLQ {DlqQueue}", messages.Count, dlqQueue);
            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar mensagens da DLQ {DlqQueue}", dlqQueue);

            // Em caso de erro, tentar devolver as mensagens coletadas
            foreach (var (deliveryTag, _) in processedMessages)
            {
                try
                {
                    channel.BasicNack(deliveryTag, false, true);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }

            return messages;
        }
    }

    public async Task<bool> PurgeDeadLetterMessageAsync(string queueName, string messageId)
    {
        using var channel = _connection.CreateModel();
        var dlqQueue = $"{queueName}-dlq";

        try
        {
            _logger.LogInformation("Removendo permanentemente mensagem {MessageId} da DLQ {DlqQueue}", messageId, dlqQueue);

            var result = channel.BasicGet(dlqQueue, false);
            if (result == null)
            {
                _logger.LogWarning("Nenhuma mensagem encontrada na DLQ {DlqQueue}", dlqQueue);
                return false;
            }

            var currentMessageId = GetMessageIdFromHeaders(result.BasicProperties?.Headers);
            if (currentMessageId != messageId)
            {
                // Rejeitar de volta para a fila
                channel.BasicNack(result.DeliveryTag, false, true);
                _logger.LogWarning("Mensagem encontrada ({CurrentId}) não corresponde ao ID solicitado ({RequestedId})",
                    currentMessageId, messageId);
                return false;
            }

            // Confirmar remoção (purge) da mensagem
            channel.BasicAck(result.DeliveryTag, false);

            _logger.LogInformation("Mensagem {MessageId} removida permanentemente da DLQ {DlqQueue}", messageId, dlqQueue);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover mensagem {MessageId} da DLQ {DlqQueue}", messageId, dlqQueue);
            return false;
        }
    }

    public async Task<int> PurgeDeadLetterQueueAsync(string queueName)
    {
        using var channel = _connection.CreateModel();
        var dlqQueue = $"{queueName}-dlq";

        try
        {
            _logger.LogInformation("Limpando completamente a DLQ {DlqQueue}", dlqQueue);

            // Purgar todas as mensagens
            var purgedCount = channel.QueuePurge(dlqQueue);

            _logger.LogInformation("DLQ {DlqQueue} limpa: {Count} mensagens removidas", dlqQueue, purgedCount);
            return (int)purgedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar DLQ {DlqQueue}", dlqQueue);
            return 0;
        }
    }

    public async Task<uint> GetDeadLetterQueueMessageCountAsync(string queueName)
    {
        using var channel = _connection.CreateModel();
        var dlqQueue = $"{queueName}-dlq";

        try
        {
            // QueueDeclarePassive é a operação mais rápida - apenas consulta informações da fila
            // Não cria, não consome, não modifica - apenas verifica o estado atual
            var queueInfo = channel.QueueDeclarePassive(dlqQueue);

            _logger.LogDebug("DLQ {DlqQueue} contém {MessageCount} mensagens", dlqQueue, queueInfo.MessageCount);

            return queueInfo.MessageCount;
        }
        catch (RabbitMQ.Client.Exceptions.OperationInterruptedException ex) when (ex.Message.Contains("NOT_FOUND"))
        {
            // DLQ não existe ainda - isso é normal quando nunca houve mensagens com falha
            _logger.LogDebug(ex, "DLQ {DlqQueue} não existe (nunca houve mensagens com falha)", dlqQueue);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar contagem de mensagens na DLQ {DlqQueue}", dlqQueue);
            return 0;
        }
    }

    #region Private Methods

    private static async Task RedriveMessage(IModel channel, string queueName, BasicGetResult message)
    {
        // Criar novas propriedades removendo metadados da DLQ
        var newProps = channel.CreateBasicProperties();
        newProps.Persistent = true;

        // Copiar headers originais, removendo metadados de DLQ
        if (message.BasicProperties?.Headers != null)
        {
            newProps.Headers = new Dictionary<string, object>();
            foreach (var header in message.BasicProperties.Headers)
            {
                // Não copiar headers relacionados à DLQ
                if (!header.Key.StartsWith("x-death") &&
                    header.Key != "x-send-dlq" &&
                    header.Key != "x-first-death-exchange")
                {
                    newProps.Headers[header.Key] = header.Value;
                }
            }

            // Resetar contador de retry para nova tentativa
            if (newProps.Headers.ContainsKey("x-retry-count"))
            {
                newProps.Headers["x-retry-count"] = Encoding.UTF8.GetBytes("0");
            }

            // Adicionar metadado de redrive
            newProps.Headers["x-redriven-at"] = Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o"));
            newProps.Headers["x-redriven-from-dlq"] = Encoding.UTF8.GetBytes("true");
        }

        // Determinar exchange e routing key originais
        var originalExchange = GetOriginalExchange(message.BasicProperties?.Headers);
        var originalRoutingKey = GetOriginalRoutingKey(message.BasicProperties?.Headers, queueName);

        // Reenviar para a fila principal
        channel.BasicPublish(
            exchange: originalExchange,
            routingKey: originalRoutingKey,
            basicProperties: newProps,
            body: message.Body);

        await Task.CompletedTask;
    }

    private DeadLetterMessage MapToDeadLetterMessage(BasicGetResult result, string dlqQueue)
    {
        var headers = result.BasicProperties?.Headers ?? new Dictionary<string, object>();
        var body = Encoding.UTF8.GetString(result.Body.ToArray());

        return new DeadLetterMessage
        {
            MessageId = GetMessageIdFromHeaders(headers),
            Body = body,
            Headers = new Dictionary<string, object>(headers),
            OriginalExchange = GetOriginalExchange(headers),
            OriginalRoutingKey = GetOriginalRoutingKey(headers, ""),
            RetryCount = GetRetryCount(headers),
            LastProcessAttempt = GetLastProcessAttempt(headers),
            SentToDlqAt = GetSentToDlqAt(headers),
            FailureReason = GetFailureReason(headers),
            DlqExchange = result.Exchange,
            DlqQueue = dlqQueue
        };
    }

    private static string GetMessageIdFromHeaders(IDictionary<string, object>? headers)
    {
        if (headers == null) return Guid.NewGuid().ToString();

        // Tentar extrair o messageId do CloudEvent
        if (headers.TryGetValue("x-message-id", out var messageIdObj) && messageIdObj is byte[] messageIdBytes)
        {
            return Encoding.UTF8.GetString(messageIdBytes);
        }

        // Fallback: usar correlation-id ou gerar novo
        if (headers.TryGetValue("x-correlation-id", out var correlationObj) && correlationObj is byte[] correlationBytes)
        {
            return Encoding.UTF8.GetString(correlationBytes);
        }

        return Guid.NewGuid().ToString();
    }

    private static string GetOriginalExchange(IDictionary<string, object>? headers)
    {
        if (headers?.TryGetValue("x-original-exchange", out var exchangeObj) == true && exchangeObj is byte[] exchangeBytes)
        {
            return Encoding.UTF8.GetString(exchangeBytes);
        }
        return "";
    }

    private static string GetOriginalRoutingKey(IDictionary<string, object>? headers, string fallbackQueue)
    {
        if (headers?.TryGetValue("x-original-routing-key", out var routingObj) == true && routingObj is byte[] routingBytes)
        {
            return Encoding.UTF8.GetString(routingBytes);
        }
        return fallbackQueue;
    }

    private static int GetRetryCount(IDictionary<string, object>? headers)
    {
        if (headers?.TryGetValue("x-retry-count", out var retryObj) == true &&
            retryObj is byte[] retryBytes &&
            int.TryParse(Encoding.UTF8.GetString(retryBytes), out var count))
        {
            return count;
        }
        return 0;
    }

    private static DateTime GetLastProcessAttempt(IDictionary<string, object>? headers)
    {
        if (headers?.TryGetValue("x-last-process", out var processObj) == true &&
            processObj is byte[] processBytes &&
            DateTime.TryParse(Encoding.UTF8.GetString(processBytes), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
        {
            return date;
        }
        return DateTime.MinValue;
    }

    private static DateTime GetSentToDlqAt(IDictionary<string, object>? headers)
    {
        if (headers?.TryGetValue("x-send-dlq", out var dlqObj) == true &&
            dlqObj is byte[] dlqBytes &&
            DateTime.TryParse(Encoding.UTF8.GetString(dlqBytes), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date))
        {
            return date;
        }
        return DateTime.MinValue;
    }

    private static string GetFailureReason(IDictionary<string, object>? headers)
    {
        if (headers?.TryGetValue("x-failure-reason", out var reasonObj) == true && reasonObj is byte[] reasonBytes)
        {
            return Encoding.UTF8.GetString(reasonBytes);
        }
        return "Unknown";
    }

    #endregion
}