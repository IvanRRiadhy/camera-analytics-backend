using CCTV.Contracts.Engine;
using Microsoft.AspNetCore.SignalR;

namespace CCTV.Infrastructure.SignalR;

/// <summary>
/// Strongly typed SignalR client interface exposing domain events to presentation clients.
/// </summary>
public interface IEngineTelemetryHubClient
{
    Task ReceiveEngineHeartbeat(EngineTelemetryBroadcastingDto telemetry);
    Task TelemetryUpdated(object payload);
    Task HealthChanged(object payload);
    Task EngineOffline(object payload);
    Task EngineRecovered(object payload);
    Task NodeStatusChanged(string instanceId, string status);
    Task EngineRegistered(object payload);
    Task EngineUpdated(object payload);
    Task EngineDisconnected(object payload);
    Task PipelineAssigned(object payload);
    Task PipelineStarted(object payload);
    Task PipelineStopped(object payload);
    Task PipelineMoved(object payload);
    Task PipelineFailed(object payload);
    Task SnapshotUpdated(object payload);
    Task AnalyticsSnapshotPublished(CCTV.Messaging.Contracts.Analytics.CameraAnalyticsSnapshot snapshot);
    Task AnalyticsEventPublished(object payload);
    Task IncidentEscalated(object payload);
}

/// <summary>
/// Real-time SignalR Hub streaming engine telemetry metrics, health status, snapshot notifications, and pipeline events to connected Web Platform clients.
/// Endpoint Path: /hubs/telemetry
/// </summary>
public class EngineTelemetryHub : Hub<IEngineTelemetryHubClient>
{
    public async Task SubscribeToNode(string instanceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"node_{instanceId}");
    }

    public async Task UnsubscribeFromNode(string instanceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"node_{instanceId}");
    }
}
