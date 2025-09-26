namespace Devplus.Security.OAuth.Contracts;

public class PasswordResetDto
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
