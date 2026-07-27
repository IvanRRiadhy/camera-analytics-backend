using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Analytics;

public class ModuleEntitlement : TenantEntity
{
    public Guid ModuleId { get; set; }
    public AnalyticsModule Module { get; set; } = null!;

    public int MaxAllowedCameraStreams { get; set; } = 10;
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime? ValidTo { get; set; }
    public bool IsSuspended { get; set; } = false;

    public ICollection<AnalyticsConfiguration> Configurations { get; set; } = new List<AnalyticsConfiguration>();
}
