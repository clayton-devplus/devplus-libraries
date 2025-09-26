using Devplus.Security.AspNetCore.Services;
using Devplus.Security.OAuth.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Devplus.Security.AspNetCore.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v1/security")] // ou algo configurável por Options se quiser
public class DevplusSecurityController : ControllerBase
{
    private readonly ISecurityService _security;

    public DevplusSecurityController(ISecurityService security) => _security = security;

    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(Token))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        => Ok(await _security.AuthorizeUser(dto));

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        => Ok(await _security.RefreshToken(dto.RefreshToken));

    [AllowAnonymous]
    [HttpPost("password-recovery-request")]
    public async Task<IActionResult> RequestPasswordRecovery([FromBody] PasswordRecoveryDto dto)
    {
        await _security.RequestPasswordRecovery(dto.Email);
        return Created();
    }

    [AllowAnonymous]
    [HttpPost("password-reset")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto dto)
    {
        await _security.ResetPassword(dto.Token, dto.NewPassword);
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        await _security.Logout(dto.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("get-tenant-info")]
    public IActionResult GetTenantId() => Ok(_security.GetTenantId());

    [AllowAnonymous]
    [HttpPost("exchange-code")]
    public async Task<IActionResult> ExchangeCode([FromBody] ExchangeCodeRequestDto dto)
    {
        var token = await _security.ExchangeCode(dto.Code, dto.State);
        HttpContext.Response.Headers.Add("Bearer", token.AccessToken);
        return Ok(token);
    }
}
