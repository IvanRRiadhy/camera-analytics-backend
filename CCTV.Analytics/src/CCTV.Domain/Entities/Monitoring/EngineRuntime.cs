using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Monitoring;

/// <summary>
/// Domain Read Model representing the active runtime state and health metrics of an AI Engine instance.
/// </summary>
public class EngineRuntime : TenantEntity
{
    public Guid EngineInstanceId { get; set; }
    public Guid? WorkerNodeId { get; set; }

    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string EngineVersion { get; set; } = "1.5.0";
    public string CudaVersion { get; set; } = "12.0";
    public string GpuModel { get; set; } = "NVIDIA RTX 4090";
    public string GpuDriver { get; set; } = "535.104";
    public string CpuModel { get; set; } = "Intel Xeon / Core i9";

    public long RamTotalMb { get; set; } = 32768;
    public long RamUsedMb { get; set; } = 8192;
    public long VramTotalMb { get; set; } = 24576;
    public long VramUsedMb { get; set; } = 6144;

    public float CpuUtilization { get; set; } = 15.5f;
    public float GpuUtilization { get; set; } = 42.0f;
    public float DiskUsage { get; set; } = 28.4f;
    public float TemperatureC { get; set; } = 55.0f;

    public float InferenceFps { get; set; } = 29.8f;
    public float AverageLatencyMs { get; set; } = 12.4f;
    public int QueueLength { get; set; } = 2;
    public long DroppedFrames { get; set; } = 0;
    public long ProcessedFrames { get; set; } = 145000;
    public long PublishedEvents { get; set; } = 8900;
    public int ReconnectCount { get; set; } = 0;

    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Online";
}
