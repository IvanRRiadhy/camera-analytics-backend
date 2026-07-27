using System.Text;
using System.Text.Json;
using CCTV.Application.Abstractions.Messaging;
using CCTV.Infrastructure.Messaging.RabbitMQ.Connection;
using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Publisher;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IRabbitMqPersistentConnection _persistentConnection;
    private readonly RabbitMqOptions _options;

    public RabbitMqPublisher(IRabbitMqPersistentConnection persistentConnection, IOptions<RabbitMqOptions> options)
    {
        _persistentConnection = persistentConnection;
        _options = options.Value;
    }

    public Task PublishAsync<TMessage>(TMessage message, string exchange, string routingKey, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        using var channel = _persistentConnection.CreateModel();
        channel.ConfirmSelect();

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.ContentEncoding = "utf-8";
        properties.DeliveryMode = _options.PersistentMessages ? (byte)2 : (byte)1; // 2 = Persistent
        properties.Timestamp = new global::RabbitMQ.Client.AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        properties.Headers = new Dictionary<string, object>
        {
            { "x-message-version", "1.0.0" },
            { "x-message-type", typeof(TMessage).Name }
        };

        channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body);

        channel.WaitForConfirmsOrDie(TimeSpan.FromMilliseconds(_options.PublisherConfirmTimeoutMs));

        return Task.CompletedTask;
    }
}
