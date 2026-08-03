namespace CCTV.Messaging.Contracts.Analytics;

public record PeopleCountingState(
    int Current,
    int Entered,
    int Exited
);

public record QueueDetectionState(
    int QueueLength,
    int WaitingPeople
);

public record LoiteringState(
    int Active,
    IReadOnlyList<int> TrackIds
);

public record ZoneOccupancyState(
    IReadOnlyDictionary<string, int> OccupancyByZone
);

public record PipelineHealthState(
    float Fps,
    float LatencyMs,
    float InferenceTimeMs,
    float TrackerTimeMs,
    int FrameCount,
    string Status,
    string EngineVersion
);

public record PluginStates(
    PeopleCountingState? PeopleCounting,
    QueueDetectionState? QueueDetection,
    LoiteringState? Loitering,
    ZoneOccupancyState? ZoneOccupancy
);

public record CameraAnalyticsSnapshot(
    Guid MessageId,
    string SchemaVersion,
    string EngineId,
    string CameraId,
    string PipelineId,
    DateTimeOffset Timestamp,
    PipelineHealthState Health,
    PluginStates Plugins
);
