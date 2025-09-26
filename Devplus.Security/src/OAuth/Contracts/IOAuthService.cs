
namespace Devplus.Security.OAuth.Contracts;

public interface IOAuthService
{
    Task<Token> GetTokenAsync(TokenRequestDto tokenRequest);
    Task<Token> RefreshToken(RefreshTokenOAuthRequestDto refreshTokenRequest);
    Task<Token> PasswordReset(PasswordResetRequestDto passwordResetDto);
    Task<Token> RequestPasswordRecovery(PasswordRecoveryRequestDto passwordRecoveryRequestDto);
    Task Logout(RefreshTokenOAuthRequestDto refreshTokenRequest);

}