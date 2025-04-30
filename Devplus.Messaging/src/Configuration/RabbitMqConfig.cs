namespace Devplus.Messaging.Configuration;
public class RabbitMqConfig
{
    public string Host { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VHost { get; set; }
    public ushort GlobalPrefetchCount { get; set; } = 3;
    public bool UseGlobalPrefetch { get; set; } = true;
}