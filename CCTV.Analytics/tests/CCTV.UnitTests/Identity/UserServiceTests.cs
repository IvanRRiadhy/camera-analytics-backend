using System.Linq.Expressions;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Application.Identity.Services;
using CCTV.Domain.Entities.Identity;
using CCTV.Shared.Interfaces;
using NSubstitute;
using Xunit;

namespace CCTV.UnitTests.Identity;

public class UserServiceTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IGenericRepository<User> _userRepo = Substitute.For<IGenericRepository<User>>();
    private readonly IGenericRepository<Role> _roleRepo = Substitute.For<IGenericRepository<Role>>();
    private readonly IGenericRepository<UserRole> _userRoleRepo = Substitute.For<IGenericRepository<UserRole>>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _unitOfWork.Repository<User>().Returns(_userRepo);
        _unitOfWork.Repository<Role>().Returns(_roleRepo);
        _unitOfWork.Repository<UserRole>().Returns(_userRoleRepo);
        _tenantProvider.ApplicationId.Returns(Guid.NewGuid());
        _service = new UserService(_unitOfWork, _passwordHasher, _tenantProvider);
    }

    [Fact]
    public async Task CreateUserAsync_ValidRequest_CreatesUserWithHashedPassword()
    {
        var req = new CreateUserRequestDto("newuser", "new@bionics.ai", "123456", "New User", "0812345678", new List<string> { "Operator" });
        _passwordHasher.HashPassword("123456").Returns("bcrypt_hash");
        _roleRepo.FindAsync(Arg.Any<Expression<Func<Role, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role> { new() { RoleName = "Operator" } });

        var result = await _service.CreateUserAsync(req, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("new@bionics.ai", result.Email);
        Assert.Equal("New User", result.FullName);
        await _userRepo.Received(1).AddAsync(Arg.Is<User>(u => u.PasswordHash == "bcrypt_hash" && u.Username == "newuser" && u.Email == "new@bionics.ai"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserAsync_ExistingUser_SoftDeletesUser()
    {
        var user = new User { Username = "deluser", Email = "del@bionics.ai", FullName = "Del User" };
        _userRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _service.DeleteUserAsync(user.Id, CancellationToken.None);

        Assert.True(result);
        Assert.True(user.IsDeleted);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
