using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, ResponseMessages.Auth.LoginSuccess));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserProfileDto>>> GetMe(CancellationToken cancellationToken)
    {
        var profile = await _authService.GetCurrentUserAsync(cancellationToken);
        if (profile == null)
        {
            throw new NotFoundException(ResponseMessages.Auth.ProfileNotFound);
        }

        return Ok(ApiResponse<CurrentUserProfileDto>.SuccessResponse(profile));
    }
}
