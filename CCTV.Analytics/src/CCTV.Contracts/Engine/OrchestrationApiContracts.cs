namespace CCTV.Contracts.Engine;

/// <summary>
/// Response DTO representing an individual AI Engine Node in fleet list response.
/// </summary>
public sealed record EngineNodeResponseDto(
    string InstanceId,
    string Hostname,
    string IpAddress,
    string State,
    string GpuModel,
    int TotalVramMb,
    int UsedVramMb,
    float GpuUsagePercent,
    int ActivePipelines,
    DateTime LastHeartbeatUtc
);

/// <summary>
/// Fleet Overview Summary API Response DTO.
/// </summary>
public sealed record EngineFleetSummaryResponseDto(
    int TotalNodes,
    int OnlineNodes,
    List<EngineNodeResponseDto> Nodes
);

/// <summary>
/// Dispatch Remote Control Command Request DTO.
/// </summary>
public sealed record DispatchCommandRequestDto(
    string CommandType,
    string CameraId,
    object? PipelineConfig
);

/// <summary>
/// Dispatch Remote Control Command Response DTO.
/// </summary>
public sealed record DispatchCommandResponseDto(
    string CommandId,
    string Status,
    DateTime Timestamp
);
