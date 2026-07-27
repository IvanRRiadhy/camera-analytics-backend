using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Connection;

public class RabbitMqPersistentConnection : IRabbitMqPersistentConnection
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly object _lockObject = new();
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqPersistentConnection(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        _connectionFactory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public bool TryConnect()
    {
        lock (_lockObject)
        {
            if (IsConnected) return true;

            try
            {
                _connection = _connectionFactory.CreateConnection();
                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    InitializeTopology();
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            TryConnect();
        }

        if (!IsConnected || _connection == null)
        {
            throw new InvalidOperationException("No RabbitMQ connection is available to perform this action.");
        }

        return _connection.CreateModel();
    }

    private void InitializeTopology()
    {
        using var channel = _connection!.CreateModel();

        // 1. Declare Topic Exchanges
        string[] topicExchanges = { "cctv.engine", "cctv.analytics", "cctv.notification", "cctv.system", "cctv.deadletter" };
        foreach (var exchange in topicExchanges)
        {
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: _options.QueueDurability, autoDelete: false);
        }

        // 2. Declare 4 Core Queues + DLQs
        DeclareQueueWithDlq(channel, "engine.events.queue", "engine.events.dlq", "cctv.engine", new[] { "engine.register", "engine.heartbeat" });
        DeclareQueueWithDlq(channel, "engine.commands.queue", "engine.commands.dlq", "cctv.engine", new[] { "camera.assign", "pipeline.assign", "analytics.config" });
        DeclareQueueWithDlq(channel, "analytics.events.queue", "analytics.events.dlq", "cctv.analytics", new[] { "analytics.event", "analytics.track", "analytics.visit", "analytics.evidence" });
        DeclareQueueWithDlq(channel, "notification.dispatches.queue", "notification.dispatches.dlq", "cctv.notification", new[] { "notification.send" });
    }

    private void DeclareQueueWithDlq(IModel channel, string queueName, string dlqName, string exchangeName, string[] routingKeys)
    {
        // Declare DLQ
        channel.QueueDeclare(queue: dlqName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: dlqName, exchange: "cctv.deadletter", routingKey: $"dlq.{queueName}");

        // Declare Main Queue with DLX arguments
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "cctv.deadletter" },
            { "x-dead-letter-routing-key", $"dlq.{queueName}" }
        };

        channel.QueueDeclare(queue: queueName, durable: _options.QueueDurability, exclusive: false, autoDelete: false, arguments: args);

        foreach (var key in routingKeys)
        {
            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: key);
        }
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e) { }
    private void OnCallbackException(object? sender, CallbackExceptionEventArgs e) { }
    private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e) { }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            _connection?.Dispose();
        }
        catch { }
    }
}
