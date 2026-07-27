using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class Building : TenantEntity
{
    public Guid SiteId { get; set; }
    public Site Site { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string BuildingCode { get; set; } = string.Empty;
    public int TotalFloors { get; set; }

    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}
