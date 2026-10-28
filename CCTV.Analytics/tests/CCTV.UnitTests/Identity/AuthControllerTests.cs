using CCTV.Api.Controllers;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace CCTV.UnitTests.Identity;

public class AuthControllerTests
{
    private readonly IAuthService _authService = Substitute.For<IAuthService>();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_authService);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsOkWithToken()
    {
        var req = new LoginRequestDto(Username: "admin", Password: "Secret123!");
        var expectedResponse = new LoginResponseDto("token123", "Bearer", 86400,
            new UserDto(Guid.NewGuid(), "admin", "admin@bionics.ai", "Admin", null, true, null, new List<string> { "Admin" }));

        _authService.LoginAsync(req, Arg.Any<CancellationToken>()).Returns(expectedResponse);

        var result = await _controller.Login(req, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.Equal("token123", body.AccessToken);
    }

    [Fact]
    public async Task Login_Unauthorized_Returns401()
    {
        var req = new LoginRequestDto(Username: "admin", Password: "WrongPass");
        _authService.LoginAsync(req, Arg.Any<CancellationToken>())
            .Returns<LoginResponseDto>(_ => throw new UnauthorizedAccessException("Invalid username/email or password."));

        var result = await _controller.Login(req, CancellationToken.None);

        var unauthResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.NotNull(unauthResult.Value);
    }
}
