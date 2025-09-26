using Devplus.Security.OAuth.Contracts;
using Refit;

namespace Devplus.Security.OAuth.Refit;

public interface IDevplusOAuthLogoutService
{
    [Post("/v1/oauth2/logout")]
    Task Logout([Body] RefreshTokenOAuthRequestDto refreshTokenRequest);

}