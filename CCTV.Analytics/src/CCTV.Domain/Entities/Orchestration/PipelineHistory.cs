using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Immutable append-only audit trail logging pipeline state transitions and movement reasons.
/// </summary>
public class PipelineHistory : BaseEntity
{
    public Guid PipelineAssignmentId { get; set; }
    public PipelineAssignment PipelineAssignment { get; set; } = null!;

    public PipelineStatus PreviousStatus { get; set; }
    public PipelineStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public Guid ApplicationId { get; set; }
}
