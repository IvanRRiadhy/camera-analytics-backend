namespace CCTV.Domain.Enumerations;

/// <summary>
/// Operational state lifecycle for AI Engine runtime instances.
/// </summary>
public enum EngineState
{
    Offline = 0,
    Initializing = 1,
    Online = 2,
    Degraded = 3,
    Faulted = 4
}
