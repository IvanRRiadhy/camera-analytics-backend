namespace CCTV.Application.Identity.DTOs;

public record RoleDto(Guid Id, string RoleName, List<string> Permissions);

public record CreateRoleRequestDto(string RoleName, List<string>? Permissions = null);

public record UpdateRolePermissionsDto(List<string> Permissions);

public record PermissionCatalogDto(string Module, List<string> Permissions);
