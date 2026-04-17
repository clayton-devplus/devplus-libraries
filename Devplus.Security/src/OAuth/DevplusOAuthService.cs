using Devplus.Security.OAuth.Contracts;
using Devplus.Security.OAuth.Exceptions;
using Devplus.Security.OAuth.Refit;
using Refit;

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

    public async Task<CreateClientAppResponseDto> CreateClientAppUser(CreateClientAppRequestDto request)
    {
        try { return await _devplusOAuthUserService.CreateUser(request); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<Token> GetTokenAsync(TokenRequestDto tokenRequest)
    {
        try { return await _devplusOAuthService.GetTokenAsync(tokenRequest); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task Logout(RefreshTokenOAuthRequestDto refreshTokenRequest)
    {
        try { await _devplusOAuthUserService.Logout(refreshTokenRequest); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<Token> PasswordReset(PasswordResetRequestDto passwordResetDto)
    {
        try { return await _devplusOAuthService.PasswordReset(passwordResetDto); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<Token> RefreshToken(RefreshTokenOAuthRequestDto refreshTokenRequest)
    {
        try { return await _devplusOAuthService.RefreshToken(refreshTokenRequest); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task RemoveClientAppUser(RemoveClientAppRequestDto request)
    {
        try { await _devplusOAuthUserService.RemoveUser(request); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<List<UsuarioResponseDto>> GetUsersClientApp()
    {
        try { return await _devplusOAuthUserService.GetUsersClientApp(); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<List<SystemClaimResponseDto>> GetClientAppClaims()
    {
        try { return await _devplusOAuthUserService.GetClientAppClaims(); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<List<UsuarioClaimResponseDto>> GetUserAppClaims(long userId)
    {
        try { return await _devplusOAuthUserService.GetUserAppClaims(userId); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<bool> InsertUserAppClaims(long userId, Guid systemClaimId)
    {
        try { return await _devplusOAuthUserService.InsertUserAppClaims(userId, systemClaimId); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<bool> RemoveUserAppClaims(long userId, Guid systemClaimId)
    {
        try { return await _devplusOAuthUserService.RemoveUserAppClaims(userId, systemClaimId); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<List<RoleResponseDto>> GetClientAppRoles()
    {
        try { return await _devplusOAuthUserService.GetClientAppRoles(); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<List<UsuarioRoleResponseDto>> GetUserAppRoles(long userId)
    {
        try { return await _devplusOAuthUserService.GetUserAppRoles(userId); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<bool> InsertUserAppRoles(long userId, Guid roleId)
    {
        try { return await _devplusOAuthUserService.InsertUserAppRoles(userId, roleId); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<bool> RemoveUserAppRoles(long userId, Guid roleId)
    {
        try { return await _devplusOAuthUserService.RemoveUserAppRoles(userId, roleId); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<Token> RequestPasswordRecovery(PasswordRecoveryRequestDto passwordRecoveryRequestDto)
    {
        try { return await _devplusOAuthService.RequestPasswordRecovery(passwordRecoveryRequestDto); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }

    public async Task<SelfRegistrationResponseDto> SelfRegistration(SelfRegistrationRequestDto request)
    {
        try { return await _devplusOAuthUserService.SelfRegistration(request); }
        catch (ApiException ex) { throw OAuthApiException.FromApiException(ex); }
    }
}