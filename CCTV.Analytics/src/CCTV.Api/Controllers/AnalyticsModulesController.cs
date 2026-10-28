using CCTV.Api.Security;
using CCTV.Application.Analytics.Interfaces;
using CCTV.Contracts.Analytics;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/analytics/modules")]
[RequirePermission(AppPermissions.Analytics.Read)]
public class AnalyticsModulesController : ControllerBase
{
    private readonly IAnalyticsCatalogService _catalogService;

    public AnalyticsModulesController(IAnalyticsCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AnalyticsModuleDto>>>> GetModules(CancellationToken cancellationToken)
    {
        var modules = await _catalogService.GetModulesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AnalyticsModuleDto>>.SuccessResponse(modules));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AnalyticsModuleDto>>> GetModuleById(Guid id, CancellationToken cancellationToken)
    {
        var module = await _catalogService.GetModuleByIdAsync(id, cancellationToken);
        if (module == null)
        {
            throw new NotFoundException(ResponseMessages.Analytics.ModuleNotFound);
        }

        return Ok(ApiResponse<AnalyticsModuleDto>.SuccessResponse(module));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AnalyticsModuleDto>>> CreateModule(
        [FromBody] CreateAnalyticsModuleDto dto,
        CancellationToken cancellationToken)
    {
        var created = await _catalogService.CreateModuleAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetModuleById), new { id = created.Id },
            ApiResponse<AnalyticsModuleDto>.SuccessResponse(created, ResponseMessages.Analytics.ModuleCreated, 201));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AnalyticsModuleDto>>> UpdateModule(
        Guid id,
        [FromBody] UpdateAnalyticsModuleDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _catalogService.UpdateModuleAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<AnalyticsModuleDto>.SuccessResponse(updated, ResponseMessages.Analytics.ModuleUpdated));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<AnalyticsModuleDto>>> ToggleStatus(
        Guid id,
        [FromBody] ToggleStatusDto dto,
        CancellationToken cancellationToken)
    {
        var updated = await _catalogService.UpdateModuleAsync(id, new UpdateAnalyticsModuleDto(Status: dto.Status), cancellationToken);
        return Ok(ApiResponse<AnalyticsModuleDto>.SuccessResponse(updated, ResponseMessages.Analytics.ModuleUpdated));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<string>>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _catalogService.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<string>>.SuccessResponse(categories));
    }

    [HttpGet("{id:guid}/entitlements")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ModuleEntitlementDto>>>> GetEntitlements(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entitlements = await _catalogService.GetEntitlementsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ModuleEntitlementDto>>.SuccessResponse(entitlements));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPost("{id:guid}/entitlements")]
    public async Task<ActionResult<ApiResponse<ModuleEntitlementDto>>> GrantEntitlement(
        Guid id,
        [FromBody] GrantEntitlementRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _catalogService.GrantEntitlementAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ModuleEntitlementDto>.SuccessResponse(result, ResponseMessages.Analytics.EntitlementGranted));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpDelete("{id:guid}/entitlements/{applicationId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeEntitlement(
        Guid id,
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        await _catalogService.RevokeEntitlementAsync(id, applicationId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Analytics.EntitlementRevoked));
    }

    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteModule(Guid id, CancellationToken cancellationToken)
    {
        await _catalogService.DeleteModuleAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Analytics.ModuleDeleted));
    }
}
