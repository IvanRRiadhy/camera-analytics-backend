using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Many-to-Many Join Entity linking a Camera Pipeline Assignment with an active Analytics Module.
/// </summary>
public class PipelineModuleAssignment : TenantEntity
{
    public Guid PipelineAssignmentId { get; set; }
    public PipelineAssignment PipelineAssignment { get; set; } = null!;

    public Guid ModuleId { get; set; }
    public AnalyticsModule Module { get; set; } = null!;

    public bool IsEnabled { get; set; } = true;
    public string ParametersJson { get; set; } = "{}";
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
}
