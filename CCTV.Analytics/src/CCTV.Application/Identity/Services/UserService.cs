using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Shared.Interfaces;

namespace CCTV.Application.Identity.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantProvider _tenantProvider;

    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var userRoleRepo = _unitOfWork.Repository<UserRole>();
        var roleRepo = _unitOfWork.Repository<Role>();

        var users = await userRepo.FindAsync(_ => true, cancellationToken);
        if (users.Count == 0) return Array.Empty<UserDto>();

        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await userRoleRepo.FindAsync(ur => userIds.Contains(ur.UserId), cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();

        var roles = roleIds.Count > 0
            ? await roleRepo.FindAsync(r => roleIds.Contains(r.Id), cancellationToken)
            : Array.Empty<Role>();

        var roleNameMap = roles.ToDictionary(r => r.Id, r => r.RoleName);

        var userRolesLookup = userRoles
            .Where(ur => roleNameMap.ContainsKey(ur.RoleId))
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => roleNameMap[ur.RoleId]).ToList());

        return users.Select(user => MapToDto(
            user,
            userRolesLookup.TryGetValue(user.Id, out var userRoleNames) ? userRoleNames : new List<string>()
        )).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null) return null;

        var roles = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        return MapToDto(user, roles);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var roleRepo = _unitOfWork.Repository<Role>();
        var userRoleRepo = _unitOfWork.Repository<UserRole>();

        var requestedUsername = request.Username.Trim().ToLowerInvariant();
        var requestedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await userRepo.FirstOrDefaultAsync(
            u => u.Username.ToLower() == requestedUsername || u.Email.ToLower() == requestedEmail,
            cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with username '{request.Username}' or email '{request.Email}' already exists.");
        }

        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var user = new User
        {
            ApplicationId = _tenantProvider.ApplicationId,
            Username = requestedUsername,
            Email = requestedEmail,
            PasswordHash = hashedPassword,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true
        };

        await userRepo.AddAsync(user, cancellationToken);

        var assignedRoleNames = new List<string>();
        if (request.Roles != null && request.Roles.Count > 0)
        {
            var requestedRoleNames = request.Roles.Select(r => r.Trim().ToLowerInvariant()).ToList();
            var roles = await roleRepo.FindAsync(r => requestedRoleNames.Contains(r.RoleName.ToLower()), cancellationToken);

            foreach (var role in roles)
            {
                await userRoleRepo.AddAsync(new UserRole
                {
                    ApplicationId = _tenantProvider.ApplicationId,
                    UserId = user.Id,
                    RoleId = role.Id
                }, cancellationToken);
                assignedRoleNames.Add(role.RoleName);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user, assignedRoleNames);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' was not found.");
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.IsActive = request.IsActive;

        var assignedRoleNames = new List<string>();
        if (request.Roles != null)
        {
            var roleRepo = _unitOfWork.Repository<Role>();
            var userRoleRepo = _unitOfWork.Repository<UserRole>();

            var existingUserRoles = await userRoleRepo.FindAsync(ur => ur.UserId == id, cancellationToken);
            foreach (var ur in existingUserRoles)
            {
                userRoleRepo.Delete(ur);
            }

            var requestedRoleNames = request.Roles.Select(r => r.Trim().ToLowerInvariant()).ToList();
            var matchedRoles = await roleRepo.FindAsync(r => requestedRoleNames.Contains(r.RoleName.ToLower()), cancellationToken);

            foreach (var role in matchedRoles)
            {
                await userRoleRepo.AddAsync(new UserRole
                {
                    ApplicationId = _tenantProvider.ApplicationId,
                    UserId = user.Id,
                    RoleId = role.Id
                }, cancellationToken);
                assignedRoleNames.Add(role.RoleName);
            }
        }
        else
        {
            assignedRoleNames = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        }

        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user, assignedRoleNames);
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null) return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<List<string>> GetRoleNamesForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userRoleRepo = _unitOfWork.Repository<UserRole>();
        var roleRepo = _unitOfWork.Repository<Role>();

        var userRoles = await userRoleRepo.FindAsync(ur => ur.UserId == userId, cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        if (roleIds.Count == 0) return new List<string>();

        var roles = await roleRepo.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);
        return roles.Select(r => r.RoleName).ToList();
    }

    private static UserDto MapToDto(User user, List<string> roles)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.IsActive,
            user.LastLoginAt,
            roles);
    }
}
