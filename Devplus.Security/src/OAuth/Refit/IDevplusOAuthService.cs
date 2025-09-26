using Devplus.Security.OAuth.Contracts;
using Refit;

namespace Devplus.Security.OAuth.Refit;

public interface IDevplusOAuthService
{
    [Headers("Content-Type: application/x-www-form-urlencoded")]
    [Post("/v1/oauth2/token")]
    Task<Token> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] TokenRequestDto tokenRequest);

    [Post("/v1/oauth2/refresh-token")]
    Task<Token> RefreshToken([Body] RefreshTokenOAuthRequestDto refreshTokenRequest);

    [Post("/v1/oauth2/password-reset")]
    Task<Token> PasswordReset([Body] PasswordResetRequestDto passwordResetDto);

    [Post("/v1/oauth2/password-recovery-request")]
    Task<Token> RequestPasswordRecovery([Body] PasswordRecoveryRequestDto passwordRecoveryRequestDto);

}