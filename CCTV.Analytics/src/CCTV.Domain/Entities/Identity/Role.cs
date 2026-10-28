using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Identity;

public class Role : TenantEntity
{
    public string RoleName { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";

    public const string Any = $"{Admin},{Operator}";

    public static readonly IReadOnlyList<string> All = new[] { Admin, Operator };
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
