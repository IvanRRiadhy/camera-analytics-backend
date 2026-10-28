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

public class AuthServiceTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IGenericRepository<User> _userRepo = Substitute.For<IGenericRepository<User>>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly ICurrentUserContext _userContext = Substitute.For<ICurrentUserContext>();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _unitOfWork.Repository<User>().Returns(_userRepo);
        _tenantProvider.ApplicationId.Returns(Guid.NewGuid());
        _service = new AuthService(_unitOfWork, _passwordHasher, _tokenGenerator, _tenantProvider, _userContext);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        var email = "admin@bionics.ai";
        var user = new User
        {
            Username = "admin",
            Email = email,
            PasswordHash = "hashed_pass",
            FullName = "Admin User",
            IsActive = true
        };

        _userRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("Pass123!", "hashed_pass").Returns(true);
        _tokenGenerator.GenerateToken(user, Arg.Any<IList<string>>(), Arg.Any<IList<string>?>()).Returns(("mock_jwt_token", 86400));

        var response = await _service.LoginAsync(new LoginRequestDto(Username: "admin", Password: "Pass123!"));

        Assert.NotNull(response);
        Assert.Equal("mock_jwt_token", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(86400, response.ExpiresIn);
        Assert.Equal("admin", response.User.Username);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Username = "admin", Email = "admin@bionics.ai", PasswordHash = "hashed_pass", IsActive = true };
        _userRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword("WrongPass", "hashed_pass").Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequestDto(Username: "admin", Password: "WrongPass")));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Username = "disabled", Email = "disabled@bionics.ai", PasswordHash = "hashed_pass", IsActive = false };
        _userRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.LoginAsync(new LoginRequestDto(Username: "disabled", Password: "Pass123!")));
    }
}
