using Devplus.Mail.Interfaces;
using Devplus.Mail.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devplus.Mail;
public static class MailServiceCollectionExtensions
{
    public static IServiceCollection AddMail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
