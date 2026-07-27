using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class Alert : TenantEntity
{
    public Guid EventId { get; set; }
    public AnalyticsEvent Event { get; set; } = null!;

    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public Guid? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    public string Title { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;
    public AlertStatus Status { get; set; } = AlertStatus.New;
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }

    public Guid? IncidentId { get; set; }
    public Incident? Incident { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
