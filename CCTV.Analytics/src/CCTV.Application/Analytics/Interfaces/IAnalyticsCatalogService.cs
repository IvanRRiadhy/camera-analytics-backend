using CCTV.Contracts.Analytics;

namespace CCTV.Application.Analytics.Interfaces;

public interface IAnalyticsCatalogService
{
    Task<IReadOnlyList<AnalyticsModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default);
    Task<AnalyticsModuleDto?> GetModuleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AnalyticsModuleDto> CreateModuleAsync(CreateAnalyticsModuleDto dto, CancellationToken cancellationToken = default);
    Task<AnalyticsModuleDto> UpdateModuleAsync(Guid id, UpdateAnalyticsModuleDto dto, CancellationToken cancellationToken = default);
    Task DeleteModuleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModuleEntitlementDto>> GetEntitlementsAsync(Guid moduleId, CancellationToken cancellationToken = default);
    Task<ModuleEntitlementDto> GrantEntitlementAsync(Guid moduleId, GrantEntitlementRequestDto request, CancellationToken cancellationToken = default);
    Task RevokeEntitlementAsync(Guid moduleId, Guid applicationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PipelineAnalyticsOverviewDto>> GetPipelinesWithModulesAsync(CancellationToken cancellationToken = default);
    Task<PipelineAnalyticsOverviewDto?> GetPipelineWithModulesByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdatePipelineModulesAsync(Guid pipelineAssignmentId, UpdatePipelineModulesDto request, CancellationToken cancellationToken = default);
}
