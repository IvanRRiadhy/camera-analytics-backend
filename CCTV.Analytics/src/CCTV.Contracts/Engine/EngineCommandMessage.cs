using CCTV.Contracts.Common;

namespace CCTV.Contracts.Engine;

/// <summary>
/// Remote control command message dispatched from Backend to AI Engine nodes over RabbitMQ topic exchange.
/// Supported Command Types: START_PIPELINE, STOP_PIPELINE, RESTART_SERVICE, UPDATE_CONFIG.
/// </summary>
public sealed record EngineCommandMessage : BaseMessage
{
    public string CommandId { get; init; } = $"cmd_{Guid.NewGuid():N}";
    public string TargetInstanceId { get; init; } = string.Empty;
    public string CommandType { get; init; } = string.Empty;
    public string CameraId { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = "{}";
    public DateTime DispatchedAtUtc { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Remote command execution result response returned by AI Engine nodes.
/// Status Values: SUCCESS, FAILED, INVALID_COMMAND.
/// </summary>
public sealed record EngineCommandResultDto
{
    public string CommandId { get; init; } = string.Empty;
    public string TargetInstanceId { get; init; } = string.Empty;
    public string Status { get; init; } = "SUCCESS";
    public string Message { get; init; } = string.Empty;
    public DateTime ExecutedAtUtc { get; init; } = DateTime.UtcNow;
}
