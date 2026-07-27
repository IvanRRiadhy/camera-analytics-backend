using CCTV.Domain.Events;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

public class WorkerNode : TenantEntity
{
    public Guid NodeId { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int MaxCapacityPipelineSlots { get; set; } = 8;
    public int TotalGpuMemoryMb { get; set; } = 16384;
    public int CpuCoreCount { get; set; } = 16;
    public string Status { get; set; } = "Active";
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public ICollection<EngineInstance> Instances { get; set; } = new List<EngineInstance>();

    public void UpdateHeartbeat(string ipAddress, int maxCapacitySlots, int gpuMemoryMb)
    {
        IpAddress = ipAddress;
        MaxCapacityPipelineSlots = maxCapacitySlots;
        TotalGpuMemoryMb = gpuMemoryMb;
        LastSeenAt = DateTime.UtcNow;

        if (Status != "Active")
        {
            var oldStatus = Status;
            Status = "Active";
            AddDomainEvent(new WorkerStatusChangedDomainEvent(ApplicationId, NodeId, oldStatus, "Active"));
        }
    }

    public void MarkOffline()
    {
        if (Status != "Offline")
        {
            var oldStatus = Status;
            Status = "Offline";
            AddDomainEvent(new WorkerStatusChangedDomainEvent(ApplicationId, NodeId, oldStatus, "Offline"));
        }
    }
}
