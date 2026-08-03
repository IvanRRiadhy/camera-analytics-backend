using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Enumerations;
using CCTV.Domain.Events;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Domain Entity representing a camera pipeline assigned to an AI Engine node.
/// </summary>
public class PipelineAssignment : TenantEntity
{
    public string PipelineId { get; set; } = string.Empty;
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public Guid InstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public string PipelineName { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;

    public List<string> RequiredCapabilities { get; set; } = new();
    public List<string> AnalyticsModules { get; set; } = new();

    public PipelineStatus Status { get; set; } = PipelineStatus.Assigned;

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? StoppedAtUtc { get; set; }

    // Optional runtime session reference
    public Guid? SessionId { get; set; }
    public AnalyticsSession? Session { get; set; }
    public DateTime AssignedAt { get => AssignedAtUtc; set => AssignedAtUtc = value; }
    public DateTime? UnassignedAt { get => StoppedAtUtc; set => StoppedAtUtc = value; }
    public string AssignmentStatus { get => Status.ToString(); set { } }

    public ICollection<PipelineHistory> History { get; set; } = new List<PipelineHistory>();
    public ICollection<PipelineModuleAssignment> ModuleAssignments { get; set; } = new List<PipelineModuleAssignment>();

    public void TransitionState(PipelineStatus newStatus, string? reason = null)
    {
        var oldStatus = Status;
        Status = newStatus;
        History.Add(new PipelineHistory
        {
            ApplicationId = ApplicationId,
            PipelineAssignmentId = Id,
            PreviousStatus = oldStatus,
            NewStatus = newStatus,
            Reason = reason,
            TimestampUtc = DateTime.UtcNow
        });
    }

    public void MarkStarting()
    {
        TransitionState(PipelineStatus.Starting, "Pipeline startup initiated");
    }

    public void MarkRunning()
    {
        StartedAtUtc = DateTime.UtcNow;
        TransitionState(PipelineStatus.Running, "Pipeline execution started");
        AddDomainEvent(new PipelineStatusChangedDomainEvent(ApplicationId, Id, PipelineId, CameraId, InstanceId, PipelineStatus.Running));
    }

    public void MarkStopping()
    {
        TransitionState(PipelineStatus.Stopping, "Pipeline shutdown initiated");
    }

    public void MarkStopped()
    {
        StoppedAtUtc = DateTime.UtcNow;
        TransitionState(PipelineStatus.Stopped, "Pipeline execution stopped");
        AddDomainEvent(new PipelineStatusChangedDomainEvent(ApplicationId, Id, PipelineId, CameraId, InstanceId, PipelineStatus.Stopped));
    }

    public void MarkFailed(string reason)
    {
        TransitionState(PipelineStatus.Failed, reason);
        AddDomainEvent(new PipelineStatusChangedDomainEvent(ApplicationId, Id, PipelineId, CameraId, InstanceId, PipelineStatus.Failed));
    }

    public void Move(Guid newEngineInstanceId, string? reason = null)
    {
        if (Status == PipelineStatus.Running)
        {
            throw new InvalidOperationException("Cannot directly move a running pipeline. Stop the pipeline before re-assigning.");
        }

        var oldInstanceId = InstanceId;
        InstanceId = newEngineInstanceId;
        TransitionState(PipelineStatus.Assigned, reason ?? $"Moved from Engine '{oldInstanceId}' to '{newEngineInstanceId}'");
        AddDomainEvent(new PipelineMovedDomainEvent(ApplicationId, Id, PipelineId, CameraId, oldInstanceId, newEngineInstanceId));
    }
}
