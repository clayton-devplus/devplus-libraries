namespace Devplus.Security.OAuth.Contracts;

public class TokenRequestDto
{
    public string GrantType { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Guid ClientId { get; set; }
    public string ClientSecret { get; set; }
    public Guid? TenantId { get; set; }
    public string? Scope { get; set; }
    public string? Code { get; set; }
    public string? RedirectUri { get; set; }
    public string? CodeVerifier { get; set; }
}
