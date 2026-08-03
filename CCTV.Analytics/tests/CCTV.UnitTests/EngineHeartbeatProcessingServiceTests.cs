using System.Linq.Expressions;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Engine.Services;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;
using NSubstitute;

namespace CCTV.UnitTests;

public class EngineHeartbeatProcessingServiceTests
{
    [Fact]
    public async Task ProcessHeartbeatAsync_ExistingEngine_LogsHeartbeatAndUpdateLastSeen()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var instanceRepo = Substitute.For<IGenericRepository<EngineInstance>>();
        var heartbeatRepo = Substitute.For<IGenericRepository<EngineHeartbeat>>();

        var existingInstance = new EngineInstance
        {
            InstanceId = "engine_edge_01",
            State = EngineState.Online,
            LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-5)
        };

        instanceRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<EngineInstance, bool>>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<EngineInstance?>(existingInstance));

        unitOfWork.Repository<EngineInstance>().Returns(instanceRepo);
        unitOfWork.Repository<EngineHeartbeat>().Returns(heartbeatRepo);

        var service = new EngineHeartbeatProcessingService(unitOfWork);

        var heartbeat = new EngineHeartbeatMessage
        {
            InstanceId = "engine_edge_01",
            TimestampUtc = DateTime.UtcNow,
            SequenceNumber = 100,
            SystemMetrics = new SystemMetricsDto
            {
                CpuUsagePercent = 45.0f,
                MemoryUsedMb = 8000,
                GpuMetrics = new GpuMetricsDto
                {
                    GpuUsagePercent = 75.0f,
                    VramUsedMb = 12000
                }
            }
        };

        // Act
        await service.ProcessHeartbeatAsync(heartbeat);

        // Assert
        instanceRepo.Received(1).Update(Arg.Is<EngineInstance>(i => i.State == EngineState.Online && i.AllocatedGpuMemoryMb == 12000));
        await heartbeatRepo.Received(1).AddAsync(Arg.Is<EngineHeartbeat>(h => h.SequenceNumber == 100 && h.VramUsedMb == 12000), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EvaluateNodeTimeoutsAsync_StaleNodes_TransitionsToDegradedAndOffline()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var instanceRepo = Substitute.For<IGenericRepository<EngineInstance>>();

        var now = DateTime.UtcNow;

        var onlineNode = new EngineInstance { InstanceId = "node_online", State = EngineState.Online, LastHeartbeatUtc = now };
        var degradedNode = new EngineInstance { InstanceId = "node_degraded", State = EngineState.Online, LastHeartbeatUtc = now.AddSeconds(-20) };
        var offlineNode = new EngineInstance { InstanceId = "node_offline", State = EngineState.Online, LastHeartbeatUtc = now.AddSeconds(-40) };

        var activeNodes = new List<EngineInstance> { onlineNode, degradedNode, offlineNode };

        instanceRepo.FindAsync(Arg.Any<Expression<Func<EngineInstance, bool>>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IReadOnlyList<EngineInstance>>(activeNodes));

        unitOfWork.Repository<EngineInstance>().Returns(instanceRepo);

        var service = new EngineHeartbeatProcessingService(unitOfWork);

        // Act
        await service.EvaluateNodeTimeoutsAsync();

        // Assert
        Assert.Equal(EngineState.Online, onlineNode.State);
        Assert.Equal(EngineState.Degraded, degradedNode.State);
        Assert.Equal(EngineState.Offline, offlineNode.State);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
