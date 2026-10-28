using CCTV.Api.Security;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [RequirePermission(AppPermissions.Users.Read)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.SuccessResponse(users));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Read)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    [HttpPost]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser(
        [FromBody] CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.CreateUserAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetUserById), 
            new { id = user.Id }, 
            ApiResponse<UserDto>.SuccessResponse(user, ResponseMessages.Users.Created, StatusCodes.Status201Created));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, ResponseMessages.Users.Updated));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeleteUserAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Users.Deleted));
    }
}
