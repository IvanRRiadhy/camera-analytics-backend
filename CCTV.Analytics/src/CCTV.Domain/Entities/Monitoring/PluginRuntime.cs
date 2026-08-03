using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Monitoring;

/// <summary>
/// Domain Read Model representing the active runtime telemetry and execution metrics of a single analytics plugin running inside a pipeline.
/// </summary>
public class PluginRuntime : TenantEntity
{
    public Guid PipelineAssignmentId { get; set; }
    public Guid? AnalyticsModuleId { get; set; }

    public string PluginName { get; set; } = string.Empty;
    public string PluginVersion { get; set; } = "1.0.0";
    public string Status { get; set; } = "Running";

    public float CurrentFps { get; set; } = 30.0f;
    public float AverageInferenceTimeMs { get; set; } = 8.5f;

    public long FramesProcessed { get; set; } = 145000;
    public long EventsGenerated { get; set; } = 3200;
    public int Errors { get; set; } = 0;
    public int Warnings { get; set; } = 0;
    public long DroppedFrames { get; set; } = 0;

    public float MemoryUsageMb { get; set; } = 512.0f;
    public float GpuUsagePercent { get; set; } = 18.5f;
    public float CpuUsagePercent { get; set; } = 8.2f;

    public DateTime? LastEventTimestampUtc { get; set; } = DateTime.UtcNow;
}
