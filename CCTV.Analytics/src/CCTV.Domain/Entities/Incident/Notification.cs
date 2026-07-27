using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Incident;

public class Notification : TenantEntity
{
    public Guid AlertId { get; set; }
    public Alert Alert { get; set; } = null!;

    public string Channel { get; set; } = "SignalR";
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}
