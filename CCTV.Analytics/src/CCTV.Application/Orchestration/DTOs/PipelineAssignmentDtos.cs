using CCTV.Domain.Enumerations;

namespace CCTV.Application.Orchestration.DTOs;

public record PipelineAssignmentDto
{
    public Guid Id { get; init; }
    public string PipelineId { get; init; } = string.Empty;
    public Guid CameraId { get; init; }
    public Guid EngineInstanceId { get; init; }
    public string InstanceIdString { get; init; } = string.Empty;
    public string Hostname { get; init; } = string.Empty;
    public string PipelineName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public List<string> RequiredCapabilities { get; init; } = new();
    public List<string> AnalyticsModules { get; init; } = new();
    public PipelineStatus Status { get; init; }
    public DateTime AssignedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; init; }
    public DateTime? StoppedAtUtc { get; init; }
}

public record AssignPipelineRequestDto
{
    public Guid ApplicationId { get; init; }
    public Guid CameraId { get; init; }
    public Guid EngineInstanceId { get; init; }
    public string PipelineId { get; init; } = string.Empty;
    public string PipelineName { get; init; } = string.Empty;
    public int Priority { get; init; } = 1;
    public List<string> RequiredCapabilities { get; init; } = new();
    public List<string> AnalyticsModules { get; init; } = new();
}

public record MovePipelineRequestDto
{
    public Guid ApplicationId { get; init; }
    public Guid AssignmentId { get; init; }
    public Guid NewEngineInstanceId { get; init; }
    public string? Reason { get; init; }
}

public record PipelineHistoryDto
{
    public Guid Id { get; init; }
    public Guid PipelineAssignmentId { get; init; }
    public PipelineStatus PreviousStatus { get; init; }
    public PipelineStatus NewStatus { get; init; }
    public string? Reason { get; init; }
    public DateTime TimestampUtc { get; init; }
}
