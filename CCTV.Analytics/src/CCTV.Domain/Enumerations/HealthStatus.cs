namespace CCTV.Domain.Enumerations;

/// <summary>
/// Domain Enumeration representing evaluated health state of an AI Engine runtime node.
/// </summary>
public enum HealthStatus
{
    Healthy = 0,
    Warning = 1,
    Critical = 2,
    Offline = 3
}
