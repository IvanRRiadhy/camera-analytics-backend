using System.Linq.Expressions;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Engine.Services;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;
using NSubstitute;

namespace CCTV.UnitTests;

public class EngineTelemetryHubTests
{
    [Fact]
    public async Task ProcessHeartbeatAsync_WithTelemetryPublisher_PublishesReceiveEngineHeartbeat()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var instanceRepo = Substitute.For<IGenericRepository<EngineInstance>>();
        var heartbeatRepo = Substitute.For<IGenericRepository<EngineHeartbeat>>();

        var existingInstance = new EngineInstance
        {
            InstanceId = "engine_edge_01",
            State = EngineState.Online,
            LastHeartbeatUtc = DateTime.UtcNow
        };

        instanceRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<EngineInstance, bool>>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<EngineInstance?>(existingInstance));

        unitOfWork.Repository<EngineInstance>().Returns(instanceRepo);
        unitOfWork.Repository<EngineHeartbeat>().Returns(heartbeatRepo);

        var publisher = Substitute.For<IEngineTelemetryPublisher>();
        var service = new EngineHeartbeatProcessingService(unitOfWork, publisher);

        var heartbeat = new EngineHeartbeatMessage
        {
            InstanceId = "engine_edge_01",
            TimestampUtc = DateTime.UtcNow,
            SequenceNumber = 200,
            SystemMetrics = new SystemMetricsDto
            {
                CpuUsagePercent = 50.0f,
                MemoryUsedMb = 8000,
                GpuMetrics = new GpuMetricsDto
                {
                    GpuUsagePercent = 80.0f,
                    VramUsedMb = 16000,
                    VramTotalMb = 24576
                }
            }
        };

        // Act
        await service.ProcessHeartbeatAsync(heartbeat);

        // Assert
        await publisher.Received(1).PublishTelemetryAsync(
            Arg.Is<EngineTelemetryBroadcastingDto>(
                t => t.InstanceId == "engine_edge_01" && t.CpuUsagePercent == 50.0f && t.GpuUsagePercent == 80.0f && t.VramUsedMb == 16000
            ),
            Arg.Any<CancellationToken>()
        );
    }
}
