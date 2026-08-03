using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Domain Entity logging periodic 5-second telemetry heartbeats from AI Engine nodes.
/// </summary>
public class EngineHeartbeat : TenantEntity
{
    public Guid InstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public string InstanceIdString { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public long SequenceNumber { get; set; }

    public float CpuUsagePercent { get; set; }
    public float GpuUsagePercent { get; set; }
    public int VramUsedMb { get; set; }
    public float GpuTemperatureC { get; set; }

    public int ActivePipelinesCount { get; set; }
    public string TelemetryJson { get; set; } = "{}";

    // Legacy fields for backward compatibility
    public decimal CpuUsagePercentage { get; set; }
    public decimal GpuUsagePercentage { get; set; }
    public int GpuMemoryUsedMb { get; set; }
    public decimal InferenceFps { get; set; }
    public int FrameDropCount { get; set; }
    public int ActiveStreamCount { get; set; }
}
