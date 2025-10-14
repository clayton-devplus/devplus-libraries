using Devplus.Messaging.Models;

namespace Devplus.Messaging.Interfaces;

/// <summary>
/// Interface para funcionalidades de redrive (reenvio) de mensagens
/// </summary>
public interface IMessagingRedrive
{
    /// <summary>
    /// Reenvia uma mensagem específica da DLQ de volta para a fila principal
    /// </summary>
    /// <param name="queueName">Nome da fila principal</param>
    /// <param name="messageId">ID da mensagem a ser reenviada</param>
    /// <returns>True se a mensagem foi reenviada com sucesso</returns>
    Task<bool> RedriveMessageAsync(string queueName, string messageId);

    /// <summary>
    /// Reenvia todas as mensagens da DLQ de volta para a fila principal
    /// </summary>
    /// <param name="queueName">Nome da fila principal</param>
    /// <param name="maxMessages">Número máximo de mensagens a reenviar (0 = todas)</param>
    /// <returns>Número de mensagens reenviadas</returns>
    Task<int> RedriveAllMessagesAsync(string queueName, int maxMessages = 0);

    /// <summary>
    /// Lista mensagens na DLQ para análise antes do redrive
    /// </summary>
    /// <param name="queueName">Nome da fila principal</param>
    /// <param name="maxMessages">Número máximo de mensagens a listar</param>
    /// <returns>Lista de mensagens na DLQ</returns>
    Task<IEnumerable<DeadLetterMessage>> GetDeadLetterMessagesAsync(string queueName, int maxMessages = 100);

    /// <summary>
    /// Remove uma mensagem específica da DLQ permanentemente
    /// </summary>
    /// <param name="queueName">Nome da fila principal</param>
    /// <param name="messageId">ID da mensagem a ser removida</param>
    /// <returns>True se a mensagem foi removida com sucesso</returns>
    Task<bool> PurgeDeadLetterMessageAsync(string queueName, string messageId);

    /// <summary>
    /// Limpa completamente uma DLQ
    /// </summary>
    /// <param name="queueName">Nome da fila principal</param>
    /// <returns>Número de mensagens removidas</returns>
    Task<int> PurgeDeadLetterQueueAsync(string queueName);

    /// <summary>
    /// Verifica se existem mensagens na DLQ de forma extremamente rápida
    /// </summary>
    /// <param name="queueName">Nome da fila principal</param>
    /// <returns>Número de mensagens na DLQ (0 se vazia)</returns>
    Task<uint> GetDeadLetterQueueMessageCountAsync(string queueName);
}