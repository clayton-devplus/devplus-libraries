using Devplus.Security.OAuth.Refit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;
using Security.OAuth;

namespace Devplus.Security.OAuth;

public static class DevplusOAuthServiceCollectionExtensions
{
    public static IServiceCollection AddDevplusOAuthHttp(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddTransient<OAuthAccessTokenHandler>();

        services.AddRefitClient<IDevplusOAuthService>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(configuration["OAuthSettings:Url"] ?? "");
            })
            .AddPolicyHandler(GetRetryPolicy());


        services.AddRefitClient<IDevplusOAuthUserService>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(configuration["OAuthSettings:Url"] ?? "");
            })
            .AddHttpMessageHandler<OAuthAccessTokenHandler>()
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(3));
    }
}

