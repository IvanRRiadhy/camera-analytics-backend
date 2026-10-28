using CCTV.Application.Orchestration.Interfaces;
using CCTV.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/engine/runtime")]
public class EngineBootstrapController : ControllerBase
{
    private readonly IRuntimeObservabilityService _observabilityService;

    public EngineBootstrapController(IRuntimeObservabilityService observabilityService)
    {
        _observabilityService = observabilityService;
    }

    [AllowAnonymous]
    [HttpGet("bootstrap")]
    public async Task<ActionResult<ApiResponse<object>>> GetRuntimeBootstrap(
        [FromQuery] string? engineInstanceId,
        CancellationToken cancellationToken)
    {
        var data = await _observabilityService.GetRuntimeBootstrapAsync(engineInstanceId, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(data));
    }
}
