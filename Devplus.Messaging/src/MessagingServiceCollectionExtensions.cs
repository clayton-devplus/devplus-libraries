using Devplus.Messaging.Configuration;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Devplus.Messaging;
public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMq").Get<RabbitMqConfig>();

        var factory = new ConnectionFactory
        {
            Uri = new Uri(rabbitMqConfig.Host),
            UserName = rabbitMqConfig.UserName,
            Password = rabbitMqConfig.Password,
            VirtualHost = rabbitMqConfig.VHost
        };


        services.AddSingleton<IConnectionFactory>(factory);
        services.AddSingleton(sp => factory.CreateConnection());
        services.AddScoped<IMessagingPublisher, RabbitMqPublisher>();
        services.AddHostedService<RabbitMqHostedService>();
        return services;
    }
}
