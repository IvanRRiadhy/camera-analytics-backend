using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;

namespace CCTV.UnitTests;

public class EngineInstanceDomainTests
{
    [Fact]
    public void EngineInstance_StateTransitions_UpdateStateAndStatus()
    {
        // Arrange
        var instance = new EngineInstance
        {
            InstanceId = "engine_edge_01",
            Hostname = "edge-server-01",
            IpAddress = "192.168.1.120",
            GpuModel = "NVIDIA RTX 4090",
            TotalVramMb = 24576
        };

        // Initial State
        Assert.Equal(EngineState.Initializing, instance.State);
        Assert.Equal("Initializing", instance.Status);

        // Mark Online
        instance.MarkOnline(14000);
        Assert.Equal(EngineState.Online, instance.State);
        Assert.Equal("Online", instance.Status);
        Assert.Equal(14000, instance.AllocatedGpuMemoryMb);
        Assert.NotEmpty(instance.DomainEvents);

        // Mark Degraded
        instance.MarkDegraded();
        Assert.Equal(EngineState.Degraded, instance.State);
        Assert.Equal("Degraded", instance.Status);

        // Mark Offline
        instance.MarkOffline();
        Assert.Equal(EngineState.Offline, instance.State);
        Assert.Equal("Offline", instance.Status);
    }
}
