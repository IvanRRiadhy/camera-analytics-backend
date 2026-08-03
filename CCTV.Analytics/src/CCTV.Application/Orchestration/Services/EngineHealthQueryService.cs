using CCTV.Application.Common.Interfaces;
using CCTV.Application.Orchestration.DTOs;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;

namespace CCTV.Application.Orchestration.Services;

public class EngineHealthQueryService : IEngineHealthQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEngineHealthEvaluationService _healthEvaluationService;

    public EngineHealthQueryService(
        IUnitOfWork unitOfWork,
        IEngineHealthEvaluationService healthEvaluationService)
    {
        _unitOfWork = unitOfWork;
        _healthEvaluationService = healthEvaluationService;
    }

    public async Task<EngineFleetHealthOverviewDto> GetFleetHealthOverviewAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var instanceRepo = _unitOfWork.Repository<EngineInstance>();
        var healthHistoryRepo = _unitOfWork.Repository<EngineHealthHistory>();

        var instances = await instanceRepo.FindAsync(i => i.ApplicationId == applicationId, cancellationToken);
        var now = DateTime.UtcNow;

        var summaries = new List<EngineHealthSummaryDto>();

        foreach (var inst in instances)
        {
            var latestHistory = await healthHistoryRepo.FirstOrDefaultAsync(
                h => h.EngineInstanceId == inst.Id && h.ApplicationId == applicationId,
                cancellationToken);

            var elapsedSec = (now - inst.LastHeartbeatUtc).TotalSeconds;

            var cpuPct = latestHistory?.CpuUsagePercent ?? 0.0f;
            var gpuPct = latestHistory?.GpuUsagePercent ?? 0.0f;
            var vramUsed = latestHistory?.VramUsedMb ?? inst.AllocatedGpuMemoryMb;
            var vramTotal = latestHistory?.TotalVramMb ?? inst.TotalVramMb;
            var gpuTemp = latestHistory?.GpuTemperatureC ?? 0.0f;
            var fps = latestHistory?.InferenceFps ?? 0.0f;
            var drops = latestHistory?.FrameDropCount ?? 0;

            var eval = _healthEvaluationService.EvaluateHealth(
                cpuPct, gpuPct, vramUsed, vramTotal, gpuTemp, drops, elapsedSec);

            summaries.Add(new EngineHealthSummaryDto(
                InstanceId: inst.Id,
                InstanceIdString: !string.IsNullOrWhiteSpace(inst.InstanceId) ? inst.InstanceId : inst.EngineId.ToString(),
                Hostname: inst.Hostname,
                IpAddress: inst.IpAddress,
                State: inst.State.ToString(),
                HealthScore: eval.HealthScore,
                HealthState: eval.HealthState,
                CpuUsagePercent: cpuPct,
                GpuUsagePercent: gpuPct,
                VramUsedMb: vramUsed,
                TotalVramMb: vramTotal,
                GpuTemperatureC: gpuTemp,
                InferenceFps: fps,
                FrameDropCount: drops,
                ActivePipelinesCount: inst.ActivePipelinesCount,
                LastHeartbeatUtc: inst.LastHeartbeatUtc
            ));
        }

        var total = summaries.Count;
        var healthy = summaries.Count(s => s.HealthState == HealthStatus.Healthy);
        var warning = summaries.Count(s => s.HealthState == HealthStatus.Warning);
        var critical = summaries.Count(s => s.HealthState == HealthStatus.Critical);
        var offline = summaries.Count(s => s.HealthState == HealthStatus.Offline);
        var avgScore = total > 0 ? (float)summaries.Average(s => s.HealthScore) : 100.0f;

        return new EngineFleetHealthOverviewDto(
            TotalEngines: total,
            HealthyCount: healthy,
            WarningCount: warning,
            CriticalCount: critical,
            OfflineCount: offline,
            AverageHealthScore: avgScore,
            Engines: summaries
        );
    }

    public async Task<EngineHealthSummaryDto?> GetEngineHealthByIdAsync(Guid applicationId, Guid engineInstanceId, CancellationToken cancellationToken = default)
    {
        var overview = await GetFleetHealthOverviewAsync(applicationId, cancellationToken);
        return overview.Engines.FirstOrDefault(e => e.InstanceId == engineInstanceId);
    }

    public async Task<IReadOnlyList<EngineHealthHistoryPointDto>> GetEngineHealthHistoryAsync(Guid applicationId, Guid engineInstanceId, int take = 100, CancellationToken cancellationToken = default)
    {
        var healthHistoryRepo = _unitOfWork.Repository<EngineHealthHistory>();
        var histories = await healthHistoryRepo.FindAsync(
            h => h.EngineInstanceId == engineInstanceId && h.ApplicationId == applicationId,
            cancellationToken);

        return histories
            .OrderByDescending(h => h.TimestampUtc)
            .Take(take)
            .Select(h => new EngineHealthHistoryPointDto(
                Id: h.Id,
                EngineInstanceId: h.EngineInstanceId,
                CpuUsagePercent: h.CpuUsagePercent,
                GpuUsagePercent: h.GpuUsagePercent,
                VramUsedMb: h.VramUsedMb,
                TotalVramMb: h.TotalVramMb,
                GpuTemperatureC: h.GpuTemperatureC,
                InferenceFps: h.InferenceFps,
                FrameDropCount: h.FrameDropCount,
                ActivePipelinesCount: h.ActivePipelinesCount,
                HealthScore: h.HealthScore,
                HealthState: h.HealthState,
                TimestampUtc: h.TimestampUtc
            ))
            .ToList();
    }
}
