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
[Route("api/v1/orchestrator/health")]
[RequirePermission(AppPermissions.Orchestrator.Read)]
public class OrchestratorHealthController : ControllerBase
{
    private readonly IEngineHealthQueryService _healthQueryService;
    private readonly ITenantProvider _tenantProvider;

    public OrchestratorHealthController(
        IEngineHealthQueryService healthQueryService,
        ITenantProvider tenantProvider)
    {
        _healthQueryService = healthQueryService;
        _tenantProvider = tenantProvider;
    }

    [HttpGet("engines")]
    public async Task<ActionResult<ApiResponse<EngineFleetHealthOverviewDto>>> GetFleetHealthOverview(CancellationToken cancellationToken)
    {
        var overview = await _healthQueryService.GetFleetHealthOverviewAsync(_tenantProvider.ApplicationId, cancellationToken);
        return Ok(ApiResponse<EngineFleetHealthOverviewDto>.SuccessResponse(overview));
    }

    [HttpGet("engines/{id:guid}")]
    public async Task<ActionResult<ApiResponse<EngineHealthSummaryDto>>> GetEngineHealth(
        Guid id,
        CancellationToken cancellationToken)
    {
        var health = await _healthQueryService.GetEngineHealthByIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        if (health == null)
        {
            throw new NotFoundException(ResponseMessages.Pipelines.EngineNotFound);
        }

        return Ok(ApiResponse<EngineHealthSummaryDto>.SuccessResponse(health));
    }

    [HttpGet("history/{id:guid}")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EngineHealthHistoryPointDto>>>> GetEngineHealthHistory(
        Guid id,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var history = await _healthQueryService.GetEngineHealthHistoryAsync(_tenantProvider.ApplicationId, id, take, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EngineHealthHistoryPointDto>>.SuccessResponse(history));
    }
}
