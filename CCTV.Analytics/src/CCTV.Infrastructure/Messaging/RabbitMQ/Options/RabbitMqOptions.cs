namespace CCTV.Infrastructure.Messaging.RabbitMQ.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "cctv_vhost";
    public string UserName { get; set; } = "cctv_admin";
    public string Password { get; set; } = "SecureCctvPass123";

    public int PrefetchCount { get; set; } = 50;
    public int ConsumerConcurrency { get; set; } = 5;
    public int PublisherConfirmTimeoutMs { get; set; } = 5000;
    public bool QueueDurability { get; set; } = true;
    public bool PersistentMessages { get; set; } = true;
}
