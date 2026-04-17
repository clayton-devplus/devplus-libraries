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
}
