using CCTV.Domain.Events;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

public class EngineInstance : TenantEntity
{
    public Guid WorkerNodeId { get; set; }
    public WorkerNode Node { get; set; } = null!;

    public Guid EngineId { get; set; }
    public int ProcessId { get; set; }
    public int Port { get; set; }
    public int GpuDeviceId { get; set; } = 0;
    public int AllocatedGpuMemoryMb { get; set; }
    public int ActivePipelinesCount { get; set; } = 0;
    public string Status { get; set; } = "Ready";

    public ICollection<EngineConnection> Connections { get; set; } = new List<EngineConnection>();
    public ICollection<PipelineAssignment> Assignments { get; set; } = new List<PipelineAssignment>();
    public ICollection<EngineHeartbeat> Heartbeats { get; set; } = new List<EngineHeartbeat>();

    public void MarkOnline(int allocatedGpuMemoryMb)
    {
        AllocatedGpuMemoryMb = allocatedGpuMemoryMb;
        if (Status != "Online")
        {
            Status = "Online";
            AddDomainEvent(new EngineInstanceStatusChangedDomainEvent(ApplicationId, EngineId, "Online"));
        }
    }

    public void MarkOffline()
    {
        if (Status != "Offline")
        {
            Status = "Offline";
            AddDomainEvent(new EngineInstanceStatusChangedDomainEvent(ApplicationId, EngineId, "Offline"));
        }
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
