using CCTV.Api.Controllers;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Contracts.Engine;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CCTV.UnitTests;

public class OrchestrationControllerTests
{
    [Fact]
    public async Task GetEngineNodes_ReturnsFleetSummary()
    {
        var observabilityService = Substitute.For<IRuntimeObservabilityService>();
        var tenantProvider = Substitute.For<ITenantProvider>();
        var commandPublisher = Substitute.For<IEngineCommandPublisher>();

        var appId = Guid.NewGuid();
        tenantProvider.ApplicationId.Returns(appId);

        var nodeDtos = new List<EngineNodeResponseDto>
        {
            new(
                InstanceId: "engine_edge_01",
                Hostname: "edge-server-01",
                IpAddress: "192.168.1.120",
                State: "Online",
                GpuModel: "NVIDIA RTX 4090",
                TotalVramMb: 24576,
                UsedVramMb: 14000,
                GpuUsagePercent: 0.0f,
                ActivePipelines: 4,
                LastHeartbeatUtc: DateTime.UtcNow
            )
        };

        var expectedSummary = new EngineFleetSummaryResponseDto(1, 1, nodeDtos);
        observabilityService.GetEngineNodesAsync(appId, Arg.Any<CancellationToken>())
            .Returns(expectedSummary);

        var controller = new OrchestrationController(observabilityService, tenantProvider, commandPublisher);

        var actionResult = await controller.GetEngineNodes(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var envelope = Assert.IsType<ApiResponse<EngineFleetSummaryResponseDto>>(okResult.Value);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        Assert.Equal(1, envelope.Data.TotalNodes);
        Assert.Equal(1, envelope.Data.OnlineNodes);
        Assert.Equal("engine_edge_01", envelope.Data.Nodes[0].InstanceId);
    }

    [Fact]
    public async Task DispatchNodeCommand_ValidRequest_ReturnsAcceptedAndDispatchesCommand()
    {
        var observabilityService = Substitute.For<IRuntimeObservabilityService>();
        var tenantProvider = Substitute.For<ITenantProvider>();
        var commandPublisher = Substitute.For<IEngineCommandPublisher>();

        tenantProvider.ApplicationId.Returns(Guid.NewGuid());

        var controller = new OrchestrationController(observabilityService, tenantProvider, commandPublisher);
        var request = new DispatchCommandRequestDto("START_PIPELINE", "cam_lobby_01", new { fps = 10 });

        var actionResult = await controller.DispatchNodeCommand("engine_edge_01", request, CancellationToken.None);

        var acceptedResult = Assert.IsType<AcceptedResult>(actionResult.Result);
        var envelope = Assert.IsType<ApiResponse<DispatchCommandResponseDto>>(acceptedResult.Value);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        Assert.Equal("DISPATCHED", envelope.Data.Status);
        Assert.NotEmpty(envelope.Data.CommandId);

        await commandPublisher.Received(1).DispatchCommandAsync(
            Arg.Is<EngineCommandMessage>(c => c.TargetInstanceId == "engine_edge_01" && c.CommandType == "START_PIPELINE"),
            Arg.Any<CancellationToken>()
        );
    }
}
