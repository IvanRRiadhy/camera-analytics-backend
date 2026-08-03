using CCTV.Contracts.Engine;

namespace CCTV.Application.Common.Interfaces;

/// <summary>
/// Application Abstraction interface for broadcasting real-time engine telemetry, health updates, snapshots, and analytics events to presentation clients.
/// </summary>
public interface IEngineTelemetryPublisher
{
    Task PublishTelemetryAsync(EngineTelemetryBroadcastingDto telemetry, CancellationToken cancellationToken = default);
    Task PublishHealthChangedAsync(string instanceId, string previousStatus, string newStatus, int healthScore, CancellationToken cancellationToken = default);
    Task PublishNodeStatusAsync(string instanceId, string status, CancellationToken cancellationToken = default);
    Task PublishEngineOfflineAsync(string instanceId, DateTime timestampUtc, CancellationToken cancellationToken = default);
    Task PublishEngineRecoveredAsync(string instanceId, DateTime timestampUtc, CancellationToken cancellationToken = default);
    Task PublishPipelineEventAsync(string eventName, object payload, CancellationToken cancellationToken = default);
    Task PublishSnapshotUpdatedAsync(string pipelineId, long frameNumber, string overlayMetadataJson, CancellationToken cancellationToken = default);
    Task PublishAnalyticsSnapshotAsync(CCTV.Messaging.Contracts.Analytics.CameraAnalyticsSnapshot snapshot, CancellationToken cancellationToken = default);
    Task PublishAnalyticsEventAsync(object analyticsEvent, CancellationToken cancellationToken = default);
    Task PublishIncidentEscalatedAsync(object incident, CancellationToken cancellationToken = default);
}
