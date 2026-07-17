namespace Devplus.Security.OAuth.Contracts;

/// <summary>
/// DTO que representa a resposta de erro padronizada da API OAuth.
/// </summary>
public class OAuthErrorResponse
{
    public string? Type { get; set; }
    public string? Code { get; set; }
    public string? ErrorCode { get; set; }
    public string? Message { get; set; }
    public string? DetailedMessage { get; set; }

    // Presente apenas quando ErrorCode == "MULTIPLE_CLIENT_APPS" (SaaS: usuário em vários tenants).
    // O app usa esta lista para o usuário escolher o tenant e reenviar o login com o TenantId.
    public List<TenantOption>? Tenants { get; set; }
}

public class TenantOption
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? Logo { get; set; }
}
