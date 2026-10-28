using System.Text.Json;
using CCTV.Api.Security;
using CCTV.Application.Common.Interfaces;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/orchestration")]
[RequirePermission(AppPermissions.Orchestrator.Read)]
public class OrchestrationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEngineCommandPublisher _commandPublisher;

    public OrchestrationController(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IEngineCommandPublisher commandPublisher)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _commandPublisher = commandPublisher;
    }

    /// <summary>
    /// GET /api/v1/orchestration/nodes
    /// List all registered AI engine runtime nodes for the current tenant.
    /// </summary>
    [HttpGet("nodes")]
    public async Task<ActionResult<EngineFleetSummaryResponseDto>> GetEngineNodes(CancellationToken cancellationToken)
    {
        var instanceRepo = _unitOfWork.Repository<EngineInstance>();
        var instances = await instanceRepo.FindAsync(
            i => i.ApplicationId == _tenantProvider.ApplicationId,
            cancellationToken);

        var nodeDtos = instances.Select(i => new EngineNodeResponseDto(
            InstanceId: !string.IsNullOrWhiteSpace(i.InstanceId) ? i.InstanceId : i.EngineId.ToString(),
            Hostname: i.Hostname,
            IpAddress: i.IpAddress,
            State: i.State.ToString(),
            GpuModel: i.GpuModel,
            TotalVramMb: i.TotalVramMb,
            UsedVramMb: i.AllocatedGpuMemoryMb,
            GpuUsagePercent: 0.0f,
            ActivePipelines: i.ActivePipelinesCount,
            LastHeartbeatUtc: i.LastHeartbeatUtc
        )).ToList();

        var totalNodes = nodeDtos.Count;
        var onlineNodes = nodeDtos.Count(n => n.State == EngineState.Online.ToString());

        var summary = new EngineFleetSummaryResponseDto(
            TotalNodes: totalNodes,
            OnlineNodes: onlineNodes,
            Nodes: nodeDtos
        );

        return Ok(summary);
    }

    /// <summary>
    /// POST /api/v1/orchestration/nodes/{instanceId}/commands
    /// Dispatch remote control command to targeted AI Engine instance.
    /// </summary>
    [RequirePermission(AppPermissions.Orchestrator.Write)]
    [HttpPost("nodes/{instanceId}/commands")]
    public async Task<ActionResult<DispatchCommandResponseDto>> DispatchNodeCommand(
        string instanceId,
        [FromBody] DispatchCommandRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(instanceId) || request == null || string.IsNullOrWhiteSpace(request.CommandType))
        {
            return BadRequest(new { Message = "InstanceId and CommandType are required." });
        }

        var commandId = $"cmd_{Guid.NewGuid():N}";
        var payloadJson = request.PipelineConfig != null ? JsonSerializer.Serialize(request.PipelineConfig) : "{}";

        var commandMessage = new EngineCommandMessage
        {
            CommandId = commandId,
            TargetInstanceId = instanceId,
            CommandType = request.CommandType.ToUpperInvariant(),
            CameraId = request.CameraId ?? string.Empty,
            PayloadJson = payloadJson,
            DispatchedAtUtc = DateTime.UtcNow,
            ApplicationId = _tenantProvider.ApplicationId
        };

        await _commandPublisher.DispatchCommandAsync(commandMessage, cancellationToken);

        var response = new DispatchCommandResponseDto(
            CommandId: commandId,
            Status: "DISPATCHED",
            Timestamp: DateTime.UtcNow
        );

        return Accepted(response);
    }
}
