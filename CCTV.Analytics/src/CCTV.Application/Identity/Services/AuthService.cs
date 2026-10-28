using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Shared.Interfaces;

namespace CCTV.Application.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserContext _userContext;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ITenantProvider tenantProvider,
        ICurrentUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _tenantProvider = tenantProvider;
        _userContext = userContext;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var username = request.Username.Trim().ToLowerInvariant();

        var user = await userRepo.FirstOrDefaultAsync(
            u => u.Username.ToLower() == username,
            cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        var permissions = await GetPermissionsForUserAsync(user.Id, cancellationToken);
        var (token, expiresIn) = _tokenGenerator.GenerateToken(user, roles, permissions);

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.PhoneNumber,
            user.IsActive,
            user.LastLoginAt,
            roles,
            permissions);

        return new LoginResponseDto(token, "Bearer", expiresIn, userDto);
    }

    public async Task<CurrentUserProfileDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_userContext.UserId.HasValue) return null;

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.FirstOrDefaultAsync(u => u.Id == _userContext.UserId.Value, cancellationToken);
        if (user == null) return null;

        var roles = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        var permissions = await GetPermissionsForUserAsync(user.Id, cancellationToken);
        return new CurrentUserProfileDto(user.Id, user.Username, user.Email, user.FullName, roles, permissions);
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

    private async Task<List<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userRoleRepo = _unitOfWork.Repository<UserRole>();
        var roleRepo = _unitOfWork.Repository<Role>();
        var permRepo = _unitOfWork.Repository<RolePermission>();

        var userRoles = await userRoleRepo.FindAsync(ur => ur.UserId == userId, cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        if (roleIds.Count == 0) return new List<string>();

        var roles = await roleRepo.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);
        if (roles.Any(r => string.Equals(r.RoleName, AppRoles.Admin, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(r.RoleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase)))
        {
            return new List<string> { AppPermissions.AllAccess };
        }

        var rolePermissions = await permRepo.FindAsync(rp => roleIds.Contains(rp.RoleId), cancellationToken);
        var perms = rolePermissions
            .Select(p => p.PermissionClaim)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (perms.Contains(AppPermissions.AllAccess))
        {
            return new List<string> { AppPermissions.AllAccess };
        }

        return perms;
    }
}
