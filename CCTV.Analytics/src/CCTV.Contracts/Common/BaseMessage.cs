namespace CCTV.Contracts.Common;

public abstract record BaseMessage
{
    public string MessageVersion { get; init; } = "1.0";
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public Guid ApplicationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
