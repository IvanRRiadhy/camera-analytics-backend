using CCTV.Contracts.Common;

namespace CCTV.Contracts.Engine;

public record EngineHeartbeatMessage : BaseMessage
{
    public Guid EngineId { get; init; }
    public decimal CpuUsagePercent { get; init; }
    public decimal GpuUsagePercent { get; init; }
    public int GpuMemoryUsedMb { get; init; }
    public int RamUsedMb { get; init; }
    public decimal InferenceFps { get; init; }
    public int FrameDropCount { get; init; }
    public int AssignedCameraCount { get; init; }
    public string HealthStatus { get; init; } = "Healthy";
}

public record PipelineAssignmentCommand : BaseMessage
{
    public Guid AssignmentId { get; init; }
    public Guid SessionId { get; init; }
    public Guid EngineId { get; init; }
    public Guid CameraId { get; init; }
    public string Action { get; init; } = "START"; // START, STOP, MIGRATE
    public string Priority { get; init; } = "NORMAL"; // LOW, NORMAL, HIGH
}
