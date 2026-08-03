using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

/// <summary>
/// Domain Entity representing an installed vision capability / plugin registered on an AI Engine node.
/// </summary>
public class EngineCapability : TenantEntity
{
    public Guid EngineInstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public string CapabilityCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string SdkVersion { get; set; } = "12.0";
}
