using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Analytics;

/// <summary>
/// Global System Catalog of available AI Analytics Modules (No ApplicationId).
/// </summary>
public class AnalyticsModule : BaseEntity
{
    public string ModuleCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Vendor { get; set; } = "Antigravity AI";
    public string? Description { get; set; }
    public string? DefaultParametersJson { get; set; }
    public string RequiredMinCudaCapability { get; set; } = "7.5";
    public bool IsGlobalActive { get; set; } = true;

    public ICollection<ModuleEntitlement> Entitlements { get; set; } = new List<ModuleEntitlement>();
}
