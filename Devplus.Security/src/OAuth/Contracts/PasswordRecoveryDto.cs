namespace Devplus.Security.OAuth.Contracts;

public class PasswordRecoveryDto
{
    public string Email { get; set; } = string.Empty;
    public Guid? CustomEmailTemplateId { get; set; } = null;
}
