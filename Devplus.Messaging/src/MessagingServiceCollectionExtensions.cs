using Devplus.Messaging.Configuration;
using Devplus.Messaging.Interfaces;
using Devplus.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Devplus.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration, bool RegisterConsumers = true)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMq").Get<RabbitMqConfig>() ?? new RabbitMqConfig();

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfig.Host,
            Port = rabbitMqConfig.Port,
            UserName = rabbitMqConfig.UserName,
            Password = rabbitMqConfig.Password,
            VirtualHost = rabbitMqConfig.VHost
        };


        services.AddSingleton<IConnectionFactory>(factory);
        services.AddSingleton(sp => factory.CreateConnection());
        services.AddSingleton(rabbitMqConfig);

        services.AddScoped<IMessagingPublisher, RabbitMqPublisher>();
        services.AddScoped<IMessagingRedrive, RabbitMqRedriveService>();

        if (RegisterConsumers)
            services.AddHostedService<RabbitMqHostedService>();

        // Aplica filtro de log para suprimir logs da lib de mensageria conforme configuração
        services.AddLogging(builder =>
        {
            builder.AddFilter("Devplus.Messaging", rabbitMqConfig.MinimumLogLevel);
        });

        return services;
    }
}
