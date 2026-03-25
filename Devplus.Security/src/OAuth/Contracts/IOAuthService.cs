
namespace Devplus.Security.OAuth.Contracts;

public interface IOAuthService
{
    Task<Token> GetTokenAsync(TokenRequestDto tokenRequest);
    Task<Token> RefreshToken(RefreshTokenOAuthRequestDto refreshTokenRequest);
    Task<Token> PasswordReset(PasswordResetRequestDto passwordResetDto);
    Task<Token> RequestPasswordRecovery(PasswordRecoveryRequestDto passwordRecoveryRequestDto);
    Task Logout(RefreshTokenOAuthRequestDto refreshTokenRequest);
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
}