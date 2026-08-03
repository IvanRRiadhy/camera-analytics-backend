using CCTV.Application.Orchestration.Interfaces;
using CCTV.Application.Orchestration.Options;
using CCTV.Domain.Enumerations;
using Microsoft.Extensions.Options;

namespace CCTV.Application.Orchestration.Services;

public class EngineHealthEvaluationService : IEngineHealthEvaluationService
{
    private readonly HealthScoringOptions _options;

    public EngineHealthEvaluationService(IOptions<HealthScoringOptions>? options = null)
    {
        _options = options?.Value ?? new HealthScoringOptions();
    }

    public HealthEvaluationResult EvaluateHealth(
        float cpuUsagePercent,
        float gpuUsagePercent,
        int vramUsedMb,
        int totalVramMb,
        float gpuTemperatureC,
        int frameDropCount,
        double elapsedHeartbeatSeconds)
    {
        var score = 100;
        var warnings = new List<string>();

        if (elapsedHeartbeatSeconds >= _options.MissedHeartbeatOfflineSec)
        {
            warnings.Add($"Heartbeat timeout exceeded ({elapsedHeartbeatSeconds:F1}s >= {_options.MissedHeartbeatOfflineSec}s)");
            return new HealthEvaluationResult(0, HealthStatus.Offline, warnings);
        }

        if (elapsedHeartbeatSeconds >= _options.MissedHeartbeatWarningSec)
        {
            score -= 30;
            warnings.Add($"Heartbeat delayed ({elapsedHeartbeatSeconds:F1}s >= {_options.MissedHeartbeatWarningSec}s)");
        }

        if (totalVramMb > 0 && (float)vramUsedMb / totalVramMb * 100.0f >= _options.VramThresholdPercent)
        {
            score -= 25;
            warnings.Add($"VRAM usage critical ({(float)vramUsedMb / totalVramMb * 100.0f:F1}% >= {_options.VramThresholdPercent}%)");
        }

        if (gpuTemperatureC >= _options.GpuTempThresholdC)
        {
            score -= 20;
            warnings.Add($"GPU temperature critical ({gpuTemperatureC:F1}°C >= {_options.GpuTempThresholdC}°C)");
        }

        if (cpuUsagePercent >= _options.CpuThresholdPercent)
        {
            score -= 15;
            warnings.Add($"CPU usage high ({cpuUsagePercent:F1}% >= {_options.CpuThresholdPercent}%)");
        }

        if (gpuUsagePercent >= _options.GpuThresholdPercent)
        {
            score -= 15;
            warnings.Add($"GPU usage high ({gpuUsagePercent:F1}% >= {_options.GpuThresholdPercent}%)");
        }

        if (frameDropCount >= _options.FrameDropThreshold)
        {
            score -= 15;
            warnings.Add($"Frame drops detected ({frameDropCount} >= {_options.FrameDropThreshold})");
        }

        score = Math.Clamp(score, 0, 100);

        HealthStatus state = score switch
        {
            >= 80 => HealthStatus.Healthy,
            >= 50 => HealthStatus.Warning,
            > 0 => HealthStatus.Critical,
            _ => HealthStatus.Offline
        };

        return new HealthEvaluationResult(score, state, warnings);
    }
}
