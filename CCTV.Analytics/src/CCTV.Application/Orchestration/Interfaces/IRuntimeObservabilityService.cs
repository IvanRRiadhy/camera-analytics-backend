using CCTV.Contracts.Engine;
using CCTV.Contracts.Observability;
using CCTV.Domain.Entities.Monitoring;

namespace CCTV.Application.Orchestration.Interfaces;

public interface IRuntimeObservabilityService
{
    Task<RuntimeDashboardDto> GetRuntimeDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EngineRuntime>> GetEnginesAsync(CancellationToken cancellationToken = default);
    Task<EngineRuntime?> GetEngineByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PluginRuntime>> GetPluginsAsync(CancellationToken cancellationToken = default);
    Task<PluginRuntime?> GetPluginByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AnalyticsRuntimeLog>> GetRuntimeLogsAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<object> GetRuntimeBootstrapAsync(string? engineInstanceId, CancellationToken cancellationToken = default);
    Task<EngineFleetSummaryResponseDto> GetEngineNodesAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
