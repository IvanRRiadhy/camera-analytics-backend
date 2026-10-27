using CCTV.Infrastructure.Messaging.RabbitMQ.Constants;
using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Connection;

public class RabbitMqPersistentConnection : IRabbitMqPersistentConnection
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPersistentConnection>? _logger;
    private readonly object _lockObject = new();
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqPersistentConnection(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqPersistentConnection>? logger = null)
    {
        _options = options.Value;
        _logger = logger;
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

            const int maxAttempts = 5;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
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
                catch (Exception ex)
                {
                    _logger?.LogWarning(
                        ex,
                        "RabbitMQ connection attempt {Attempt}/{MaxAttempts} failed (Host={Host}:{Port}, VHost={VHost}).",
                        attempt, maxAttempts, _options.HostName, _options.Port, _options.VirtualHost);

                    if (attempt < maxAttempts)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                }
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
        string[] topicExchanges =
        {
            RabbitMqTopology.Exchanges.Engine,
            RabbitMqTopology.Exchanges.EngineCommands,
            RabbitMqTopology.Exchanges.EngineRegister,
            RabbitMqTopology.Exchanges.EngineHeartbeat,
            RabbitMqTopology.Exchanges.Analytics,
            RabbitMqTopology.Exchanges.Notification,
            RabbitMqTopology.Exchanges.System,
            RabbitMqTopology.Exchanges.DeadLetter
        };

        foreach (var exchange in topicExchanges)
        {
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: _options.QueueDurability, autoDelete: false);
        }

        // 2. Declare Queues + DLQs & Bindings
        // 2a. Engine Events / Registration Queue
        DeclareQueueWithDlq(
            channel,
            RabbitMqTopology.Queues.EngineEvents,
            RabbitMqTopology.DeadLetterQueues.EngineEventsDlq,
            new[]
            {
                (RabbitMqTopology.Exchanges.Engine, "engine.register"),
                (RabbitMqTopology.Exchanges.Engine, "engine.register.#"),
                (RabbitMqTopology.Exchanges.Engine, "engine.events.#"),
                (RabbitMqTopology.Exchanges.EngineRegister, "#"),
                (RabbitMqTopology.Exchanges.EngineRegister, "engine.register.#")
            });

        // 2b. Engine Telemetry Heartbeat Queue
        DeclareQueueWithDlq(
            channel,
            RabbitMqTopology.Queues.EngineHeartbeat,
            RabbitMqTopology.DeadLetterQueues.EngineHeartbeatDlq,
            new[]
            {
                (RabbitMqTopology.Exchanges.Engine, "engine.heartbeat"),
                (RabbitMqTopology.Exchanges.Engine, "engine.heartbeat.#"),
                (RabbitMqTopology.Exchanges.EngineHeartbeat, "#"),
                (RabbitMqTopology.Exchanges.EngineHeartbeat, "engine.heartbeat.#")
            });

        // 2c. Engine Remote Control Commands Queue
        DeclareQueueWithDlq(
            channel,
            RabbitMqTopology.Queues.EngineCommands,
            RabbitMqTopology.DeadLetterQueues.EngineCommandsDlq,
            new[]
            {
                (RabbitMqTopology.Exchanges.EngineCommands, "engine.commands.#"),
                (RabbitMqTopology.Exchanges.EngineCommands, "camera.assign"),
                (RabbitMqTopology.Exchanges.EngineCommands, "pipeline.assign"),
                (RabbitMqTopology.Exchanges.EngineCommands, "analytics.config"),
                (RabbitMqTopology.Exchanges.Engine, "engine.commands.#")
            });

        // 2d. Analytics Events Queue
        DeclareQueueWithDlq(
            channel,
            RabbitMqTopology.Queues.AnalyticsEvents,
            RabbitMqTopology.DeadLetterQueues.AnalyticsEventsDlq,
            new[]
            {
                (RabbitMqTopology.Exchanges.Analytics, "analytics.event"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.event.#"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.runtime.#"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.runtime.snapshot"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.#"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.track"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.visit"),
                (RabbitMqTopology.Exchanges.Analytics, "analytics.evidence")
            });

        // 2e. Notification Dispatches Queue
        DeclareQueueWithDlq(
            channel,
            RabbitMqTopology.Queues.NotificationDispatches,
            RabbitMqTopology.DeadLetterQueues.NotificationDispatchesDlq,
            new[]
            {
                (RabbitMqTopology.Exchanges.Notification, "notification.send"),
                (RabbitMqTopology.Exchanges.Notification, "notification.send.#")
            });
    }

    private void DeclareQueueWithDlq(
        IModel channel,
        string queueName,
        string dlqName,
        IEnumerable<(string Exchange, string RoutingKey)> bindings)
    {
        // Declare DLQ
        channel.QueueDeclare(queue: dlqName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue: dlqName, exchange: RabbitMqTopology.Exchanges.DeadLetter, routingKey: $"dlq.{queueName}");

        // Declare Main Queue with DLX arguments
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", RabbitMqTopology.Exchanges.DeadLetter },
            { "x-dead-letter-routing-key", $"dlq.{queueName}" }
        };

        channel.QueueDeclare(queue: queueName, durable: _options.QueueDurability, exclusive: false, autoDelete: false, arguments: args);

        foreach (var (exchange, routingKey) in bindings)
        {
            channel.QueueBind(queue: queueName, exchange: exchange, routingKey: routingKey);
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
