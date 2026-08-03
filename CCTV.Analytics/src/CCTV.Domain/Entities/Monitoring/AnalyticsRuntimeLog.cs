using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Monitoring;

/// <summary>
/// Domain Entity storing historical 1Hz telemetry snapshots emitted by edge AI engines and pipelines.
/// </summary>
public class AnalyticsRuntimeLog : TenantEntity
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string EngineInstanceId { get; set; } = string.Empty;
    public string CameraId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;

    public string PluginName { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double MetricValue { get; set; }

    public string JsonSnapshot { get; set; } = "{}";
    public DateTime InsertedUtc { get; set; } = DateTime.UtcNow;
}
