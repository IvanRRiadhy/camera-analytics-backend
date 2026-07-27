using CCTV.Domain.Enumerations;
using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class Camera : TenantEntity
{
    public Guid SiteId { get; set; }
    public Site Site { get; set; } = null!;

    public Guid? FloorId { get; set; }
    public Floor? Floor { get; set; }

    public string Name { get; set; } = string.Empty;
    public string DeviceCode { get; set; } = string.Empty;
    public string RtspStreamUrl { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public int Port { get; set; } = 554;
    public string? MacAddress { get; set; }

    public CameraStatus Status { get; set; } = CameraStatus.Offline;
    public bool PtzSupported { get; set; }
    public string Resolution { get; set; } = "1080p";
    public int FrameRate { get; set; } = 30;

    // Operational Calibration Metadata
    public string? HomographyMatrixJson { get; set; }
    public string? GroundPlaneOriginJson { get; set; }
    public decimal? ScaleFactorMetersPerPixel { get; set; }
    public string? PtzPresetCoordinatesJson { get; set; }

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
    public ICollection<CameraGroupMember> CameraGroupMembers { get; set; } = new List<CameraGroupMember>();
}
