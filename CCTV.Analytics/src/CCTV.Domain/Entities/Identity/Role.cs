using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Identity;

public class Role : TenantEntity
{
    public string RoleName { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class UserRole : TenantEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public class RolePermission : TenantEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string PermissionClaim { get; set; } = string.Empty;
}
