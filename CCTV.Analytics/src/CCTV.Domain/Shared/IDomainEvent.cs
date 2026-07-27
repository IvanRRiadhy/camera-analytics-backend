namespace CCTV.Domain.Shared;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
