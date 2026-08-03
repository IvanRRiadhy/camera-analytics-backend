using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Events;

public record PipelineAssignedDomainEvent(
    Guid ApplicationId,
    Guid AssignmentId,
    string PipelineId,
    Guid CameraId,
    Guid InstanceId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PipelineMovedDomainEvent(
    Guid ApplicationId,
    Guid AssignmentId,
    string PipelineId,
    Guid CameraId,
    Guid PreviousInstanceId,
    Guid NewInstanceId
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record PipelineStatusChangedDomainEvent(
    Guid ApplicationId,
    Guid AssignmentId,
    string PipelineId,
    Guid CameraId,
    Guid InstanceId,
    PipelineStatus NewStatus
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
