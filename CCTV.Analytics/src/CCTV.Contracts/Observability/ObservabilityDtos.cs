namespace CCTV.Contracts.Observability;

public record RuntimeDashboardDto(
    DateTime TimestampUtc,
    int OnlineEngines,
    int OfflineEngines,
    int RunningPipelines,
    int RunningPlugins,
    double GpuUsagePercent,
    double CpuUsagePercent,
    double RamUsagePercent,
    double AverageFps,
    double AverageLatencyMs,
    int PublishedEventsPerSec,
    long TotalPublishedEvents
);
