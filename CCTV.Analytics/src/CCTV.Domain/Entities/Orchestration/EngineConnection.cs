using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Orchestration;

public class EngineConnection : TenantEntity
{
    public Guid InstanceId { get; set; }
    public EngineInstance Instance { get; set; } = null!;

    public string ConnectionProtocol { get; set; } = "gRPC";
    public string ClientIp { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int KeepAlivePingMs { get; set; } = 30;
}
