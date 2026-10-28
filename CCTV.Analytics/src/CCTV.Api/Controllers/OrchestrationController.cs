using System.Text.Json;
using CCTV.Api.Security;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/orchestration")]
[RequirePermission(AppPermissions.Orchestrator.Read)]
public class OrchestrationController : ControllerBase
{
    private readonly IRuntimeObservabilityService _observabilityService;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEngineCommandPublisher _commandPublisher;

    public OrchestrationController(
        IRuntimeObservabilityService observabilityService,
        ITenantProvider tenantProvider,
        IEngineCommandPublisher commandPublisher)
    {
        _observabilityService = observabilityService;
        _tenantProvider = tenantProvider;
        _commandPublisher = commandPublisher;
    }

    [HttpGet("nodes")]
    public async Task<ActionResult<ApiResponse<EngineFleetSummaryResponseDto>>> GetEngineNodes(CancellationToken cancellationToken)
    {
        var summary = await _observabilityService.GetEngineNodesAsync(_tenantProvider.ApplicationId, cancellationToken);
        return Ok(ApiResponse<EngineFleetSummaryResponseDto>.SuccessResponse(summary));
    }

    [RequirePermission(AppPermissions.Orchestrator.Write)]
    [HttpPost("nodes/{instanceId}/commands")]
    public async Task<ActionResult<ApiResponse<DispatchCommandResponseDto>>> DispatchNodeCommand(
        string instanceId,
        [FromBody] DispatchCommandRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(instanceId) || request == null || string.IsNullOrWhiteSpace(request.CommandType))
        {
            throw new BusinessException("InstanceId and CommandType are required.");
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

        return Accepted(ApiResponse<DispatchCommandResponseDto>.SuccessResponse(response));
    }
}
