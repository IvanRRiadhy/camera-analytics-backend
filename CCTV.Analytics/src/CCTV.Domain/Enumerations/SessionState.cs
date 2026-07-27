namespace CCTV.Domain.Enumerations;

public enum SessionState
{
    Pending = 0,
    Initializing = 1,
    Streaming = 2,
    Paused = 3,
    Stopped = 4,
    Failed = 5
}
