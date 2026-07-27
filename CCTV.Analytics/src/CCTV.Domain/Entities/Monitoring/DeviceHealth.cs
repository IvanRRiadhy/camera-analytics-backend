using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Monitoring;

public class DeviceHealth : TenantEntity
{
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public decimal Fps { get; set; } = 30.0m;
    public int LatencyMs { get; set; } = 50;
    public decimal PacketLossPercentage { get; set; } = 0.0m;
    public string Status { get; set; } = "Online";
}
