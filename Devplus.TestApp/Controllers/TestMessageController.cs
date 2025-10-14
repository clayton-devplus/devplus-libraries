using System.Text;
using Devplus.Mail.Enum;
using Devplus.Mail.Interfaces;
using Devplus.Mail.Models;
using Devplus.Security.AspNetCore.Services;
using Devplus.TestApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Devplus.TestApp.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/v1/test-message")]
public class TestMessageController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ISecurityService _securityService;
    private readonly ITestMessageService _testMessageService;
    private readonly ILogger<TestMessageController> _logger;

    public TestMessageController(IEmailService emailService,
                                 ISecurityService securityService,
                                    ITestMessageService testMessageService,
                                 ILogger<TestMessageController> logger)
    {
        _emailService = emailService;
        _securityService = securityService;
        _testMessageService = testMessageService;
        _logger = logger;
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage()
    {

        await _testMessageService.SendMessage();
        return Ok("Mensagem enviada com sucesso");

        // var resp = (new Devplus.Security.OAuth.Contracts.CreateClientAppRequestDto
        // {
        //     NomeCompleto = "Clayton Oliveira",
        //     Email = "coliveira.dba@gmail.com"
        // });
        // try
        // {
        //     var result = await _securityService.CreateClientAppUser(resp);
        //     return Ok(result);
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError(ex, "Erro ao enviar mensagem");
        //     throw;
        // }

    }

    [HttpGet("send-mail")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMail()
    {
        // CSV simples para teste
        var csvContent = "Nome,Email,Idade\nClayton,clayton@devplus.com.br,30\nMaria,maria@devplus.com.br,25";
        var csvBytes = Encoding.UTF8.GetBytes(csvContent);
        var csvAttachment = new EmailAttachment
        {
            FileName = "usuarios.csv",
            MimeType = "text/csv",
            Disposition = "attachment",
            ContentStream = new MemoryStream(csvBytes),
        };

        await _emailService.SendTemplateEmail("clayton@devplus.com.br", "Integração com a API de Pagamento",
                        "Integração com a API de Pagamentos realizada com sucesso", attachments: [csvAttachment]);

        await _emailService.SendTemplateEmail("clayton@devplus.com.br", "Alta Latência da API",
                        "O sistema está com alta latência de comunicação com a API de pagamentos.\n Melhor Verificar",
                        templateType: EmailTemplateType.Warning);

        await _emailService.SendTemplateEmail("clayton@devplus.com.br", "Falha na Comunicação da API",
                         "O sistema falhou em comunicar com a API de pagamento",
                         templateType: EmailTemplateType.Error);
        return Ok();
    }

    [HttpGet("dlq-count/{queueName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDlqCount(string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            return BadRequest("Nome da fila é obrigatório");
        }

        try
        {
            var dlqService = HttpContext.RequestServices.GetService<Devplus.TestApp.Services.DlqMonitorExampleService>();

            if (dlqService == null)
            {
                return StatusCode(500, "Serviço de monitoramento DLQ não configurado");
            }

            var hasMessages = await dlqService.CheckIfDlqHasMessages(queueName);

            return Ok(new
            {
                QueueName = queueName,
                HasMessages = hasMessages,
                CheckedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar DLQ da fila {QueueName}", queueName);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    [HttpGet("dlq-monitor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MonitorAllDlqs()
    {
        try
        {
            var dlqService = HttpContext.RequestServices.GetService<Devplus.TestApp.Services.DlqMonitorExampleService>();

            if (dlqService == null)
            {
                return StatusCode(500, "Serviço de monitoramento DLQ não configurado");
            }

            var results = await dlqService.MonitorMultipleDlqs();

            return Ok(new
            {
                Results = results,
                TotalMessages = results.Values.Sum(x => (int)x),
                CheckedAt = DateTime.UtcNow,
                Status = results.Any(r => r.Value > 0) ? "Warning" : "Healthy"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao monitorar DLQs");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}