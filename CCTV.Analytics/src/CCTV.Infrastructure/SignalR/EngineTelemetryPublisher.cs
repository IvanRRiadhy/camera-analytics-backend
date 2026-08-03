using CCTV.Application.Common.Interfaces;
using CCTV.Contracts.Engine;
using Microsoft.AspNetCore.SignalR;

namespace CCTV.Infrastructure.SignalR;

public class EngineTelemetryPublisher : IEngineTelemetryPublisher
{
    private readonly IHubContext<EngineTelemetryHub, IEngineTelemetryHubClient> _hubContext;

    public EngineTelemetryPublisher(IHubContext<EngineTelemetryHub, IEngineTelemetryHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishTelemetryAsync(EngineTelemetryBroadcastingDto telemetry, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.ReceiveEngineHeartbeat(telemetry);
        await _hubContext.Clients.All.TelemetryUpdated(telemetry);
    }

    public async Task PublishHealthChangedAsync(string instanceId, string previousStatus, string newStatus, int healthScore, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            instanceId,
            previousStatus,
            newStatus,
            healthScore,
            timestampUtc = DateTime.UtcNow
        };
        await _hubContext.Clients.All.HealthChanged(payload);
    }

    public async Task PublishNodeStatusAsync(string instanceId, string status, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.NodeStatusChanged(instanceId, status);
    }

    public async Task PublishEngineOfflineAsync(string instanceId, DateTime timestampUtc, CancellationToken cancellationToken = default)
    {
        var payload = new { instanceId, timestampUtc };
        await _hubContext.Clients.All.EngineOffline(payload);
        await _hubContext.Clients.All.NodeStatusChanged(instanceId, "OFFLINE");
    }

    public async Task PublishEngineRecoveredAsync(string instanceId, DateTime timestampUtc, CancellationToken cancellationToken = default)
    {
        var payload = new { instanceId, timestampUtc };
        await _hubContext.Clients.All.EngineRecovered(payload);
        await _hubContext.Clients.All.NodeStatusChanged(instanceId, "ONLINE");
    }

    public async Task PublishPipelineEventAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        switch (eventName)
        {
            case "PipelineAssigned":
                await _hubContext.Clients.All.PipelineAssigned(payload);
                break;
            case "PipelineStarted":
                await _hubContext.Clients.All.PipelineStarted(payload);
                break;
            case "PipelineStopped":
                await _hubContext.Clients.All.PipelineStopped(payload);
                break;
            case "PipelineMoved":
                await _hubContext.Clients.All.PipelineMoved(payload);
                break;
            case "PipelineFailed":
                await _hubContext.Clients.All.PipelineFailed(payload);
                break;
        }
    }

    public async Task PublishSnapshotUpdatedAsync(string pipelineId, long frameNumber, string overlayMetadataJson, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            pipelineId,
            frameNumber,
            timestampUtc = DateTime.UtcNow,
            overlayMetadata = overlayMetadataJson
        };
        await _hubContext.Clients.All.SnapshotUpdated(payload);
    }

    public async Task PublishAnalyticsSnapshotAsync(CCTV.Messaging.Contracts.Analytics.CameraAnalyticsSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.AnalyticsSnapshotPublished(snapshot);
    }

    public async Task PublishAnalyticsEventAsync(object analyticsEvent, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.AnalyticsEventPublished(analyticsEvent);
    }

    public async Task PublishIncidentEscalatedAsync(object incident, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.IncidentEscalated(incident);
    }
}
