using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class ObjectVisit : TenantEntity
{
    public Guid TrackSummaryId { get; set; }
    public TrackSummary TrackSummary { get; set; } = null!;

    public Guid ZoneId { get; set; }
    public Zone Zone { get; set; } = null!;

    public DateTime EntryTime { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; set; }
    public decimal DwellTimeSeconds { get; set; }
    public decimal MaxConfidenceInZone { get; set; }
}
