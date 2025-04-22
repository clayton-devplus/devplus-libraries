using Devplus.Mail.Configuration;
using Devplus.Mail.Interfaces;
using Devplus.Mail.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devplus.Mail;
public static class MailServiceCollectionExtensions
{
    public static IServiceCollection AddMail(this IServiceCollection services, IConfiguration configuration)
    {
        var sendGridConfig = configuration.GetSection("SendGrid").Get<SendGridConfig>();

        if (sendGridConfig == null)
            throw new ArgumentNullException(nameof(sendGridConfig), "SendGrid configuration section is missing.");

        if (string.IsNullOrEmpty(sendGridConfig.ApiKey))
            throw new ArgumentException("SendGrid API Key is required.");
        if (string.IsNullOrEmpty(sendGridConfig.FromEmail))
            throw new ArgumentException("SendGrid From Email is required.");

        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
}
