using Devplus.Security.OAuth.Contracts;

namespace Devplus.Security.AspNetCore.Services;

public interface ISecurityService
{
    Dictionary<string, string> GetUserClaims();
    IEnumerable<string> GetUserRoles();
    Guid GetTenantId();              // via "tenant_id"
    string GetDomain();              // via "app_code"

    Task<Token> AuthorizeUser(UserLoginDto userInfo);
    Task<Token> ExchangeCode(string code, string state);
    Task<Token> RefreshToken(string refreshToken);
    Task<Token> RequestPasswordRecovery(string emailOrLogin);
    Task<Token> ResetPassword(string resetToken, string newPassword);
    Task Logout(string refreshToken);

    Task<CreateClientAppResponseDto> CreateClientAppUser(CreateClientAppRequestDto request);
    Task RemoveClientAppUser(RemoveClientAppRequestDto request);

    void AddClaim(string type, string value);
}
