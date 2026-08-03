namespace CCTV.Domain.Enumerations;

/// <summary>
/// Domain Enumeration representing the lifecycle state of a Camera Analytics Pipeline Assignment.
/// </summary>
public enum PipelineStatus
{
    Unknown = 0,
    Unassigned = 1,
    Assigned = 2,
    Starting = 3,
    Running = 4,
    Stopping = 5,
    Stopped = 6,
    Failed = 7,
    Recovering = 8
}
