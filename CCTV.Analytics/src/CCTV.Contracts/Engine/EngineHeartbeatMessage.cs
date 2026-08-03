using System.Text.Json.Serialization;
using CCTV.Contracts.Common;

namespace CCTV.Contracts.Engine;

/// <summary>
/// Continuous 5-second heartbeat telemetry message emitted by AI Engine nodes.
/// </summary>
public sealed record EngineHeartbeatMessage : BaseMessage
{
    public string InstanceId { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public long SequenceNumber { get; init; }
    public SystemMetricsDto SystemMetrics { get; init; } = new();
    public List<PipelineTelemetryDto> Pipelines { get; init; } = new();
    public ServiceRuntimeTelemetryDto ServiceRuntime { get; init; } = new();

    // Backward Compatibility Properties for legacy handlers
    [JsonIgnore]
    public Guid EngineId => Guid.TryParse(InstanceId, out var g) ? g : Guid.Empty;

    [JsonIgnore]
    public decimal CpuUsagePercent => (decimal)SystemMetrics.CpuUsagePercent;

    [JsonIgnore]
    public decimal GpuUsagePercent => (decimal)SystemMetrics.GpuMetrics.GpuUsagePercent;

    [JsonIgnore]
    public int GpuMemoryUsedMb => SystemMetrics.GpuMetrics.VramUsedMb;

    [JsonIgnore]
    public int RamUsedMb => SystemMetrics.MemoryUsedMb;

    [JsonIgnore]
    public string HealthStatus => ServiceRuntime.Status;
}

/// <summary>
/// System CPU & Memory Utilization Metrics DTO.
/// </summary>
public sealed record SystemMetricsDto
{
    public float CpuUsagePercent { get; init; }
    public int MemoryUsedMb { get; init; }
    public int MemoryTotalMb { get; init; }
    public GpuMetricsDto GpuMetrics { get; init; } = new();
}

/// <summary>
/// Real-time GPU Hardware Metrics DTO.
/// </summary>
public sealed record GpuMetricsDto
{
    public float GpuUsagePercent { get; init; }
    public int VramUsedMb { get; init; }
    public int VramTotalMb { get; init; }
    public float GpuTemperatureC { get; init; }
    public float PowerDrawWatts { get; init; }
}

/// <summary>
/// Individual Vision Pipeline Telemetry DTO.
/// </summary>
public sealed record PipelineTelemetryDto
{
    public string PipelineId { get; init; } = string.Empty;
    public string CameraId { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public float Fps { get; init; }
    public long FrameCount { get; init; }
    public long DroppedFrames { get; init; }
    public float AvgFrameLatencyMs { get; init; }
    public string RtspStatus { get; init; } = "CONNECTED";
}

/// <summary>
/// Event Service Runtime Health Telemetry DTO.
/// </summary>
public sealed record ServiceRuntimeTelemetryDto
{
    public string Status { get; init; } = "HEALTHY";
    public List<string> RunningServices { get; init; } = new();
    public int ErrorCount { get; init; }
}

/// <summary>
/// Real-time Telemetry Broadcasting DTO sent to SignalR web dashboard clients.
/// </summary>
public sealed record EngineTelemetryBroadcastingDto(
    string InstanceId,
    float CpuUsagePercent,
    float GpuUsagePercent,
    int VramUsedMb,
    int VramTotalMb,
    int ActivePipelines,
    DateTime TimestampUtc
);

/// <summary>
/// Legacy pipeline assignment command contract.
/// </summary>
public record PipelineAssignmentCommand : BaseMessage
{
    public Guid AssignmentId { get; init; }
    public Guid SessionId { get; init; }
    public Guid EngineId { get; init; }
    public Guid CameraId { get; init; }
    public string Action { get; init; } = "START";
    public string Priority { get; init; } = "NORMAL";
}
