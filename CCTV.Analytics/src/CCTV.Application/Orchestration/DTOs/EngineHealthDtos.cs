using CCTV.Domain.Enumerations;

namespace CCTV.Application.Orchestration.DTOs;

public record EngineHealthSummaryDto(
    Guid InstanceId,
    string InstanceIdString,
    string Hostname,
    string IpAddress,
    string State,
    int HealthScore,
    HealthStatus HealthState,
    float CpuUsagePercent,
    float GpuUsagePercent,
    int VramUsedMb,
    int TotalVramMb,
    float GpuTemperatureC,
    float InferenceFps,
    int FrameDropCount,
    int ActivePipelinesCount,
    DateTime LastHeartbeatUtc
);

public record EngineFleetHealthOverviewDto(
    int TotalEngines,
    int HealthyCount,
    int WarningCount,
    int CriticalCount,
    int OfflineCount,
    float AverageHealthScore,
    IReadOnlyList<EngineHealthSummaryDto> Engines
);

public record EngineHealthHistoryPointDto(
    Guid Id,
    Guid EngineInstanceId,
    float CpuUsagePercent,
    float GpuUsagePercent,
    int VramUsedMb,
    int TotalVramMb,
    float GpuTemperatureC,
    float InferenceFps,
    int FrameDropCount,
    int ActivePipelinesCount,
    int HealthScore,
    HealthStatus HealthState,
    DateTime TimestampUtc
);
