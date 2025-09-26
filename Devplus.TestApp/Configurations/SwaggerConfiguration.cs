using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Devplus.TestApp.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TestApp Libs Devplus - Management Core",
                Version = "v1",
                Description = "Core API TestApp Devplus",
                Contact = new OpenApiContact
                {
                    Name = "Dev+ Consultoria - Arquitetura",
                    Email = "clayton@devplus.com.br"
                },
            });
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token de Autorizacao Dev+ Identity \r\n Entre com 'Bearer' [space] e seu token de autorizacao.\r\n\r\n Exemplo: \"Bearer token\"",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

        });

        return services;
    }
}

