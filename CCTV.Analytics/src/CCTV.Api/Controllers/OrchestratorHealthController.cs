using CCTV.Api.Security;
using CCTV.Application.Orchestration.DTOs;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    /// <summary>
    /// GET /api/v1/orchestrator/health/engines
    /// Fleet health overview with score averages and status breakdowns.
    /// </summary>
    [HttpGet("engines")]
    public async Task<ActionResult<EngineFleetHealthOverviewDto>> GetFleetHealthOverview(CancellationToken cancellationToken)
    {
        var overview = await _healthQueryService.GetFleetHealthOverviewAsync(_tenantProvider.ApplicationId, cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// GET /api/v1/orchestrator/health/engines/{id}
    /// Detailed health metrics and score evaluation for a specific AI Engine instance.
    /// </summary>
    [HttpGet("engines/{id:guid}")]
    public async Task<ActionResult<EngineHealthSummaryDto>> GetEngineHealth(
        Guid id,
        CancellationToken cancellationToken)
    {
        var health = await _healthQueryService.GetEngineHealthByIdAsync(_tenantProvider.ApplicationId, id, cancellationToken);
        if (health == null) return NotFound(new { Message = $"Engine instance '{id}' not found." });
        return Ok(health);
    }

    /// <summary>
    /// GET /api/v1/orchestrator/health/history/{id}
    /// Historical telemetry points and score timeline for target AI Engine instance.
    /// </summary>
    [HttpGet("history/{id:guid}")]
    public async Task<ActionResult<IReadOnlyList<EngineHealthHistoryPointDto>>> GetEngineHealthHistory(
        Guid id,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var history = await _healthQueryService.GetEngineHealthHistoryAsync(_tenantProvider.ApplicationId, id, take, cancellationToken);
        return Ok(history);
    }
}
