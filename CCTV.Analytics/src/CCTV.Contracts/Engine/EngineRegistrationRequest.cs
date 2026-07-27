using CCTV.Contracts.Common;

namespace CCTV.Contracts.Engine;

public record EngineRegistrationRequest : BaseMessage
{
    public Guid EngineId { get; init; }
    public Guid NodeId { get; init; }
    public string EngineVersion { get; init; } = "1.0.0";
    public string Hostname { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string? GpuName { get; init; }
    public string? CudaVersion { get; init; }
    public int TotalGpuMemoryMb { get; init; }
    public int MaxPipelineCapacity { get; init; } = 8;
    public IReadOnlyList<string> SupportedModules { get; init; } = Array.Empty<string>();
    public DateTime StartupTime { get; init; } = DateTime.UtcNow;
}
