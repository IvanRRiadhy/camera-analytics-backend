using CCTV.Api.Security;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Contracts.Observability;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Entities.Monitoring;
using CCTV.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/runtime")]
[RequirePermission(AppPermissions.Monitoring.Read)]
public class RuntimeObservabilityController : ControllerBase
{
    private readonly IRuntimeObservabilityService _observabilityService;

    public RuntimeObservabilityController(IRuntimeObservabilityService observabilityService)
    {
        _observabilityService = observabilityService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<RuntimeDashboardDto>>> GetRuntimeDashboard(CancellationToken cancellationToken)
    {
        var dashboard = await _observabilityService.GetRuntimeDashboardAsync(cancellationToken);
        return Ok(ApiResponse<RuntimeDashboardDto>.SuccessResponse(dashboard));
    }

    [HttpGet("engines")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EngineRuntime>>>> GetEngines(CancellationToken cancellationToken)
    {
        var engines = await _observabilityService.GetEnginesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EngineRuntime>>.SuccessResponse(engines));
    }

    [HttpGet("engines/{id:guid}")]
    public async Task<ActionResult<ApiResponse<EngineRuntime>>> GetEngineById(Guid id, CancellationToken cancellationToken)
    {
        var engine = await _observabilityService.GetEngineByIdAsync(id, cancellationToken);
        if (engine == null)
        {
            throw new NotFoundException(ResponseMessages.Pipelines.EngineNotFound);
        }

        return Ok(ApiResponse<EngineRuntime>.SuccessResponse(engine));
    }

    [HttpGet("plugins")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PluginRuntime>>>> GetPlugins(CancellationToken cancellationToken)
    {
        var plugins = await _observabilityService.GetPluginsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PluginRuntime>>.SuccessResponse(plugins));
    }

    [HttpGet("plugins/{id:guid}")]
    public async Task<ActionResult<ApiResponse<PluginRuntime>>> GetPluginById(Guid id, CancellationToken cancellationToken)
    {
        var plugin = await _observabilityService.GetPluginByIdAsync(id, cancellationToken);
        if (plugin == null)
        {
            throw new NotFoundException(ResponseMessages.NotFound);
        }

        return Ok(ApiResponse<PluginRuntime>.SuccessResponse(plugin));
    }

    [HttpGet("logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AnalyticsRuntimeLog>>>> GetRuntimeLogs(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _observabilityService.GetRuntimeLogsAsync(limit, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AnalyticsRuntimeLog>>.SuccessResponse(logs));
    }
}
