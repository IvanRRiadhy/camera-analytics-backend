using CCTV.Domain.Enumerations;

namespace CCTV.Application.Orchestration.Interfaces;

public record HealthEvaluationResult(
    int HealthScore,
    HealthStatus HealthState,
    List<string> Warnings
);

public interface IEngineHealthEvaluationService
{
    HealthEvaluationResult EvaluateHealth(
        float cpuUsagePercent,
        float gpuUsagePercent,
        int vramUsedMb,
        int totalVramMb,
        float gpuTemperatureC,
        int frameDropCount,
        double elapsedHeartbeatSeconds
    );
}
