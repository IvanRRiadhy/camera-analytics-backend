using CCTV.Contracts.Common;

namespace CCTV.Contracts.Analytics;

public record AnalyticsEventMessage : BaseMessage
{
    public Guid EventId { get; init; }
    public Guid SessionId { get; init; }
    public Guid CameraId { get; init; }
    public Guid? ZoneId { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public DateTime EventTimestamp { get; init; } = DateTime.UtcNow;
    public decimal Confidence { get; init; }
    public string? TrackId { get; init; }
    public string? MetadataJson { get; init; }
}
