using Microsoft.Extensions.Logging;

namespace Devplus.Messaging.Configuration;

public class RabbitMqConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VHost { get; set; } = "/";
    public ushort GlobalPrefetchCount { get; set; } = 0;
    public bool UseGlobalPrefetch { get; set; } = false;

    /// <summary>
    /// Nível mínimo de log para a biblioteca de mensageria.
    /// Use LogLevel.Warning ou LogLevel.Error em produção para evitar excesso de logs.
    /// Use LogLevel.None para desabilitar completamente os logs da lib.
    /// Padrão: LogLevel.Information (comportamento atual).
    /// </summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
}