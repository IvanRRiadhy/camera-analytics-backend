using CCTV.Api.Security;
using CCTV.Application.Common.Interfaces;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Entities.Spatial;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/spatial")]
[RequirePermission(AppPermissions.Spatial.Read)]
public class SpatialController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public SpatialController(IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    // ==========================================
    // 0. SITES
    // ==========================================

    [HttpGet("sites")]
    public async Task<IActionResult> GetSites(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Site>();
        var items = await repo.FindAsync(x => true, cancellationToken);
        var result = items.Select(s => new
        {
            id = s.Id.ToString(),
            name = s.Name,
            description = $"Site Code: {s.Code}",
            code = s.Code,
            image = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=300&q=80"
        });
        return Ok(result);
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPost("sites")]
    public async Task<IActionResult> CreateSite([FromBody] CreateSiteRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Site>();
        var baseCode = string.IsNullOrWhiteSpace(req.Code) ? req.Name.ToLower().Replace(" ", "_") : req.Code;
        var existingSites = await repo.FindAsync(s => s.Code == baseCode, cancellationToken);
        var uniqueCode = existingSites.Any() ? $"{baseCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseCode;

        var site = new Site
        {
            ApplicationId = _tenantProvider.ApplicationId,
            Name = req.Name,
            Code = uniqueCode
        };
        await repo.AddAsync(site, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = site.Id.ToString(), name = site.Name, code = site.Code, description = req.Description, image = req.Image });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPut("sites/{id}")]
    public async Task<IActionResult> UpdateSite(Guid id, [FromBody] CreateSiteRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Site>();
        var site = await repo.GetByIdAsync(id, cancellationToken);
        if (site == null) return NotFound();

        site.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Code)) site.Code = req.Code;
        repo.Update(site);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = site.Id.ToString(), name = site.Name, code = site.Code, description = req.Description, image = req.Image });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpDelete("sites/{id}")]
    public async Task<IActionResult> DeleteSite(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Site>();
        var site = await repo.GetByIdAsync(id, cancellationToken);
        if (site == null) return NotFound();

        repo.Delete(site);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ==========================================
    // 1. BUILDINGS
    // ==========================================

    [HttpGet("buildings")]
    public async Task<IActionResult> GetBuildings(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Building>();
        var items = await repo.FindAsync(x => true, cancellationToken);
        var result = items.Select(b => new
        {
            id = b.Id.ToString(),
            name = b.Name,
            description = $"Building Code: {b.BuildingCode}",
            image = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=300&q=80",
            siteId = b.SiteId.ToString()
        });
        return Ok(result);
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPost("buildings")]
    public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Building>();
        var siteRepo = _unitOfWork.Repository<Site>();

        Guid targetSiteId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(req.SiteId) && Guid.TryParse(req.SiteId, out var parsedSiteId) && parsedSiteId != Guid.Empty)
        {
            var siteExists = await siteRepo.GetByIdAsync(parsedSiteId, cancellationToken);
            if (siteExists != null)
            {
                targetSiteId = siteExists.Id;
            }
        }

        if (targetSiteId == Guid.Empty)
        {
            var sites = await siteRepo.FindAsync(x => true, cancellationToken);
            var site = sites.FirstOrDefault();
            if (site == null)
            {
                site = new Site
                {
                    ApplicationId = _tenantProvider.ApplicationId,
                    Name = "Main Site",
                    Code = $"site_main_{Guid.NewGuid().ToString("N")[..6]}"
                };
                await siteRepo.AddAsync(site, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            targetSiteId = site.Id;
        }

        var baseBldgCode = req.Name.ToLower().Replace(" ", "_");
        var existingBldgs = await repo.FindAsync(b => b.SiteId == targetSiteId && b.BuildingCode == baseBldgCode, cancellationToken);
        var uniqueBldgCode = existingBldgs.Any() ? $"{baseBldgCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseBldgCode;

        var building = new Building
        {
            ApplicationId = _tenantProvider.ApplicationId,
            Name = req.Name,
            BuildingCode = uniqueBldgCode,
            TotalFloors = 1,
            SiteId = targetSiteId
        };
        await repo.AddAsync(building, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = building.Id.ToString(), name = building.Name, description = req.Description, image = req.Image, siteId = building.SiteId.ToString() });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPut("buildings/{id}")]
    public async Task<IActionResult> UpdateBuilding(Guid id, [FromBody] CreateBuildingRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Building>();
        var siteRepo = _unitOfWork.Repository<Site>();
        var building = await repo.GetByIdAsync(id, cancellationToken);
        if (building == null) return NotFound();

        building.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.SiteId) && Guid.TryParse(req.SiteId, out var sId) && sId != Guid.Empty)
        {
            var siteExists = await siteRepo.GetByIdAsync(sId, cancellationToken);
            if (siteExists != null)
            {
                building.SiteId = sId;
            }
        }
        repo.Update(building);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = building.Id.ToString(), name = building.Name, description = req.Description, image = req.Image, siteId = building.SiteId.ToString() });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpDelete("buildings/{id}")]
    public async Task<IActionResult> DeleteBuilding(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Building>();
        var building = await repo.GetByIdAsync(id, cancellationToken);
        if (building == null) return NotFound();

        repo.Delete(building);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ==========================================
    // 2. FLOORS
    // ==========================================

    [HttpGet("floors")]
    public async Task<IActionResult> GetFloors(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var items = await repo.FindAsync(x => true, cancellationToken);
        var result = items.Select(f => new
        {
            id = f.Id.ToString(),
            name = f.Name,
            description = $"Level {f.FloorLevel}",
            image = f.FloorPlanImageUrl ?? "https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=300&q=80",
            buildingId = f.BuildingId.ToString()
        });
        return Ok(result);
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPost("floors")]
    public async Task<IActionResult> CreateFloor([FromBody] CreateFloorRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var buildingRepo = _unitOfWork.Repository<Building>();
        var siteRepo = _unitOfWork.Repository<Site>();

        Guid targetBuildingId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(req.BuildingId) && Guid.TryParse(req.BuildingId, out var parsedBldgId) && parsedBldgId != Guid.Empty)
        {
            var bldgExists = await buildingRepo.GetByIdAsync(parsedBldgId, cancellationToken);
            if (bldgExists != null)
            {
                targetBuildingId = bldgExists.Id;
            }
        }

        if (targetBuildingId == Guid.Empty)
        {
            var existingBldgs = await buildingRepo.FindAsync(x => true, cancellationToken);
            var firstBldg = existingBldgs.FirstOrDefault();
            if (firstBldg != null)
            {
                targetBuildingId = firstBldg.Id;
            }
            else
            {
                var sites = await siteRepo.FindAsync(x => true, cancellationToken);
                var site = sites.FirstOrDefault();
                if (site == null)
                {
                    site = new Site
                    {
                        ApplicationId = _tenantProvider.ApplicationId,
                        Name = "Main Site",
                        Code = $"site_main_{Guid.NewGuid().ToString("N")[..6]}"
                    };
                    await siteRepo.AddAsync(site, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                var defaultBldg = new Building
                {
                    ApplicationId = _tenantProvider.ApplicationId,
                    Name = "Main Building",
                    BuildingCode = $"bldg_main_{Guid.NewGuid().ToString("N")[..6]}",
                    SiteId = site.Id,
                    TotalFloors = 1
                };
                await buildingRepo.AddAsync(defaultBldg, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                targetBuildingId = defaultBldg.Id;
            }
        }

        var existingFloors = await repo.FindAsync(f => f.BuildingId == targetBuildingId, cancellationToken);
        int nextFloorLevel = existingFloors.Any() ? existingFloors.Max(f => f.FloorLevel) + 1 : 1;

        var floor = new Floor
        {
            ApplicationId = _tenantProvider.ApplicationId,
            BuildingId = targetBuildingId,
            Name = req.Name,
            FloorLevel = nextFloorLevel,
            FloorPlanImageUrl = req.Image
        };
        await repo.AddAsync(floor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = floor.Id.ToString(), name = floor.Name, description = req.Description, image = req.Image, buildingId = floor.BuildingId.ToString() });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPut("floors/{id}")]
    public async Task<IActionResult> UpdateFloor(Guid id, [FromBody] CreateFloorRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var floor = await repo.GetByIdAsync(id, cancellationToken);
        if (floor == null) return NotFound();

        floor.Name = req.Name;
        floor.FloorPlanImageUrl = req.Image;
        repo.Update(floor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = floor.Id.ToString(), name = floor.Name, description = req.Description, image = req.Image, buildingId = floor.BuildingId.ToString() });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpDelete("floors/{id}")]
    public async Task<IActionResult> DeleteFloor(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var floor = await repo.GetByIdAsync(id, cancellationToken);
        if (floor == null) return NotFound();

        repo.Delete(floor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ==========================================
    // 3. ROOMS / AREAS (Spatial Entity)
    // ==========================================

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms(
        [FromQuery] Guid? floorId,
        [FromQuery] bool? isActive,
        [FromQuery] string? searchTerm,
        [FromQuery] string? sortBy,
        [FromQuery] bool isDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var query = repo.GetQueryable();

        if (floorId.HasValue && floorId.Value != Guid.Empty)
        {
            query = query.Where(r => r.FloorId == floorId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(term) || r.Code.ToLower().Contains(term) || (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        query = sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "code" => isDescending ? query.OrderByDescending(r => r.Code) : query.OrderBy(r => r.Code),
            "sortorder" => isDescending ? query.OrderByDescending(r => r.SortOrder) : query.OrderBy(r => r.SortOrder),
            "createdat" => isDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            _ => query.OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var rooms = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var result = rooms.Select(r => new
        {
            id = r.Id.ToString(),
            name = r.Name,
            code = r.Code,
            description = r.Description ?? $"Room in Floor {r.FloorId}",
            sortOrder = r.SortOrder,
            isActive = r.IsActive,
            image = "https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=300&q=80",
            floorId = r.FloorId.ToString(),
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt
        });

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(result);
    }

    [HttpGet("rooms/{id}")]
    public async Task<IActionResult> GetRoomById(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id, cancellationToken);
        if (room == null) return NotFound();

        return Ok(new
        {
            id = room.Id.ToString(),
            name = room.Name,
            code = room.Code,
            description = room.Description,
            sortOrder = room.SortOrder,
            isActive = room.IsActive,
            image = "https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=300&q=80",
            floorId = room.FloorId.ToString(),
            createdAt = room.CreatedAt,
            updatedAt = room.UpdatedAt
        });
    }

    [HttpGet("rooms/hierarchy")]
    public async Task<IActionResult> GetRoomHierarchy(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Room>();
        var rooms = await repo.GetQueryable()
            .Include(r => r.Cameras)
                .ThenInclude(c => c.Zones)
            .ToListAsync(cancellationToken);

        var result = rooms.Select(r => new
        {
            id = r.Id.ToString(),
            name = r.Name,
            code = r.Code,
            description = r.Description,
            floorId = r.FloorId.ToString(),
            sortOrder = r.SortOrder,
            isActive = r.IsActive,
            cameraCount = r.Cameras.Count,
            zoneCount = r.Cameras.Sum(c => c.Zones.Count),
            cameras = r.Cameras.Select(c => new { id = c.Id.ToString(), name = c.Name, status = c.Status.ToString().ToLower() })
        });

        return Ok(result);
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Room>();
        var floorRepo = _unitOfWork.Repository<Floor>();

        Guid targetFloorId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(req.FloorId) && Guid.TryParse(req.FloorId, out var parsedFloorId) && parsedFloorId != Guid.Empty)
        {
            var floorExists = await floorRepo.GetByIdAsync(parsedFloorId, cancellationToken);
            if (floorExists != null)
            {
                targetFloorId = floorExists.Id;
            }
        }

        if (targetFloorId == Guid.Empty)
        {
            var floors = await floorRepo.FindAsync(x => true, cancellationToken);
            var firstFloor = floors.FirstOrDefault();
            if (firstFloor != null)
            {
                targetFloorId = firstFloor.Id;
            }
            else
            {
                var buildingRepo = _unitOfWork.Repository<Building>();
                var siteRepo = _unitOfWork.Repository<Site>();
                var sites = await siteRepo.FindAsync(x => true, cancellationToken);
                var site = sites.FirstOrDefault();
                if (site == null)
                {
                    site = new Site { ApplicationId = _tenantProvider.ApplicationId, Name = "Main Site", Code = $"site_main_{Guid.NewGuid().ToString("N")[..6]}" };
                    await siteRepo.AddAsync(site, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                var defaultBldg = new Building { ApplicationId = _tenantProvider.ApplicationId, Name = "Main Building", BuildingCode = $"bldg_main_{Guid.NewGuid().ToString("N")[..6]}", SiteId = site.Id, TotalFloors = 1 };
                await buildingRepo.AddAsync(defaultBldg, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                firstFloor = new Floor { ApplicationId = _tenantProvider.ApplicationId, BuildingId = defaultBldg.Id, Name = "Ground Floor", FloorLevel = 1 };
                await floorRepo.AddAsync(firstFloor, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                targetFloorId = firstFloor.Id;
            }
        }

        var baseCode = string.IsNullOrWhiteSpace(req.Code) ? req.Name.ToLower().Replace(" ", "_") : req.Code;
        var existingRooms = await repo.FindAsync(r => r.FloorId == targetFloorId && r.Code == baseCode, cancellationToken);
        var uniqueCode = existingRooms.Any() ? $"{baseCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseCode;

        var room = new Room
        {
            ApplicationId = _tenantProvider.ApplicationId,
            FloorId = targetFloorId,
            Name = req.Name,
            Code = uniqueCode,
            Description = req.Description,
            SortOrder = 0,
            IsActive = true
        };

        await repo.AddAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            id = room.Id.ToString(),
            name = room.Name,
            code = room.Code,
            description = room.Description,
            sortOrder = room.SortOrder,
            isActive = room.IsActive,
            image = req.Image ?? "https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=300&q=80",
            floorId = room.FloorId.ToString()
        });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPut("rooms/{id}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] CreateRoomRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id, cancellationToken);
        if (room == null) return NotFound();

        room.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Description)) room.Description = req.Description;
        if (!string.IsNullOrWhiteSpace(req.Code)) room.Code = req.Code;

        repo.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            id = room.Id.ToString(),
            name = room.Name,
            code = room.Code,
            description = room.Description,
            sortOrder = room.SortOrder,
            isActive = room.IsActive,
            image = req.Image ?? "https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=300&q=80",
            floorId = room.FloorId.ToString()
        });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpDelete("rooms/{id}")]
    public async Task<IActionResult> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id, cancellationToken);
        if (room == null) return NotFound();

        repo.Delete(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // ==========================================
    // 4. CAMERAS
    // ==========================================

    [HttpGet("cameras")]
    public async Task<IActionResult> GetCameras(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var cameras = await repo.FindAsync(x => true, cancellationToken);
        var result = cameras.Select(c => new
        {
            id = c.Id.ToString(),
            name = c.Name,
            description = $"RTSP Endpoint Stream ({c.Resolution})",
            rtspUrl = string.IsNullOrWhiteSpace(c.RtspStreamUrl) ? "rtsp://admin:Admin1234@192.168.1.64/Streaming/channels/2" : c.RtspStreamUrl,
            roomId = c.RoomId?.ToString(),
            status = c.Status.ToString().ToLower()
        });
        return Ok(result);
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPost("cameras")]
    public async Task<IActionResult> CreateCamera([FromBody] CreateCameraRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var roomRepo = _unitOfWork.Repository<Room>();

        Guid? targetRoomId = null;
        if (!string.IsNullOrWhiteSpace(req.RoomId) && Guid.TryParse(req.RoomId, out var parsedRoomId) && parsedRoomId != Guid.Empty)
        {
            var roomExists = await roomRepo.GetByIdAsync(parsedRoomId, cancellationToken);
            if (roomExists != null)
            {
                targetRoomId = roomExists.Id;
            }
        }

        var baseDeviceCode = req.Name.ToLower().Replace(" ", "_");
        var existingCams = await repo.FindAsync(c => c.DeviceCode == baseDeviceCode, cancellationToken);
        var uniqueDeviceCode = existingCams.Any() ? $"{baseDeviceCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseDeviceCode;

        var camera = new Camera
        {
            ApplicationId = _tenantProvider.ApplicationId,
            RoomId = targetRoomId,
            Name = req.Name,
            DeviceCode = uniqueDeviceCode,
            RtspStreamUrl = req.RtspUrl,
            IpAddress = "192.168.1.202"
        };
        await repo.AddAsync(camera, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = camera.Id.ToString(), name = camera.Name, description = req.Description, rtspUrl = camera.RtspStreamUrl, roomId = camera.RoomId?.ToString() ?? req.RoomId, status = "online" });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPut("cameras/{id}")]
    public async Task<IActionResult> UpdateCamera(Guid id, [FromBody] CreateCameraRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var roomRepo = _unitOfWork.Repository<Room>();
        var camera = await repo.GetByIdAsync(id, cancellationToken);
        if (camera == null) return NotFound();

        camera.Name = req.Name;
        camera.RtspStreamUrl = req.RtspUrl;

        if (!string.IsNullOrWhiteSpace(req.RoomId) && Guid.TryParse(req.RoomId, out var parsedRoomId) && parsedRoomId != Guid.Empty)
        {
            var roomExists = await roomRepo.GetByIdAsync(parsedRoomId, cancellationToken);
            if (roomExists != null)
            {
                camera.RoomId = roomExists.Id;
            }
        }

        repo.Update(camera);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = camera.Id.ToString(), name = camera.Name, description = req.Description, rtspUrl = camera.RtspStreamUrl, roomId = camera.RoomId?.ToString() ?? req.RoomId, status = "online" });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpDelete("cameras/{id}")]
    public async Task<IActionResult> DeleteCamera(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var camera = await repo.GetByIdAsync(id, cancellationToken);
        if (camera == null) return NotFound();

        repo.Delete(camera);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string ExtractPolygonJson(object? cameraConfigs)
    {
        if (cameraConfigs == null) return "[]";

        try
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(cameraConfigs);
            using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstEl = root[0];
                if (firstEl.ValueKind == System.Text.Json.JsonValueKind.Object && firstEl.TryGetProperty("polygon", out var polyProp))
                {
                    return polyProp.GetRawText();
                }
                else if (firstEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return root.GetRawText();
                }
            }
            return rawJson;
        }
        catch
        {
            return "[]";
        }
    }

    // ==========================================
    // 5. ZONES
    // ==========================================

    [HttpGet("zones")]
    public async Task<IActionResult> GetZones(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var cameraRepo = _unitOfWork.Repository<Camera>();
        var zones = await repo.FindAsync(x => true, cancellationToken);
        var cameras = await cameraRepo.FindAsync(x => true, cancellationToken);
        var camDict = cameras.ToDictionary(c => c.Id);

        var result = zones.Select(z =>
        {
            object? polygonPoints = null;
            if (!string.IsNullOrWhiteSpace(z.PolygonCoordinatesJson))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(z.PolygonCoordinatesJson);
                    var root = doc.RootElement;
                    if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0 && root[0].ValueKind == System.Text.Json.JsonValueKind.Object && root[0].TryGetProperty("polygon", out var p))
                    {
                        polygonPoints = System.Text.Json.JsonSerializer.Deserialize<object>(p.GetRawText());
                    }
                    else
                    {
                        polygonPoints = System.Text.Json.JsonSerializer.Deserialize<object>(z.PolygonCoordinatesJson);
                    }
                }
                catch { }
            }

            var poly = polygonPoints ?? new[] { new[] { 0.1, 0.2 }, new[] { 0.9, 0.2 }, new[] { 0.9, 0.8 }, new[] { 0.1, 0.8 } };

            camDict.TryGetValue(z.CameraId, out var cam);
            var actualRoomId = cam?.RoomId?.ToString() ?? string.Empty;

            return new
            {
                id = z.Id.ToString(),
                name = z.Name,
                roomId = actualRoomId,
                cameraConfigs = new[]
                {
                    new
                    {
                        cameraId = z.CameraId.ToString(),
                        polygon = poly
                    }
                }
            };
        });
        return Ok(result);
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPost("zones")]
    public async Task<IActionResult> CreateZone([FromBody] CreateZoneRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var cameraRepo = _unitOfWork.Repository<Camera>();

        Guid targetCameraId = Guid.Empty;

        // 1. Try to extract cameraId directly from req.CameraConfigs
        if (req.CameraConfigs != null && req.CameraConfigs.Length > 0)
        {
            try
            {
                var rawJson = System.Text.Json.JsonSerializer.Serialize(req.CameraConfigs);
                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var root = doc.RootElement;
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstEl = root[0];
                    if (firstEl.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        firstEl.TryGetProperty("cameraId", out var camIdProp) &&
                        Guid.TryParse(camIdProp.GetString(), out var parsedCamId) &&
                        parsedCamId != Guid.Empty)
                    {
                        var camExists = await cameraRepo.GetByIdAsync(parsedCamId, cancellationToken);
                        if (camExists != null)
                        {
                            targetCameraId = camExists.Id;
                        }
                    }
                }
            }
            catch { }
        }

        // 2. Try looking up req.RoomId directly as a Camera ID
        if (targetCameraId == Guid.Empty && !string.IsNullOrWhiteSpace(req.RoomId) && Guid.TryParse(req.RoomId, out var parsedId) && parsedId != Guid.Empty)
        {
            var camExists = await cameraRepo.GetByIdAsync(parsedId, cancellationToken);
            if (camExists != null)
            {
                targetCameraId = camExists.Id;
            }
            else
            {
                // 3. Or check if req.RoomId is a Room ID, and find a camera in that room
                var roomCams = await cameraRepo.FindAsync(c => c.RoomId == parsedId, cancellationToken);
                var roomCam = roomCams.FirstOrDefault();
                if (roomCam != null)
                {
                    targetCameraId = roomCam.Id;
                }
            }
        }

        if (targetCameraId == Guid.Empty)
        {
            var cameras = await cameraRepo.FindAsync(x => true, cancellationToken);
            var firstCam = cameras.FirstOrDefault();
            if (firstCam != null)
            {
                targetCameraId = firstCam.Id;
            }
            else
            {
                var roomRepo = _unitOfWork.Repository<Room>();
                var rooms = await roomRepo.FindAsync(x => true, cancellationToken);
                var firstRoom = rooms.FirstOrDefault();

                firstCam = new Camera { ApplicationId = _tenantProvider.ApplicationId, RoomId = firstRoom?.Id, Name = "Default Camera", DeviceCode = $"cam_default_{Guid.NewGuid().ToString("N")[..6]}", RtspStreamUrl = "rtsp://localhost/live" };
                await cameraRepo.AddAsync(firstCam, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                targetCameraId = firstCam.Id;
            }
        }

        var baseZoneName = req.Name;
        var existingZones = await repo.FindAsync(z => z.CameraId == targetCameraId && z.Name == baseZoneName, cancellationToken);
        var uniqueZoneName = existingZones.Any() ? $"{baseZoneName}_{Guid.NewGuid().ToString("N")[..4]}" : baseZoneName;

        var jsonPolygon = ExtractPolygonJson(req.CameraConfigs);

        var zone = new Zone
        {
            ApplicationId = _tenantProvider.ApplicationId,
            Name = uniqueZoneName,
            CameraId = targetCameraId,
            PolygonCoordinatesJson = jsonPolygon
        };
        await repo.AddAsync(zone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var targetCam = await cameraRepo.GetByIdAsync(targetCameraId, cancellationToken);
        var actualRoomId = targetCam?.RoomId?.ToString() ?? (!string.IsNullOrWhiteSpace(req.RoomId) ? req.RoomId : string.Empty);

        return Ok(new { 
            id = zone.Id.ToString(), 
            name = zone.Name, 
            roomId = actualRoomId, 
            cameraConfigs = new[]
            {
                new
                {
                    cameraId = targetCameraId.ToString(),
                    polygon = System.Text.Json.JsonSerializer.Deserialize<object>(jsonPolygon)
                }
            }
        });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpPut("zones/{id}")]
    public async Task<IActionResult> UpdateZone(Guid id, [FromBody] CreateZoneRequest req, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var cameraRepo = _unitOfWork.Repository<Camera>();
        var zone = await repo.GetByIdAsync(id, cancellationToken);
        if (zone == null) return NotFound();

        zone.Name = req.Name;
        if (req.CameraConfigs != null)
        {
            zone.PolygonCoordinatesJson = ExtractPolygonJson(req.CameraConfigs);

            // If a camera config specifies a new camera ID, update target camera
            try
            {
                var rawJson = System.Text.Json.JsonSerializer.Serialize(req.CameraConfigs);
                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var root = doc.RootElement;
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstEl = root[0];
                    if (firstEl.ValueKind == System.Text.Json.JsonValueKind.Object &&
                        firstEl.TryGetProperty("cameraId", out var camIdProp) &&
                        Guid.TryParse(camIdProp.GetString(), out var parsedCamId) &&
                        parsedCamId != Guid.Empty)
                    {
                        var camExists = await cameraRepo.GetByIdAsync(parsedCamId, cancellationToken);
                        if (camExists != null)
                        {
                            zone.CameraId = camExists.Id;
                        }
                    }
                }
            }
            catch { }
        }
        repo.Update(zone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var cam = await cameraRepo.GetByIdAsync(zone.CameraId, cancellationToken);
        var actualRoomId = cam?.RoomId?.ToString() ?? (!string.IsNullOrWhiteSpace(req.RoomId) ? req.RoomId : string.Empty);

        return Ok(new { 
            id = zone.Id.ToString(), 
            name = zone.Name, 
            roomId = actualRoomId, 
            cameraConfigs = new[]
            {
                new
                {
                    cameraId = zone.CameraId.ToString(),
                    polygon = System.Text.Json.JsonSerializer.Deserialize<object>(zone.PolygonCoordinatesJson)
                }
            }
        });
    }

    [RequirePermission(AppPermissions.Spatial.Write)]
    [HttpDelete("zones/{id}")]
    public async Task<IActionResult> DeleteZone(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var zone = await repo.GetByIdAsync(id, cancellationToken);
        if (zone == null) return NotFound();

        repo.Delete(zone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public record CreateSiteRequest(string Name, string Description, string? Code, string? Image);
public record CreateBuildingRequest(string Name, string Description, string? Image, string? SiteId = null);
public record CreateFloorRequest(string Name, string Description, string? Image, string BuildingId);
public record CreateRoomRequest(string Name, string Description, string? Image, string FloorId, string? Code = null);
public record CreateCameraRequest(string Name, string Description, string RtspUrl, string RoomId);
public record CreateZoneRequest(string Name, string RoomId, object[]? CameraConfigs);
