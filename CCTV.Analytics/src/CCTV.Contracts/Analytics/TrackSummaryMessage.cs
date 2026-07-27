using CCTV.Contracts.Common;

namespace CCTV.Contracts.Analytics;

public record TrajectoryPointDto(float X, float Y, DateTime T);

public record TrackSummaryMessage : BaseMessage
{
    public Guid TrackSummaryId { get; init; }
    public string ExternalTrackId { get; init; } = string.Empty;
    public Guid SessionId { get; init; }
    public Guid CameraId { get; init; }
    public string ObjectClass { get; init; } = "Person";
    public DateTime FirstSeenAt { get; init; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; init; } = DateTime.UtcNow;
    public decimal DwellDurationSec { get; init; }
    public decimal AverageConfidence { get; init; }
    public IReadOnlyList<TrajectoryPointDto> TrajectoryPoints { get; init; } = Array.Empty<TrajectoryPointDto>();
    public decimal? SpeedKmh { get; init; }
}

public record ObjectVisitMessage : BaseMessage
{
    public Guid VisitId { get; init; }
    public string ExternalTrackId { get; init; } = string.Empty;
    public Guid CameraId { get; init; }
    public Guid ZoneId { get; init; }
    public DateTime EntryTime { get; init; } = DateTime.UtcNow;
    public DateTime? ExitTime { get; init; }
    public decimal DwellTimeSec { get; init; }
    public decimal MaxConfidence { get; init; }
}
