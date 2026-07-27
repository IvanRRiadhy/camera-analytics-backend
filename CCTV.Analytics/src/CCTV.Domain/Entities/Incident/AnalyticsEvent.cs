using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class AnalyticsEvent : TenantEntity
{
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public Guid? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    public Guid SessionId { get; set; }
    public AnalyticsSession Session { get; set; } = null!;

    public Guid? TrackSummaryId { get; set; }
    public TrackSummary? TrackSummary { get; set; }

    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public decimal Confidence { get; set; } = 0.90m;
    public string? RawPayloadJson { get; set; }

    public DetectionEvidence? Evidence { get; set; }
    public Alert? Alert { get; set; }
}
