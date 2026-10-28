using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Shared.Interfaces;

namespace CCTV.Application.Identity.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public RoleService(IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesWithPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var permRepo = _unitOfWork.Repository<RolePermission>();

        var roles = await roleRepo.FindAsync(_ => true, cancellationToken);
        var permissions = await permRepo.FindAsync(_ => true, cancellationToken);

        var groupedPermissions = permissions
            .GroupBy(p => p.RoleId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.PermissionClaim).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        return roles.Select(r => new RoleDto(
            r.Id,
            r.RoleName,
            groupedPermissions.TryGetValue(r.Id, out var perms) ? perms : new List<string>()
        )).ToList();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var permRepo = _unitOfWork.Repository<RolePermission>();

        var role = await roleRepo.GetByIdAsync(roleId, cancellationToken);
        if (role == null) return null;

        var permissions = await permRepo.FindAsync(p => p.RoleId == roleId, cancellationToken);
        var perms = permissions.Select(p => p.PermissionClaim).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return new RoleDto(role.Id, role.RoleName, perms);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            throw new ArgumentException("Role name cannot be empty.", nameof(request));
        }

        var trimmedRoleName = request.RoleName.Trim();
        var roleRepo = _unitOfWork.Repository<Role>();

        var exists = await roleRepo.ExistsAsync(r => r.RoleName.ToLower() == trimmedRoleName.ToLower(), cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Role '{trimmedRoleName}' already exists.");
        }

        var role = new Role
        {
            ApplicationId = _tenantProvider.ApplicationId,
            RoleName = trimmedRoleName
        };

        await roleRepo.AddAsync(role, cancellationToken);

        var validPerms = (request.Permissions ?? new List<string>())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (validPerms.Count > 0)
        {
            var permRepo = _unitOfWork.Repository<RolePermission>();
            foreach (var claim in validPerms)
            {
                await permRepo.AddAsync(new RolePermission
                {
                    ApplicationId = _tenantProvider.ApplicationId,
                    RoleId = role.Id,
                    PermissionClaim = claim
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.Id, role.RoleName, validPerms);
    }

    public async Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, List<string> permissions, CancellationToken cancellationToken = default)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var permRepo = _unitOfWork.Repository<RolePermission>();

        var role = await roleRepo.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID '{roleId}' was not found.");
        }

        var existingPerms = await permRepo.FindAsync(p => p.RoleId == roleId, cancellationToken);
        foreach (var p in existingPerms)
        {
            permRepo.Delete(p);
        }

        var validDistinctPerms = permissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var claim in validDistinctPerms)
        {
            await permRepo.AddAsync(new RolePermission
            {
                ApplicationId = role.ApplicationId,
                RoleId = role.Id,
                PermissionClaim = claim
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.Id, role.RoleName, validDistinctPerms);
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(roleId, cancellationToken);
        if (role == null) return false;

        if (string.Equals(role.RoleName, AppRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleName, AppRoles.Operator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"System role '{role.RoleName}' cannot be deleted.");
        }

        var permRepo = _unitOfWork.Repository<RolePermission>();
        var userRoleRepo = _unitOfWork.Repository<UserRole>();

        var perms = await permRepo.FindAsync(p => p.RoleId == roleId, cancellationToken);
        foreach (var p in perms) permRepo.Delete(p);

        var userRoles = await userRoleRepo.FindAsync(ur => ur.RoleId == roleId, cancellationToken);
        foreach (var ur in userRoles) userRoleRepo.Delete(ur);

        roleRepo.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
