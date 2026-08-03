using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Common.Interfaces;
using CCTV.Infrastructure.Messaging.RabbitMQ.Constants;
using CCTV.Messaging.Contracts.Analytics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCTV.Infrastructure.Messaging.BackgroundServices;

public class AnalyticsSnapshotConsumerHostedService : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsSnapshotConsumerHostedService> _logger;

    public AnalyticsSnapshotConsumerHostedService(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<AnalyticsSnapshotConsumerHostedService> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[+] Starting AnalyticsSnapshotConsumerHostedService listening on queue '{QueueName}'...", RabbitMqTopology.Queues.AnalyticsEvents);

        await _messageConsumer.SubscribeAsync<CameraAnalyticsSnapshot>(
            queueName: RabbitMqTopology.Queues.AnalyticsEvents,
            messageHandler: async (snapshot, headers) =>
            {
                if (snapshot == null) return;

                using var scope = _serviceProvider.CreateScope();
                var telemetryPublisher = scope.ServiceProvider.GetService<IEngineTelemetryPublisher>();

                if (telemetryPublisher != null)
                {
                    var peopleCount = snapshot.Plugins?.PeopleCounting?.Current ?? 0;
                    var queueLength = snapshot.Plugins?.QueueDetection?.QueueLength ?? 0;
                    var loiteringCount = snapshot.Plugins?.Loitering?.Active ?? 0;

                    _logger.LogInformation(
                        "[-->] Received 2s CameraAnalyticsSnapshot (ID: {MessageId}, Version: {SchemaVersion}) from Engine '{EngineId}', Camera '{CameraId}' -> People: {PeopleCount}, Queue: {QueueLength}, Loitering: {LoiteringCount}",
                        snapshot.MessageId, snapshot.SchemaVersion, snapshot.EngineId, snapshot.CameraId, peopleCount, queueLength, loiteringCount);

                    await telemetryPublisher.PublishAnalyticsSnapshotAsync(snapshot, stoppingToken);

                    _logger.LogInformation("[<--] Successfully broadcast 2s CameraAnalyticsSnapshot over SignalR hub '/hubs/telemetry'.");
                }
            },
            cancellationToken: stoppingToken);
    }
}
