using System.Text.Json;
using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Common.Interfaces;
using CCTV.Infrastructure.Messaging.RabbitMQ.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCTV.Infrastructure.Messaging.BackgroundServices;

public class CameraRuntimeSnapshotMessage
{
    public string EngineId { get; set; } = string.Empty;
    public string CameraId { get; set; } = string.Empty;
    public string CameraName { get; set; } = string.Empty;
    public string StreamUri { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public int PeopleCount { get; set; }
    public int QueueLength { get; set; }
    public int LoiteringCount { get; set; }
    public float PipelineFps { get; set; }
    public float LatencyMs { get; set; }
    public string Health { get; set; } = "OK";
}

public class RuntimeTickConsumerHostedService : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RuntimeTickConsumerHostedService> _logger;

    public RuntimeTickConsumerHostedService(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<RuntimeTickConsumerHostedService> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[+] Starting RuntimeTickConsumerHostedService listening on queue '{QueueName}'...", RabbitMqTopology.Queues.AnalyticsEvents);

        await _messageConsumer.SubscribeAsync<CameraRuntimeSnapshotMessage>(
            queueName: RabbitMqTopology.Queues.AnalyticsEvents,
            messageHandler: async (snapshotMsg, headers) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var telemetryPublisher = scope.ServiceProvider.GetService<IEngineTelemetryPublisher>();

                if (telemetryPublisher != null && snapshotMsg != null)
                {
                    var rawJson = JsonSerializer.Serialize(snapshotMsg);
                    var cameraName = !string.IsNullOrEmpty(snapshotMsg.CameraName) ? snapshotMsg.CameraName :
                                     !string.IsNullOrEmpty(snapshotMsg.CameraId) ? snapshotMsg.CameraId : "camera_01";
                    var pipelineId = !string.IsNullOrEmpty(snapshotMsg.StreamUri) ? snapshotMsg.StreamUri : "pipeline_default";

                    _logger.LogInformation("[-->] Received V2 RuntimeSnapshot from RabbitMQ for Camera '{CameraName}' | Payload Size: {Size} bytes",
                        cameraName, rawJson.Length);

                    // Broadcast raw JSON intact to preserve plugins, summary, byZone, and byCamera
                    await telemetryPublisher.PublishAnalyticsEventAsync(snapshotMsg, stoppingToken);
                    await telemetryPublisher.PublishSnapshotUpdatedAsync(pipelineId, 1, rawJson, stoppingToken);

                    _logger.LogInformation("[<--] Successfully broadcast V2 RuntimeSnapshot over SignalR hub '/hubs/telemetry'.");
                }
            },
            cancellationToken: stoppingToken);
    }
}

