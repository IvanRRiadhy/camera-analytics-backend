using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using CCTV.Shared.Interfaces;
using FluentValidation;

namespace CCTV.Application.Identity.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IValidator<CreateRoleRequestDto> _createRoleValidator;
    private readonly IValidator<UpdateRolePermissionsDto> _updateRoleValidator;

    public RoleService(
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IValidator<CreateRoleRequestDto> createRoleValidator,
        IValidator<UpdateRolePermissionsDto> updateRoleValidator)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _createRoleValidator = createRoleValidator;
        _updateRoleValidator = updateRoleValidator;
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
        if (role == null)
        {
            return null;
        }

        var permissions = await permRepo.FindAsync(p => p.RoleId == roleId, cancellationToken);
        var perms = permissions.Select(p => p.PermissionClaim).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        return new RoleDto(role.Id, role.RoleName, perms);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        await _createRoleValidator.ValidateAndThrowAsync(request, cancellationToken);

        var trimmedRoleName = request.RoleName.Trim();
        var roleRepo = _unitOfWork.Repository<Role>();

        var exists = await roleRepo.ExistsAsync(r => r.RoleName.ToLower() == trimmedRoleName.ToLower(), cancellationToken);
        if (exists)
        {
            throw new BusinessException(ResponseMessages.Roles.AlreadyExists);
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

    public async Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, UpdateRolePermissionsDto request, CancellationToken cancellationToken = default)
    {
        await _updateRoleValidator.ValidateAndThrowAsync(request, cancellationToken);

        var roleRepo = _unitOfWork.Repository<Role>();
        var permRepo = _unitOfWork.Repository<RolePermission>();

        var role = await roleRepo.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ResponseMessages.Roles.NotFound);
        }

        var existingPerms = await permRepo.FindAsync(p => p.RoleId == roleId, cancellationToken);
        foreach (var p in existingPerms)
        {
            permRepo.Delete(p);
        }

        var validDistinctPerms = request.Permissions
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

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ResponseMessages.Roles.NotFound);
        }

        if (string.Equals(role.RoleName, AppRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleName, AppRoles.Operator, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(ResponseMessages.Roles.CannotDeleteSystemRole);
        }

        var permRepo = _unitOfWork.Repository<RolePermission>();
        var userRoleRepo = _unitOfWork.Repository<UserRole>();

        var perms = await permRepo.FindAsync(p => p.RoleId == roleId, cancellationToken);
        foreach (var p in perms)
        {
            permRepo.Delete(p);
        }

        var userRoles = await userRoleRepo.FindAsync(ur => ur.RoleId == roleId, cancellationToken);
        foreach (var ur in userRoles)
        {
            userRoleRepo.Delete(ur);
        }

        roleRepo.Delete(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
