using Devplus.Security.OAuth.Contracts;
using Refit;

namespace Devplus.Security.OAuth.Refit;

public interface IDevplusOAuthUserService
{
    [Post("/v1/integration/create-client-app-user")]
    Task CreateUser([Body] CreateClientAppRequestDto createUserRequest);

    [Post("/v1/integration/remove-client-app-user")]
    Task RemoveUser([Body] RemoveClientAppRequestDto removeUserRequest);

}