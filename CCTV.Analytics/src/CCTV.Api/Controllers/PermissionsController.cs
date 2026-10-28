using CCTV.Api.Security;
using CCTV.Application.Identity.DTOs;
using CCTV.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/permissions")]
[RequirePermission(AppPermissions.Users.Read)]
public class PermissionsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PermissionCatalogDto>> GetPermissions()
    {
        var catalog = new List<PermissionCatalogDto>
        {
            new("Spatial", new List<string> { AppPermissions.Spatial.Read, AppPermissions.Spatial.Write }),
            new("Analytics", new List<string> { AppPermissions.Analytics.Read, AppPermissions.Analytics.Write }),
            new("Orchestrator", new List<string> { AppPermissions.Orchestrator.Read, AppPermissions.Orchestrator.Write, AppPermissions.Orchestrator.Execute }),
            new("Monitoring", new List<string> { AppPermissions.Monitoring.Read, AppPermissions.Monitoring.Write }),
            new("Users", new List<string> { AppPermissions.Users.Read, AppPermissions.Users.Write })
        };

        return Ok(catalog);
    }
}
