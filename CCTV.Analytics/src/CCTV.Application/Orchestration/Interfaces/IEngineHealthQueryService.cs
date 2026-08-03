using CCTV.Application.Orchestration.DTOs;

namespace CCTV.Application.Orchestration.Interfaces;

public interface IEngineHealthQueryService
{
    Task<EngineFleetHealthOverviewDto> GetFleetHealthOverviewAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<EngineHealthSummaryDto?> GetEngineHealthByIdAsync(Guid applicationId, Guid engineInstanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EngineHealthHistoryPointDto>> GetEngineHealthHistoryAsync(Guid applicationId, Guid engineInstanceId, int take = 100, CancellationToken cancellationToken = default);
}
