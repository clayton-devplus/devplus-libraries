namespace Devplus.Messaging.Models;

/// <summary>
/// Representa uma mensagem na Dead Letter Queue
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// ID único da mensagem
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo da mensagem (JSON)
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Headers da mensagem
    /// </summary>
    public Dictionary<string, object> Headers { get; set; } = new();

    /// <summary>
    /// Exchange de origem
    /// </summary>
    public string OriginalExchange { get; set; } = string.Empty;

    /// <summary>
    /// Routing key original
    /// </summary>
    public string OriginalRoutingKey { get; set; } = string.Empty;

    /// <summary>
    /// Número de tentativas de processamento
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Data/hora da última tentativa de processamento
    /// </summary>
    public DateTime LastProcessAttempt { get; set; }

    /// <summary>
    /// Data/hora em que foi enviada para a DLQ
    /// </summary>
    public DateTime SentToDlqAt { get; set; }

    /// <summary>
    /// Motivo do envio para DLQ
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Exchange da DLQ
    /// </summary>
    public string DlqExchange { get; set; } = string.Empty;

    /// <summary>
    /// Nome da DLQ
    /// </summary>
    public string DlqQueue { get; set; } = string.Empty;
}