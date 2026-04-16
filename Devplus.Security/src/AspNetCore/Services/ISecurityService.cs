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
    Task<Token> RequestPasswordRecovery(string emailOrLogin, Guid? customEmailTemplateId = null);
    Task<Token> ResetPassword(string resetToken, string newPassword);
    Task Logout(string refreshToken);

    Task<CreateClientAppResponseDto> CreateClientAppUser(CreateClientAppRequestDto request);
    Task RemoveClientAppUser(RemoveClientAppRequestDto request);
    Task<List<UsuarioResponseDto>> GetUsersClientApp();

    // Claims
    Task<List<SystemClaimResponseDto>> GetClientAppClaims();
    Task<List<UsuarioClaimResponseDto>> GetUserAppClaims(long userId);
    Task<bool> InsertUserAppClaims(long userId, Guid systemClaimId);
    Task<bool> RemoveUserAppClaims(long userId, Guid systemClaimId);

    // Roles
    Task<List<RoleResponseDto>> GetClientAppRoles();
    Task<List<UsuarioRoleResponseDto>> GetUserAppRoles(long userId);
    Task<bool> InsertUserAppRoles(long userId, Guid roleId);
    Task<bool> RemoveUserAppRoles(long userId, Guid roleId);

    // Self Registration
    Task<SelfRegistrationResponseDto> SelfRegistration(SelfRegistrationRequestDto request);

    void AddClaim(string type, string value);
}
