using Devplus.Security.OAuth.Contracts;
using Devplus.Security.OAuth.Refit;

namespace Devplus.Security.OAuth;

public class DevplusOAuthService : IOAuthService
{
    private readonly IDevplusOAuthLogoutService _devplusOAuthLogoutService;
    private readonly IDevplusOAuthService _devplusOAuthService;

    public DevplusOAuthService(IDevplusOAuthService devplusOAuthService,
                               IDevplusOAuthLogoutService devplusOAuthLogoutService)
    {
        _devplusOAuthService = devplusOAuthService;
        _devplusOAuthLogoutService = devplusOAuthLogoutService;
    }
    public async Task<Token> GetTokenAsync(TokenRequestDto tokenRequest)
    {
        return await _devplusOAuthService.GetTokenAsync(tokenRequest);
    }

    public async Task Logout(RefreshTokenOAuthRequestDto refreshTokenRequest)
    {
        await _devplusOAuthLogoutService.Logout(refreshTokenRequest);
    }

    public async Task<Token> PasswordReset(PasswordResetRequestDto passwordResetDto)
    {
        return await _devplusOAuthService.PasswordReset(passwordResetDto);
    }

    public async Task<Token> RefreshToken(RefreshTokenOAuthRequestDto refreshTokenRequest)
    {
        return await _devplusOAuthService.RefreshToken(refreshTokenRequest);
    }

    public async Task<Token> RequestPasswordRecovery(PasswordRecoveryRequestDto passwordRecoveryRequestDto)
    {
        return await _devplusOAuthService.RequestPasswordRecovery(passwordRecoveryRequestDto);
    }
}