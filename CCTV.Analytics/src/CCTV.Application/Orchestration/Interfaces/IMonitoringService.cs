using CCTV.Application.Orchestration.DTOs;

namespace CCTV.Application.Orchestration.Interfaces;

public interface IMonitoringService
{
    Task<MonitoringOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<List<MonitoringEventDto>> GetAnalyticsEventsAsync(int take = 50, CancellationToken cancellationToken = default);
    Task<List<MonitoringIncidentDto>> GetIncidentsAsync(int take = 20, CancellationToken cancellationToken = default);
}
