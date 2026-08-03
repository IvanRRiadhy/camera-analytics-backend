using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Domain Entity representing a historical telemetry snapshot and health score for an AI Engine runtime instance.
/// </summary>
public class EngineHealthHistory : TenantEntity
{
    public Guid EngineInstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public float CpuUsagePercent { get; set; }
    public float GpuUsagePercent { get; set; }
    public int VramUsedMb { get; set; }
    public int TotalVramMb { get; set; }
    public float GpuTemperatureC { get; set; }
    public float InferenceFps { get; set; }
    public int FrameDropCount { get; set; }
    public int ActivePipelinesCount { get; set; }

    public int HealthScore { get; set; } = 100;
    public HealthStatus HealthState { get; set; } = HealthStatus.Healthy;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
