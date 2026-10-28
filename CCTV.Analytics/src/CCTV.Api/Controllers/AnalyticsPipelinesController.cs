using CCTV.Api.Security;
using CCTV.Application.Analytics.Interfaces;
using CCTV.Contracts.Analytics;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/analytics/pipelines")]
[RequirePermission(AppPermissions.Analytics.Read)]
public class AnalyticsPipelinesController : ControllerBase
{
    private readonly IAnalyticsCatalogService _catalogService;

    public AnalyticsPipelinesController(IAnalyticsCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PipelineAnalyticsOverviewDto>>>> GetPipelines(CancellationToken cancellationToken)
    {
        var dtos = await _catalogService.GetPipelinesWithModulesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PipelineAnalyticsOverviewDto>>.SuccessResponse(dtos));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PipelineAnalyticsOverviewDto>>> GetPipelineById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _catalogService.GetPipelineWithModulesByIdAsync(id, cancellationToken);
        if (dto == null)
        {
            throw new NotFoundException(ResponseMessages.Pipelines.NotFound);
        }

        return Ok(ApiResponse<PipelineAnalyticsOverviewDto>.SuccessResponse(dto));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPut("{id:guid}/modules")]
    public async Task<ActionResult<ApiResponse<object>>> UpdatePipelineModules(
        Guid id,
        [FromBody] UpdatePipelineModulesDto request,
        CancellationToken cancellationToken)
    {
        await _catalogService.UpdatePipelineModulesAsync(id, request, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Analytics.PipelineModulesUpdated));
    }
}
