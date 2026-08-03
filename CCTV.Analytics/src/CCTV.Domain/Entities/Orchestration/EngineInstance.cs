using CCTV.Domain.Enumerations;
using CCTV.Domain.Events;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Domain Entity representing an active AI Engine runtime instance process.
/// </summary>
public class EngineInstance : TenantEntity
{
    public string InstanceId { get; set; } = string.Empty;
    public Guid WorkerNodeId { get; set; }
    public WorkerNode Node { get; set; } = null!;

    public EngineState State { get; set; } = EngineState.Initializing;
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string OsPlatform { get; set; } = string.Empty;
    public string EngineVersion { get; set; } = "12.6.0";

    public string GpuModel { get; set; } = string.Empty;
    public int TotalVramMb { get; set; }
    public int TotalRamMb { get; set; }

    public string InstalledPluginsJson { get; set; } = "[]";
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;

    // Legacy fields for backward compatibility
    public Guid EngineId { get; set; } = Guid.NewGuid();
    public int ProcessId { get; set; }
    public int Port { get; set; }
    public int GpuDeviceId { get; set; } = 0;
    public int AllocatedGpuMemoryMb { get; set; }
    public int ActivePipelinesCount { get; set; } = 0;
    public string Status { get; set; } = "Initializing";

    public ICollection<EngineConnection> Connections { get; set; } = new List<EngineConnection>();
    public ICollection<PipelineAssignment> Assignments { get; set; } = new List<PipelineAssignment>();
    public ICollection<EngineHeartbeat> Heartbeats { get; set; } = new List<EngineHeartbeat>();
    public ICollection<EngineCapability> Capabilities { get; set; } = new List<EngineCapability>();

    public void MarkOnline(int allocatedGpuMemoryMb)
    {
        AllocatedGpuMemoryMb = allocatedGpuMemoryMb;
        State = EngineState.Online;
        Status = "Online";
        LastHeartbeatUtc = DateTime.UtcNow;
        AddDomainEvent(new EngineInstanceStatusChangedDomainEvent(ApplicationId, EngineId, "Online"));
    }

    public void MarkDegraded()
    {
        State = EngineState.Degraded;
        Status = "Degraded";
    }

    public void MarkOffline()
    {
        State = EngineState.Offline;
        Status = "Offline";
        AddDomainEvent(new EngineInstanceStatusChangedDomainEvent(ApplicationId, EngineId, "Offline"));
    }

    public void IncrementPipelineCount()
    {
        ActivePipelinesCount++;
    }

    public void DecrementPipelineCount()
    {
        if (ActivePipelinesCount > 0)
        {
            ActivePipelinesCount--;
        }
    }
}
