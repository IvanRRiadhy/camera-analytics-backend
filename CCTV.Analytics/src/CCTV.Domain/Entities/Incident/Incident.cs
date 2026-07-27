using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class Incident : TenantEntity
{
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentPriority Priority { get; set; } = IncidentPriority.High;
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public string? ResolutionNotes { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<EventArchive> Archives { get; set; } = new List<EventArchive>();
}
