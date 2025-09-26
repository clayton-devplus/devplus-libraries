using System.Security.Claims;
using Devplus.Security.OAuth;
using Devplus.Security.OAuth.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Security.OAuth;

namespace Devplus.Security.AspNetCore.Services;

public sealed class SecurityService : ISecurityService
{
    private readonly IOptions<OAuthSettings> _opt;
    private readonly IHttpContextAccessor _accessor;
    private readonly IOAuthService _oauth;

    public SecurityService(
        IOptions<OAuthSettings> opt,
        IHttpContextAccessor accessor,
        IOAuthService oauth)
    {
        _opt = opt;
        _accessor = accessor;
        _oauth = oauth;
    }

    public Dictionary<string, string> GetUserClaims()
        => _accessor.HttpContext?.User.Claims?.ToDictionary(c => c.Type, c => c.Value)
           ?? new Dictionary<string, string>();

    public IEnumerable<string> GetUserRoles()
        => (_accessor.HttpContext?.User?.Identity as ClaimsIdentity)?
               .Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
           ?? Enumerable.Empty<string>();

    public Guid GetTenantId()
    {
        var claims = GetUserClaims();
        if (claims.TryGetValue("tenant_id", out var v) && Guid.TryParse(v, out var id))
            return id;
        throw new UnauthorizedAccessException("Tenant ID não encontrado no token JWT.");
    }

    public string GetDomain()
        => GetUserClaims().TryGetValue("app_code", out var v)
           ? v
           : throw new UnauthorizedAccessException("Domain não encontrado no token.");

    public async Task<Token> AuthorizeUser(UserLoginDto user)
    {
        var s = _opt.Value;
        var req = new TokenRequestDto
        {
            ClientId = Guid.Parse(s.ClientId ?? ""),
            ClientSecret = s.ClientSecret ?? "",
            Password = user.Senha ?? string.Empty,
            Username = user.NomeUsuario ?? string.Empty,
            GrantType = "password"
        };
        return await _oauth.GetTokenAsync(req);
    }

    public Task<Token> ExchangeCode(string code, string state)
        => _oauth.GetTokenAsync(new TokenRequestDto { Code = code, GrantType = "authorization_code" });

    public Task<Token> RefreshToken(string refresh)
        => _oauth.RefreshToken(new RefreshTokenOAuthRequestDto { RefreshToken = refresh });

    public Task<Token> RequestPasswordRecovery(string emailOrLogin)
        => _oauth.RequestPasswordRecovery(new PasswordRecoveryRequestDto
        {
            EmailOrLogin = emailOrLogin,
            ResetLink = _opt.Value.PasswordRecoveryRedirectUrl ?? string.Empty
        });

    public Task<Token> ResetPassword(string token, string newPwd)
        => _oauth.PasswordReset(new PasswordResetRequestDto { Token = token, NewPassword = newPwd });

    public Task Logout(string refresh)
        => _oauth.Logout(new RefreshTokenOAuthRequestDto { RefreshToken = refresh });

    public void AddClaim(string type, string value)
        => (_accessor.HttpContext?.User.Identity as ClaimsIdentity)?.AddClaim(new Claim(type, value));

    public Task CreateClientAppUser(CreateClientAppRequestDto request)
    {
        return _oauth.CreateClientAppUser(request);
    }

    public Task RemoveClientAppUser(RemoveClientAppRequestDto request)
    {
        return _oauth.RemoveClientAppUser(request);
    }
}
