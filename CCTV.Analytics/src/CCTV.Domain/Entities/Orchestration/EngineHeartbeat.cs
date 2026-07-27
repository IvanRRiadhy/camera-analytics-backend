using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

public class EngineHeartbeat : TenantEntity
{
    public Guid InstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public decimal CpuUsagePercentage { get; set; }
    public decimal GpuUsagePercentage { get; set; }
    public int GpuMemoryUsedMb { get; set; }
    public decimal InferenceFps { get; set; }
    public int FrameDropCount { get; set; }
    public int ActiveStreamCount { get; set; }
}
