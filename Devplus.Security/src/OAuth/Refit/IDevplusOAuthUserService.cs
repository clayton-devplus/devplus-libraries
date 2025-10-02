using Devplus.Security.OAuth.Contracts;
using Refit;

namespace Devplus.Security.OAuth.Refit;

public interface IDevplusOAuthUserService
{
    [Post("/v1/integration/create-client-app-user")]
    Task<CreateClientAppResponseDto> CreateUser([Body] CreateClientAppRequestDto createUserRequest);

    [Post("/v1/integration/remove-client-app-user")]
    Task RemoveUser([Body] RemoveClientAppRequestDto removeUserRequest);

    [Post("/v1/oauth2/logout")]
    Task Logout([Body] RefreshTokenOAuthRequestDto refreshTokenRequest);

    [Post("/v1/integration/get-client-app-users")]
    Task<UsuarioResponseDto> GetUsersClientApp();

}