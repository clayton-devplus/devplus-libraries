using Devplus.TestApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Devplus.TestApp.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/v1/test-message")]
public class TestMessageController : ControllerBase
{
    private readonly ITestMessageService _testMessageService;
    public TestMessageController(ITestMessageService testMessageService)
    {
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
}