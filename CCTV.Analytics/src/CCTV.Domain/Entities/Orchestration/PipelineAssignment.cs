using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

public class PipelineAssignment : TenantEntity
{
    public Guid SessionId { get; set; }
    public AnalyticsSession Session { get; set; } = null!;

    public Guid InstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }
    public string AssignmentStatus { get; set; } = "Active";
    public string? PipelineParametersSnapshotJson { get; set; }
}
