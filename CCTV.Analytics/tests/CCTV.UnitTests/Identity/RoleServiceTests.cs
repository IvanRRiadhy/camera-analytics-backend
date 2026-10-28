using System.Linq.Expressions;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Services;
using CCTV.Application.Identity.Validators;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using CCTV.Shared.Interfaces;
using FluentValidation;
using NSubstitute;

namespace CCTV.UnitTests.Identity;

public class RoleServiceTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly IGenericRepository<Role> _roleRepo = Substitute.For<IGenericRepository<Role>>();
    private readonly IGenericRepository<RolePermission> _permRepo = Substitute.For<IGenericRepository<RolePermission>>();
    private readonly IGenericRepository<UserRole> _userRoleRepo = Substitute.For<IGenericRepository<UserRole>>();
    private readonly RoleService _service;

    public RoleServiceTests()
    {
        _unitOfWork.Repository<Role>().Returns(_roleRepo);
        _unitOfWork.Repository<RolePermission>().Returns(_permRepo);
        _unitOfWork.Repository<UserRole>().Returns(_userRoleRepo);
        _tenantProvider.ApplicationId.Returns(Guid.NewGuid());

        _service = new RoleService(
            _unitOfWork,
            _tenantProvider,
            new CreateRoleRequestValidator(),
            new UpdateRolePermissionsValidator());
    }

    [Fact]
    public async Task GetRolesWithPermissionsAsync_ReturnsRolesWithAssignedPermissions()
    {
        var role = new Role { RoleName = "Operator" };
        var roleId = role.Id;
        var roles = new List<Role> { role };
        var permissions = new List<RolePermission>
        {
            new() { RoleId = roleId, PermissionClaim = "spatial.read" },
            new() { RoleId = roleId, PermissionClaim = "monitoring.read" }
        };

        _roleRepo.FindAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>()).Returns(roles);
        _permRepo.FindAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>()).Returns(permissions);

        var result = await _service.GetRolesWithPermissionsAsync();

        var roleDto = Assert.Single(result);
        Assert.Equal("Operator", roleDto.RoleName);
        Assert.Equal(2, roleDto.Permissions.Count);
        Assert.Contains("spatial.read", roleDto.Permissions);
    }

    [Fact]
    public async Task GetRoleByIdAsync_WhenNotFound_ReturnsNull()
    {
        _roleRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Role?)null);

        var result = await _service.GetRoleByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateRoleAsync_WithValidInput_CreatesRoleAndPermissions()
    {
        _roleRepo.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>()).Returns(false);

        var request = new CreateRoleRequestDto("Supervisor", new List<string> { "spatial.read", "monitoring.read" });
        var result = await _service.CreateRoleAsync(request);

        Assert.Equal("Supervisor", result.RoleName);
        Assert.Equal(2, result.Permissions.Count);
        await _roleRepo.Received(1).AddAsync(Arg.Is<Role>(r => r.RoleName == "Supervisor"), Arg.Any<CancellationToken>());
        await _permRepo.Received(2).AddAsync(Arg.Any<RolePermission>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateRoleAsync_WithEmptyName_ThrowsValidationException()
    {
        var request = new CreateRoleRequestDto("");
        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateRoleAsync(request));
    }

    [Fact]
    public async Task CreateRoleAsync_WithDuplicateName_ThrowsBusinessException()
    {
        _roleRepo.ExistsAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>()).Returns(true);

        var request = new CreateRoleRequestDto("Supervisor");
        var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateRoleAsync(request));
        Assert.Equal(ResponseMessages.Roles.AlreadyExists, ex.Message);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _roleRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Role?)null);

        var request = new UpdateRolePermissionsDto(new List<string> { "spatial.read" });
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateRolePermissionsAsync(Guid.NewGuid(), request));

        Assert.Equal(ResponseMessages.Roles.NotFound, ex.Message);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_ReplacesExistingPermissions()
    {
        var tenantId = Guid.NewGuid();
        var role = new Role { ApplicationId = tenantId, RoleName = "Operator" };
        var roleId = role.Id;
        var oldPerm = new RolePermission { RoleId = roleId, PermissionClaim = "old.permission" };

        _roleRepo.GetByIdAsync(roleId, Arg.Any<CancellationToken>()).Returns(role);
        _permRepo.FindAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<RolePermission> { oldPerm });

        var request = new UpdateRolePermissionsDto(new List<string> { "spatial.read", "monitoring.read" });
        var updated = await _service.UpdateRolePermissionsAsync(roleId, request);

        _permRepo.Received(1).Delete(oldPerm);
        await _permRepo.Received(2).AddAsync(Arg.Is<RolePermission>(p => p.RoleId == roleId), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.Equal(2, updated.Permissions.Count);
        Assert.Contains("spatial.read", updated.Permissions);
        Assert.Contains("monitoring.read", updated.Permissions);
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _roleRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Role?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteRoleAsync(Guid.NewGuid()));
        Assert.Equal(ResponseMessages.Roles.NotFound, ex.Message);
    }

    [Fact]
    public async Task DeleteRoleAsync_SystemRole_ThrowsBusinessException()
    {
        var role = new Role { RoleName = "Admin" };
        _roleRepo.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);

        var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.DeleteRoleAsync(role.Id));
        Assert.Equal(ResponseMessages.Roles.CannotDeleteSystemRole, ex.Message);
    }

    [Fact]
    public async Task DeleteRoleAsync_CustomRole_DeletesSuccessfully()
    {
        var role = new Role { RoleName = "CustomTechnician" };
        _roleRepo.GetByIdAsync(role.Id, Arg.Any<CancellationToken>()).Returns(role);
        _permRepo.FindAsync(Arg.Any<Expression<Func<RolePermission, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<RolePermission>());
        _userRoleRepo.FindAsync(Arg.Any<Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UserRole>());

        await _service.DeleteRoleAsync(role.Id);

        _roleRepo.Received(1).Delete(role);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
