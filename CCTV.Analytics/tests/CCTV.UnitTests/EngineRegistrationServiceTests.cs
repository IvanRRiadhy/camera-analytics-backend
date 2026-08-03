using System.Linq.Expressions;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Engine.Services;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;
using NSubstitute;

namespace CCTV.UnitTests;

public class EngineRegistrationServiceTests
{
    [Fact]
    public async Task RegisterOrUpdateEngineAsync_NewEngine_CreatesNodeAndInstance()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var nodeRepo = Substitute.For<IGenericRepository<WorkerNode>>();
        var instanceRepo = Substitute.For<IGenericRepository<EngineInstance>>();

        nodeRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<WorkerNode, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<WorkerNode?>(null));

        instanceRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<EngineInstance, bool>>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<EngineInstance?>(null));

        unitOfWork.Repository<WorkerNode>().Returns(nodeRepo);
        unitOfWork.Repository<EngineInstance>().Returns(instanceRepo);

        var service = new EngineRegistrationService(unitOfWork);

        var request = new EngineRegistrationRequest
        {
            InstanceId = "engine_edge_01",
            WorkerNodeId = Guid.NewGuid(),
            Hostname = "edge-server-01",
            IpAddress = "192.168.1.120",
            OsPlatform = "Linux x86_64",
            EngineVersion = "12.6.0",
            GpuInfo = new GpuInfoDto
            {
                DeviceName = "NVIDIA RTX 4090",
                TotalVramMb = 24576
            },
            CpuInfo = new CpuInfoDto
            {
                TotalRamMb = 65536
            },
            InstalledPlugins = new List<PluginMetadataDto>
            {
                new PluginMetadataDto { Name = "people_counter", Version = "1.0.0" }
            }
        };

        // Act
        await service.RegisterOrUpdateEngineAsync(request);

        // Assert
        await nodeRepo.Received(1).AddAsync(Arg.Any<WorkerNode>(), Arg.Any<CancellationToken>());
        await instanceRepo.Received(1).AddAsync(Arg.Is<EngineInstance>(i => i.InstanceId == "engine_edge_01" && i.State == EngineState.Online), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
