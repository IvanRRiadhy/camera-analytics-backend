using CCTV.Api.Security;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [RequirePermission(AppPermissions.Users.Read)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoleDto>>>> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetRolesWithPermissionsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoleDto>>.SuccessResponse(roles));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Read)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException(ResponseMessages.Roles.NotFound);
        }

        return Ok(ApiResponse<RoleDto>.SuccessResponse(role));
    }

    [HttpPost]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole(
        [FromBody] CreateRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        var role = await _roleService.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, ApiResponse<RoleDto>.SuccessResponse(role, ResponseMessages.Roles.Created, 201));
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRolePermissions(
        Guid id,
        [FromBody] UpdateRolePermissionsDto request,
        CancellationToken cancellationToken)
    {
        var updated = await _roleService.UpdateRolePermissionsAsync(id, request, cancellationToken);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(updated, ResponseMessages.Roles.Updated));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        await _roleService.DeleteRoleAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Roles.Deleted));
    }
}
