using Devplus.Messaging.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Devplus.TestApp.Controllers;

[ApiController]
[Route("api/test/redrive")]
public class TestRedriveController : ControllerBase
{
    private readonly IMessagingRedrive _redrive;
    private readonly ILogger<TestRedriveController> _logger;

    public TestRedriveController(IMessagingRedrive redrive, ILogger<TestRedriveController> logger)
    {
        _redrive = redrive;
        _logger = logger;
    }

    /// <summary>
    /// Exemplo de uso: Listar mensagens na DLQ
    /// </summary>
    [HttpGet("dead-letters/{queueName}")]
    public async Task<IActionResult> GetDeadLetters(string queueName)
    {
        try
        {
            var messages = await _redrive.GetDeadLetterMessagesAsync(queueName, 50);
            return Ok(new
            {
                QueueName = queueName,
                DeadLetterCount = messages.Count(),
                Messages = messages.Select(m => new
                {
                    m.MessageId,
                    m.RetryCount,
                    m.LastProcessAttempt,
                    m.SentToDlqAt,
                    m.FailureReason,
                    BodyPreview = m.Body.Length > 200 ? m.Body[..200] + "..." : m.Body
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar DLQ para {QueueName}", queueName);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Exemplo de uso: Redrive de mensagem específica
    /// </summary>
    [HttpPost("redrive/{queueName}/{messageId}")]
    public async Task<IActionResult> RedriveMessage(string queueName, string messageId)
    {
        try
        {
            var success = await _redrive.RedriveMessageAsync(queueName, messageId);

            if (success)
            {
                return Ok($"Mensagem {messageId} reenviada para {queueName}");
            }

            return NotFound($"Mensagem {messageId} não encontrada na DLQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no redrive da mensagem {MessageId}", messageId);
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Exemplo de uso: Redrive de todas as mensagens
    /// </summary>
    [HttpPost("redrive-all/{queueName}")]
    public async Task<IActionResult> RedriveAllMessages(string queueName, [FromQuery] int maxMessages = 100)
    {
        try
        {
            var count = await _redrive.RedriveAllMessagesAsync(queueName, maxMessages);

            return Ok(new
            {
                Message = $"{count} mensagens reenviadas",
                QueueName = queueName,
                Count = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no redrive em lote para {QueueName}", queueName);
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost("purge/{queueName}")]
    public async Task<IActionResult> PurgeDeadLetterQueue(string queueName)
    {
        try
        {
            var count = await _redrive.PurgeDeadLetterQueueAsync(queueName);

            return Ok(new
            {
                Message = $"{count} mensagens purgadas",
                QueueName = queueName,
                Count = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no purge em lote para {QueueName}", queueName);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("purge/{queueName}/{messageId}")]
    public async Task<IActionResult> PurgeDeadLetterMessage(string queueName, string messageId)
    {
        try
        {
            var success = await _redrive.PurgeDeadLetterMessageAsync(queueName, messageId);

            if (success)
            {
                return Ok($"Mensagem {messageId} purgada da DLQ {queueName}");
            }

            return NotFound($"Mensagem {messageId} não encontrada na DLQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no purge da mensagem {MessageId}", messageId);
            return StatusCode(500, ex.Message);
        }
    }


}