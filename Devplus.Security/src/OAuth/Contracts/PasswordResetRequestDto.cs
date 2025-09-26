namespace Devplus.Security.OAuth.Contracts;

public class PasswordResetRequestDto
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
