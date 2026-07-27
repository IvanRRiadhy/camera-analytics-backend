using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Spatial;

public class CameraGroup : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<CameraGroupMember> Members { get; set; } = new List<CameraGroupMember>();
}

public class CameraGroupMember : TenantEntity
{
    public Guid CameraGroupId { get; set; }
    public CameraGroup CameraGroup { get; set; } = null!;

    public Guid CameraId { get; set; }
    public Camera Camera { get; set; } = null!;
}
