using CCTV.Contracts.Analytics;
using CCTV.Contracts.Common;

namespace CCTV.Contracts.Camera;

public record CameraAssignmentCommand : BaseMessage
{
    public Guid CameraId { get; init; }
    public string DeviceCode { get; init; } = string.Empty;
    public string RtspStreamUrl { get; init; } = string.Empty;
    public string? RtspUsername { get; init; }
    public string? RtspPassword { get; init; }
    public int TargetFps { get; init; } = 30;

    public CameraCalibrationDto? Calibration { get; init; }
    public IReadOnlyList<ZoneDefinitionDto> Zones { get; init; } = Array.Empty<ZoneDefinitionDto>();
    public IReadOnlyList<AnalyticsConfigDto> Configurations { get; init; } = Array.Empty<AnalyticsConfigDto>();
}
