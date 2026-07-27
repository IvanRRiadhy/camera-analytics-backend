using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

public class AnalyticsSession : TenantEntity
{
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public Guid ConfigurationId { get; set; }
    public AnalyticsConfiguration Configuration { get; set; } = null!;

    public SessionState SessionState { get; set; } = SessionState.Pending;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int TargetFps { get; set; } = 30;
    public long TotalFramesProcessed { get; set; } = 0;
    public string? FailureReason { get; set; }

    public PipelineAssignment? Assignment { get; set; }
}
