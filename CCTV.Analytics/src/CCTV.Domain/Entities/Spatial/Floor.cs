using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class Floor : TenantEntity
{
    public Guid BuildingId { get; set; }
    public Building Building { get; set; } = null!;

    public int FloorLevel { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FloorPlanImageUrl { get; set; }

    public ICollection<Camera> Cameras { get; set; } = new List<Camera>();
}
