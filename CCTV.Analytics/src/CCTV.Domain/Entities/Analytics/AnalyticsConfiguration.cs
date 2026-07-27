using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Analytics;

public class AnalyticsConfiguration : TenantEntity
{
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public Guid EntitlementId { get; set; }
    public ModuleEntitlement Entitlement { get; set; } = null!;

    public Guid? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    public decimal SensitivityThreshold { get; set; } = 0.75m;
    public string? ScheduleCron { get; set; }
    public TimeSpan? ActiveTimeStart { get; set; }
    public TimeSpan? ActiveTimeEnd { get; set; }
    public string? ParametersJson { get; set; }
    public bool IsEnabled { get; set; } = true;
}
