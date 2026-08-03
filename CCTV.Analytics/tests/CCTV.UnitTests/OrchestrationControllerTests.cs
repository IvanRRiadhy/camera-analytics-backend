using System.Linq.Expressions;
using CCTV.Api.Controllers;
using CCTV.Application.Common.Interfaces;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CCTV.UnitTests;

public class OrchestrationControllerTests
{
    [Fact]
    public async Task GetEngineNodes_ReturnsFleetSummary()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var instanceRepo = Substitute.For<IGenericRepository<EngineInstance>>();
        var tenantProvider = Substitute.For<ITenantProvider>();
        var commandPublisher = Substitute.For<IEngineCommandPublisher>();

        var appId = Guid.NewGuid();
        tenantProvider.ApplicationId.Returns(appId);

        var instances = new List<EngineInstance>
        {
            new EngineInstance
            {
                ApplicationId = appId,
                InstanceId = "engine_edge_01",
                Hostname = "edge-server-01",
                IpAddress = "192.168.1.120",
                State = EngineState.Online,
                GpuModel = "NVIDIA RTX 4090",
                TotalVramMb = 24576,
                AllocatedGpuMemoryMb = 14000,
                ActivePipelinesCount = 4
            }
        };

        instanceRepo.FindAsync(Arg.Any<Expression<Func<EngineInstance, bool>>>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult<IReadOnlyList<EngineInstance>>(instances));

        unitOfWork.Repository<EngineInstance>().Returns(instanceRepo);

        var controller = new OrchestrationController(unitOfWork, tenantProvider, commandPublisher);

        // Act
        var actionResult = await controller.GetEngineNodes(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var summary = Assert.IsType<EngineFleetSummaryResponseDto>(okResult.Value);
        Assert.Equal(1, summary.TotalNodes);
        Assert.Equal(1, summary.OnlineNodes);
        Assert.Equal("engine_edge_01", summary.Nodes[0].InstanceId);
    }

    [Fact]
    public async Task DispatchNodeCommand_ValidRequest_ReturnsAcceptedAndDispatchesCommand()
    {
        // Arrange
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var tenantProvider = Substitute.For<ITenantProvider>();
        var commandPublisher = Substitute.For<IEngineCommandPublisher>();

        tenantProvider.ApplicationId.Returns(Guid.NewGuid());

        var controller = new OrchestrationController(unitOfWork, tenantProvider, commandPublisher);
        var request = new DispatchCommandRequestDto("START_PIPELINE", "cam_lobby_01", new { fps = 10 });

        // Act
        var actionResult = await controller.DispatchNodeCommand("engine_edge_01", request, CancellationToken.None);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult.Result);
        var response = Assert.IsType<DispatchCommandResponseDto>(acceptedResult.Value);
        Assert.Equal("DISPATCHED", response.Status);
        Assert.NotEmpty(response.CommandId);

        await commandPublisher.Received(1).DispatchCommandAsync(
            Arg.Is<EngineCommandMessage>(c => c.TargetInstanceId == "engine_edge_01" && c.CommandType == "START_PIPELINE"),
            Arg.Any<CancellationToken>()
        );
    }
}
