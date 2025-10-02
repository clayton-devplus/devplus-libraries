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

    public async Task<Token> RequestPasswordRecovery(PasswordRecoveryRequestDto passwordRecoveryRequestDto)
    {
        return await _devplusOAuthService.RequestPasswordRecovery(passwordRecoveryRequestDto);
    }
}