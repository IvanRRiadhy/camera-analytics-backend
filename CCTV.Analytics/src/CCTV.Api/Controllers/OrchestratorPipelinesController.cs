using CCTV.Api.Security;
using CCTV.Application.Orchestration.DTOs;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/orchestrator")]
[RequirePermission(AppPermissions.Orchestrator.Read)]
public class OrchestratorPipelinesController : ControllerBase
{
    private readonly IPipelineRegistryService _pipelineRegistry;
    private readonly ITenantProvider _tenantProvider;

    public OrchestratorPipelinesController(
        IPipelineRegistryService pipelineRegistry,
        ITenantProvider tenantProvider)
    {
        _pipelineRegistry = pipelineRegistry;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("pipelines")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PipelineAssignmentDto>>>> ListPipelines(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var pipelines = await _pipelineRegistry.ListPipelinesAsync(_tenantProvider.ApplicationId, status, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PipelineAssignmentDto>>.SuccessResponse(pipelines));
    }

    [HttpGet("pipelines/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PipelineAssignmentDto>>> GetPipelineById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipeline = await _pipelineRegistry.GetByIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        if (pipeline == null)
        {
            throw new NotFoundException(ResponseMessages.Pipelines.NotFound);
        }

        return Ok(ApiResponse<PipelineAssignmentDto>.SuccessResponse(pipeline));
    }

    [HttpGet("engines/{id:guid}/pipelines")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PipelineAssignmentDto>>>> GetPipelinesByEngine(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipelines = await _pipelineRegistry.GetByEngineIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PipelineAssignmentDto>>.SuccessResponse(pipelines));
    }

    [HttpGet("cameras/{id:guid}/pipeline")]
    public async Task<ActionResult<ApiResponse<PipelineAssignmentDto>>> GetPipelineByCamera(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipeline = await _pipelineRegistry.GetByCameraIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        if (pipeline == null)
        {
            throw new NotFoundException(ResponseMessages.Pipelines.NotFound);
        }

        return Ok(ApiResponse<PipelineAssignmentDto>.SuccessResponse(pipeline));
    }

    [HttpPost("pipelines/assign")]
    [RequirePermission(AppPermissions.Orchestrator.Write)]
    public async Task<ActionResult<ApiResponse<PipelineAssignmentDto>>> AssignPipeline(
        [FromBody] AssignPipelineRequestDto request,
        CancellationToken cancellationToken)
    {
        var payload = request with { ApplicationId = _tenantProvider.ApplicationId };
        var assignment = await _pipelineRegistry.AssignPipelineAsync(payload, cancellationToken);
        return CreatedAtAction(nameof(GetPipelineById), new { id = assignment.Id },
            ApiResponse<PipelineAssignmentDto>.SuccessResponse(assignment, ResponseMessages.Pipelines.Assigned, 201));
    }

    [HttpPost("pipelines/move")]
    [RequirePermission(AppPermissions.Orchestrator.Write)]
    public async Task<ActionResult<ApiResponse<PipelineAssignmentDto>>> MovePipeline(
        [FromBody] MovePipelineRequestDto request,
        CancellationToken cancellationToken)
    {
        var payload = request with { ApplicationId = _tenantProvider.ApplicationId };
        var assignment = await _pipelineRegistry.MovePipelineAsync(payload, cancellationToken);
        return Ok(ApiResponse<PipelineAssignmentDto>.SuccessResponse(assignment, ResponseMessages.Pipelines.Moved));
    }

    [HttpPost("pipelines/{id:guid}/stop")]
    [RequirePermission(AppPermissions.Orchestrator.Execute)]
    public async Task<ActionResult<ApiResponse<PipelineAssignmentDto>>> StopPipeline(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        var assignment = await _pipelineRegistry.StopPipelineAsync(_tenantProvider.ApplicationId, id, reason, cancellationToken);
        return Ok(ApiResponse<PipelineAssignmentDto>.SuccessResponse(assignment, ResponseMessages.Pipelines.Stopped));
    }

    [HttpGet("pipelines/{id:guid}/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PipelineHistoryDto>>>> GetPipelineHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var history = await _pipelineRegistry.GetPipelineHistoryAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PipelineHistoryDto>>.SuccessResponse(history));
    }
}
