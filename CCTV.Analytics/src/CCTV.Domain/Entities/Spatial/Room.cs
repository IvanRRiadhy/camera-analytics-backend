using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class Room : TenantEntity
{
    public Guid FloorId { get; set; }
    public Floor Floor { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<Camera> Cameras { get; set; } = new List<Camera>();
}
