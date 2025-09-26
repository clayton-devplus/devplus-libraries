namespace Security.OAuth;

public sealed class OAuthSettings
{
    public string Url { get; init; } = string.Empty;

    // fluxo password
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }

    // client_credentials p/ chamadas de serviço (delegating handler)
    public string? IdentityClientId { get; init; }
    public string? IdentityClientSecret { get; init; }

    // password recovery
    public string? PasswordRecoveryRedirectUrl { get; init; }
}
