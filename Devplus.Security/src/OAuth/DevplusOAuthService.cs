using Devplus.Security.OAuth.Contracts;
using Devplus.Security.OAuth.Refit;

namespace Devplus.Security.OAuth;

public class DevplusOAuthService : IOAuthService
{
    private readonly IDevplusOAuthService _devplusOAuthService;
    private readonly IDevplusOAuthUserService _devplusOAuthUserService;

    public DevplusOAuthService(IDevplusOAuthService devplusOAuthService,
                               IDevplusOAuthUserService devplusOAuthUserService)
    {
        _devplusOAuthService = devplusOAuthService;
        _devplusOAuthUserService = devplusOAuthUserService;
    }

    public Task<CreateClientAppResponseDto> CreateClientAppUser(CreateClientAppRequestDto request)
    {
        return _devplusOAuthUserService.CreateUser(request);
    }

    public async Task<Token> GetTokenAsync(TokenRequestDto tokenRequest)
    {
        return await _devplusOAuthService.GetTokenAsync(tokenRequest);
    }

    public async Task Logout(RefreshTokenOAuthRequestDto refreshTokenRequest)
    {
        await _devplusOAuthUserService.Logout(refreshTokenRequest);
    }

    public async Task<Token> PasswordReset(PasswordResetRequestDto passwordResetDto)
    {
        return await _devplusOAuthService.PasswordReset(passwordResetDto);
    }

    public async Task<Token> RefreshToken(RefreshTokenOAuthRequestDto refreshTokenRequest)
    {
        return await _devplusOAuthService.RefreshToken(refreshTokenRequest);
    }

    public Task RemoveClientAppUser(RemoveClientAppRequestDto request)
    {
        return _devplusOAuthUserService.RemoveUser(request);
    }

    public Task<List<UsuarioResponseDto>> GetUsersClientApp()
    {
        return _devplusOAuthUserService.GetUsersClientApp();
    }

    public Task<List<SystemClaimResponseDto>> GetClientAppClaims()
    {
        return _devplusOAuthUserService.GetClientAppClaims();
    }

    public Task<List<UsuarioClaimResponseDto>> GetUserAppClaims(long userId)
    {
        return _devplusOAuthUserService.GetUserAppClaims(userId);
    }

    public Task<bool> InsertUserAppClaims(long userId, Guid systemClaimId)
    {
        return _devplusOAuthUserService.InsertUserAppClaims(userId, systemClaimId);
    }

    public Task<bool> RemoveUserAppClaims(long userId, Guid systemClaimId)
    {
        return _devplusOAuthUserService.RemoveUserAppClaims(userId, systemClaimId);
    }

    public Task<List<RoleResponseDto>> GetClientAppRoles()
    {
        return _devplusOAuthUserService.GetClientAppRoles();
    }

    public Task<List<UsuarioRoleResponseDto>> GetUserAppRoles(long userId)
    {
        return _devplusOAuthUserService.GetUserAppRoles(userId);
    }

    public Task<bool> InsertUserAppRoles(long userId, Guid roleId)
    {
        return _devplusOAuthUserService.InsertUserAppRoles(userId, roleId);
    }

    public Task<bool> RemoveUserAppRoles(long userId, Guid roleId)
    {
        return _devplusOAuthUserService.RemoveUserAppRoles(userId, roleId);
    }

    public async Task<Token> RequestPasswordRecovery(PasswordRecoveryRequestDto passwordRecoveryRequestDto)
    {
        return await _devplusOAuthService.RequestPasswordRecovery(passwordRecoveryRequestDto);
    }
}