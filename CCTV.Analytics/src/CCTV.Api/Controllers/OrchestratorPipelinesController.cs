using CCTV.Api.Security;
using CCTV.Application.Orchestration.DTOs;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// GET /api/v1/orchestrator/pipelines
    /// List all pipeline assignments for current tenant with optional status filtering.
    /// </summary>
    [HttpGet("pipelines")]
    public async Task<ActionResult<IReadOnlyList<PipelineAssignmentDto>>> ListPipelines(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var pipelines = await _pipelineRegistry.ListPipelinesAsync(_tenantProvider.ApplicationId, status, cancellationToken);
        return Ok(pipelines);
    }

    /// <summary>
    /// GET /api/v1/orchestrator/pipelines/{id}
    /// Get pipeline assignment by ID.
    /// </summary>
    [HttpGet("pipelines/{id:guid}")]
    public async Task<ActionResult<PipelineAssignmentDto>> GetPipelineById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipeline = await _pipelineRegistry.GetByIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        if (pipeline == null) return NotFound(new { Message = $"Pipeline assignment '{id}' not found." });
        return Ok(pipeline);
    }

    /// <summary>
    /// GET /api/v1/orchestrator/engines/{id}/pipelines
    /// List all pipelines assigned to a specific AI Engine Instance.
    /// </summary>
    [HttpGet("engines/{id:guid}/pipelines")]
    public async Task<ActionResult<IReadOnlyList<PipelineAssignmentDto>>> GetPipelinesByEngine(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipelines = await _pipelineRegistry.GetByEngineIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        return Ok(pipelines);
    }

    /// <summary>
    /// GET /api/v1/orchestrator/cameras/{id}/pipeline
    /// Get pipeline assignment for a specific Camera.
    /// </summary>
    [HttpGet("cameras/{id:guid}/pipeline")]
    public async Task<ActionResult<PipelineAssignmentDto>> GetPipelineByCamera(
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipeline = await _pipelineRegistry.GetByCameraIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        if (pipeline == null) return NotFound(new { Message = $"Pipeline assignment for camera '{id}' not found." });
        return Ok(pipeline);
    }

    /// <summary>
    /// POST /api/v1/orchestrator/pipelines/assign
    /// Assign a camera pipeline to an AI Engine Instance in the Registry.
    /// </summary>
    [HttpPost("pipelines/assign")]
    [RequirePermission(AppPermissions.Orchestrator.Write)]
    public async Task<ActionResult<PipelineAssignmentDto>> AssignPipeline(
        [FromBody] AssignPipelineRequestDto request,
        CancellationToken cancellationToken)
    {
        var payload = request with { ApplicationId = _tenantProvider.ApplicationId };
        var assignment = await _pipelineRegistry.AssignPipelineAsync(payload, cancellationToken);
        return CreatedAtAction(nameof(GetPipelineById), new { id = assignment.Id }, assignment);
    }

    /// <summary>
    /// POST /api/v1/orchestrator/pipelines/move
    /// Move/re-assign a pipeline to a new target AI Engine Instance in the Registry.
    /// </summary>
    [HttpPost("pipelines/move")]
    [RequirePermission(AppPermissions.Orchestrator.Write)]
    public async Task<ActionResult<PipelineAssignmentDto>> MovePipeline(
        [FromBody] MovePipelineRequestDto request,
        CancellationToken cancellationToken)
    {
        var payload = request with { ApplicationId = _tenantProvider.ApplicationId };
        var assignment = await _pipelineRegistry.MovePipelineAsync(payload, cancellationToken);
        return Ok(assignment);
    }

    /// <summary>
    /// POST /api/v1/orchestrator/pipelines/{id}/stop
    /// Mark pipeline as Stopped in the Registry.
    /// </summary>
    [HttpPost("pipelines/{id:guid}/stop")]
    [RequirePermission(AppPermissions.Orchestrator.Execute)]
    public async Task<ActionResult<PipelineAssignmentDto>> StopPipeline(
        Guid id,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        var assignment = await _pipelineRegistry.StopPipelineAsync(_tenantProvider.ApplicationId, id, reason, cancellationToken);
        return Ok(assignment);
    }

    /// <summary>
    /// GET /api/v1/orchestrator/pipelines/{id}/history
    /// Get audit history log for a pipeline assignment.
    /// </summary>
    [HttpGet("pipelines/{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<PipelineHistoryDto>>> GetPipelineHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var history = await _pipelineRegistry.GetPipelineHistoryAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        return Ok(history);
    }
}
