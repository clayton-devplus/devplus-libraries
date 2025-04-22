using Devplus.Mail.Enum;
using Devplus.Mail.Interfaces;
using Devplus.TestApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Devplus.TestApp.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/v1/test-message")]
public class TestMessageController : ControllerBase
{
    private readonly ITestMessageService _testMessageService;
    private readonly IEmailService _emailService;
    public TestMessageController(IEmailService emailService, ITestMessageService testMessageService)
    {
        _emailService = emailService;
        _testMessageService = testMessageService;
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage()
    {
        await _testMessageService.SendMessage();
        return Ok("Message sent");
    }
    [HttpGet("send-mail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMail()
    {
        await _emailService.SendTemplateEmail("clayton@devplus.com.br", "Integração com a API de Pagamento",
                        "Integração com a API de Pagamentos realizada com sucesso");

        await _emailService.SendTemplateEmail("clayton@devplus.com.br", "Alta Latência da API",
                        "O sistema está com alta latência de comunicação com a API de pagamentos.\n Melhor Verificar", templateType: EmailTemplateType.Warning);

        await _emailService.SendTemplateEmail("clayton@devplus.com.br", "Falha na Comunicação da API",
                         "O sistema falhou em comunicar com a API de pagamento", templateType: EmailTemplateType.Error);
        return Ok();
    }
}