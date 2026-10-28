using CCTV.Application.Identity.DTOs;

namespace CCTV.Application.Identity.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetRolesWithPermissionsAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, UpdateRolePermissionsDto request, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
