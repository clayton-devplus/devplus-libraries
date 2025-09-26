// Security.AspNetCore/DependencyInjection/LegacyJwtAuthenticationExtensions.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Devplus.Security.AspNetCore.DependencyInjection;

public static class LegacyJwtAuthenticationExtensions
{
    /// <summary>
    /// Configura JwtBearer lendo diretamente as chaves planas:
    /// "Jwt:Issuer", "Jwt:Audience", "Jwt:Key".
    /// Mantém compatibilidade com apps existentes e habilita token via querystring em /notifications.
    /// </summary>
    public static IServiceCollection AddDevplusJwtFromLegacyKeys(
        this IServiceCollection services,
        IConfiguration cfg,
        string scheme = JwtBearerDefaults.AuthenticationScheme,
        string queryParamName = "access_token",
        string queryPathPrefix = "/notifications")
    {
        var issuer = cfg["Jwt:Issuer"];
        var audience = cfg["Jwt:Audience"];
        var key = cfg["Jwt:Key"];

        // Fail-fast com mensagem clara se estiver faltando algo
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Config ausente: 'Jwt:Issuer'.");
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Config ausente: 'Jwt:Audience'.");
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Config ausente: 'Jwt:Key'.");

        services.AddAuthentication(scheme)
            .AddJwtBearer(scheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.FromSeconds(60), // opcional: deixa respostas mais “secas”
                    // NameClaimType/RoleClaimType ficam nos padrões (name / role / ClaimTypes.*)
                };

                // Mesmo comportamento que você tinha no OnMessageReceived
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query[queryParamName];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(queryPathPrefix))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
