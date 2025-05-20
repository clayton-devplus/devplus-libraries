namespace Devplus.Messaging.Configuration;

public class RabbitMqConfig
{
    public string Host { get; set; }
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VHost { get; set; }
    public ushort GlobalPrefetchCount { get; set; } = 3;
    public bool UseGlobalPrefetch { get; set; } = true;
}