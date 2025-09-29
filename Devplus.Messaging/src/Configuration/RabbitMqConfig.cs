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
}