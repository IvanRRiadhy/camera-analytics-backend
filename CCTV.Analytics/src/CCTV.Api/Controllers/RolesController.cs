using CCTV.Api.Security;
using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Interfaces;
using CCTV.Domain.Entities.Identity;
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
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetRolesWithPermissionsAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Read)]
    public async Task<ActionResult<RoleDto>> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
        if (role == null)
        {
            return NotFound(new { Message = $"Role with ID '{id}' was not found." });
        }
        return Ok(role);
    }

    [HttpPost]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<RoleDto>> CreateRole(
        [FromBody] CreateRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<ActionResult<RoleDto>> UpdateRolePermissions(
        Guid id,
        [FromBody] UpdateRolePermissionsDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _roleService.UpdateRolePermissionsAsync(id, request.Permissions, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(AppPermissions.Users.Write)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _roleService.DeleteRoleAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { Message = $"Role with ID '{id}' was not found." });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
    }
}
