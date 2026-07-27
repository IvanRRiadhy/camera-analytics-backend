using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class Site : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string TimeZone { get; set; } = "UTC";

    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<Camera> Cameras { get; set; } = new List<Camera>();
}
