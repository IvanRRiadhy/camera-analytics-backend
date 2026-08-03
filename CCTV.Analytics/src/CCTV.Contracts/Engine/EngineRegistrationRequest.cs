using System.Text.Json.Serialization;
using CCTV.Contracts.Common;

namespace CCTV.Contracts.Engine;

/// <summary>
/// Immutable registration contract emitted by AI Engine nodes during bootstrap onboarding.
/// </summary>
public sealed record EngineRegistrationRequest : BaseMessage
{
    public string InstanceId { get; init; } = string.Empty;
    public Guid WorkerNodeId { get; init; }
    public string Hostname { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string OsPlatform { get; init; } = string.Empty;
    public string PythonVersion { get; init; } = string.Empty;
    public string EngineVersion { get; init; } = "12.6.0";
    public GpuInfoDto GpuInfo { get; init; } = new();
    public CpuInfoDto CpuInfo { get; init; } = new();
    public List<PluginMetadataDto> InstalledPlugins { get; init; } = new();
    public DateTime RegisteredAtUtc { get; init; } = DateTime.UtcNow;

    // Backward Compatibility Properties for legacy handlers prior to TASK-BE-002
    [JsonIgnore]
    public Guid NodeId => WorkerNodeId;

    [JsonIgnore]
    public Guid EngineId => Guid.TryParse(InstanceId, out var g) ? g : Guid.Empty;

    [JsonIgnore]
    public int TotalGpuMemoryMb => GpuInfo.TotalVramMb;

    public int MaxPipelineCapacity { get; init; } = 8;
}

/// <summary>
/// GPU Hardware Specification DTO.
/// </summary>
public sealed record GpuInfoDto
{
    public string DeviceName { get; init; } = string.Empty;
    public string CudaVersion { get; init; } = string.Empty;
    public string DriverVersion { get; init; } = string.Empty;
    public int TotalVramMb { get; init; }
    public int CudaCores { get; init; }
}

/// <summary>
/// CPU Hardware & System Memory DTO.
/// </summary>
public sealed record CpuInfoDto
{
    public string Architecture { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public int TotalCores { get; init; }
    public int TotalThreads { get; init; }
    public int TotalRamMb { get; init; }
}

/// <summary>
/// Plugin Metadata DTO representing an installed vision plugin capability.
/// </summary>
public sealed record PluginMetadataDto
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string SdkVersion { get; init; } = string.Empty;
    public List<string> Capabilities { get; init; } = new();
}
