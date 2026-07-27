using CCTV.Domain.Shared;

namespace CCTV.Domain.Events;

public record WorkerRegisteredDomainEvent(
    Guid ApplicationId,
    Guid NodeId,
    string Hostname,
    string IpAddress
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record WorkerStatusChangedDomainEvent(
    Guid ApplicationId,
    Guid NodeId,
    string PreviousStatus,
    string NewStatus
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public record EngineInstanceStatusChangedDomainEvent(
    Guid ApplicationId,
    Guid EngineId,
    string Status
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
