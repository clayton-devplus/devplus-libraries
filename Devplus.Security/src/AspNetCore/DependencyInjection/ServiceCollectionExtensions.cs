using Devplus.Security.AspNetCore.Services;
using Devplus.Security.OAuth;
using Devplus.Security.OAuth.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Security.OAuth;

namespace Devplus.Security.AspNetCore.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra tudo da lib e adiciona o ApplicationPart para expor o controller.
    /// </summary>
    public static IServiceCollection AddDevplusSecurity(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddHttpContextAccessor();
        services.Configure<OAuthSettings>(cfg.GetSection("OAuthSettings"));

        services.AddDevplusOAuthHttp(cfg); // a extensão do item (2)
        services.AddScoped<IOAuthService, DevplusOAuthService>();
        services.AddScoped<ISecurityService, SecurityService>();

        services.AddControllers()
                .AddApplicationPart(typeof(Security.AspNetCore.Controllers.DevplusSecurityController).Assembly)
                .AddControllersAsServices();

        return services;
    }
}
