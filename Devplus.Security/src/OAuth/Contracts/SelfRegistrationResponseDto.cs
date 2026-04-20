namespace Devplus.Security.OAuth.Contracts;

public class SelfRegistrationResponseDto
{
    public long UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public Guid ClientAppId { get; set; }
    public Guid? CustomTemplateId { get; set; }
}
