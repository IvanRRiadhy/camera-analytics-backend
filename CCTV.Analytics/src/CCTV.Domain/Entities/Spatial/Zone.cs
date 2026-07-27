using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class Zone : TenantEntity
{
    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public ZoneType ZoneType { get; set; } = ZoneType.Tripwire;
    public string PolygonCoordinatesJson { get; set; } = "[]";
    public string ColorHex { get; set; } = "#FF0000";
}
