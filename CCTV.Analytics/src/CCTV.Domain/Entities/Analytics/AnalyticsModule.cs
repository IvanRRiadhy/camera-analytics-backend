using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Analytics;

/// <summary>
/// Global System Catalog of available AI Analytics Modules (No ApplicationId).
/// </summary>
public class AnalyticsModule : BaseEntity
{
    public string ModuleCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Vendor { get; set; } = "Bionics AI Platform";
    public string Category { get; set; } = "SAFETY_SECURITY";
    public string PythonEntryPoint { get; set; } = string.Empty;
    public string Icon { get; set; } = "IconShield";
    public string DocumentationUrl { get; set; } = string.Empty;
    public string MinimumCudaCapability { get; set; } = "6.0";
    public bool SupportsRealtime { get; set; } = true;
    public bool SupportsBatch { get; set; } = true;
    public string DefaultConfigurationJson { get; set; } = "{}";
    public string Status { get; set; } = "Active";

    public ICollection<ModuleEntitlement> Entitlements { get; set; } = new List<ModuleEntitlement>();
    public ICollection<PipelineModuleAssignment> PipelineAssignments { get; set; } = new List<PipelineModuleAssignment>();
}
