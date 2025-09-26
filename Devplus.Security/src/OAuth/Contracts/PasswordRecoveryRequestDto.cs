namespace Devplus.Security.OAuth.Contracts;

public class PasswordRecoveryRequestDto
{
    public string EmailOrLogin { get; set; } = string.Empty;
    public string ResetLink { get; set; } = string.Empty;
}
