using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class TrackSummary : TenantEntity
{
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public Guid SessionId { get; set; }
    public AnalyticsSession Session { get; set; } = null!;

    public string ExternalTrackId { get; set; } = string.Empty;
    public string ObjectClass { get; set; } = "Person";
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public decimal DwellDurationSeconds { get; set; }
    public decimal AverageConfidence { get; set; } = 0.85m;
    public string? TrajectoryPathJson { get; set; }
    public decimal? SpeedKmh { get; set; }

    public ICollection<ObjectVisit> Visits { get; set; } = new List<ObjectVisit>();
}
