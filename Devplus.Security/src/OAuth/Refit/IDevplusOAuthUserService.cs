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
    Task<List<UsuarioResponseDto>> GetUsersClientApp();

    // Claims
    [Get("/v1/integration/client-app-claims")]
    Task<List<SystemClaimResponseDto>> GetClientAppClaims();

    [Get("/v1/integration/user-app-claims/{userId}")]
    Task<List<UsuarioClaimResponseDto>> GetUserAppClaims(long userId);

    [Post("/v1/integration/user-app-claims/{userId}/{systemClaimId}")]
    Task<bool> InsertUserAppClaims(long userId, Guid systemClaimId);

    [Delete("/v1/integration/user-app-claims/{userId}/{systemClaimId}")]
    Task<bool> RemoveUserAppClaims(long userId, Guid systemClaimId);

    // Roles
    [Get("/v1/integration/client-app-roles")]
    Task<List<RoleResponseDto>> GetClientAppRoles();

    [Get("/v1/integration/user-app-roles/{userId}")]
    Task<List<UsuarioRoleResponseDto>> GetUserAppRoles(long userId);

    [Post("/v1/integration/user-app-roles/{userId}/{roleId}")]
    Task<bool> InsertUserAppRoles(long userId, Guid roleId);

    [Delete("/v1/integration/user-app-roles/{userId}/{roleId}")]
    Task<bool> RemoveUserAppRoles(long userId, Guid roleId);

    // Self Registration
    [Post("/v1/integration/self-registration")]
    Task<SelfRegistrationResponseDto> SelfRegistration([Body] SelfRegistrationRequestDto request);
}