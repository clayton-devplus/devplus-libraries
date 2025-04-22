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
        await _emailService.SendEmailAsync();
        return Ok("Message sent");
    }
}