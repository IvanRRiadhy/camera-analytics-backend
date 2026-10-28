using System.Text.Json;
using CCTV.Application.Common.Extensions;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Spatial.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Contracts.Spatial;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Exceptions;
using CCTV.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Application.Spatial.Services;

public class SpatialService : ISpatialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    private const string DefaultBuildingImage = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=300&q=80";
    private const string DefaultFloorImage = "https://images.unsplash.com/photo-1497366216548-37526070297c?auto=format&fit=crop&w=300&q=80";
    private const string DefaultRoomImage = "https://images.unsplash.com/photo-1497366811353-6870744d04b2?auto=format&fit=crop&w=300&q=80";

    public SpatialService(IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyList<SiteDto>> GetSitesAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Site>();
        var items = await repo.FindAsync(_ => true, cancellationToken);
        return items.Select(s => new SiteDto(
            s.Id.ToString(),
            s.Name,
            s.Code,
            $"Site Code: {s.Code}",
            DefaultBuildingImage
        )).ToList();
    }

    public async Task<SiteDto> CreateSiteAsync(CreateSiteRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Site>();
        var baseCode = string.IsNullOrWhiteSpace(req.Code) ? req.Name.ToLower().Replace(" ", "_") : req.Code;
        var existingSites = await repo.FindAsync(s => s.Code == baseCode, cancellationToken);
        var uniqueCode = existingSites.Count > 0 ? $"{baseCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseCode;

        var site = new Site
        {
            ApplicationId = _tenantProvider.ApplicationId,
            Name = req.Name,
            Code = uniqueCode
        };

        await repo.AddAsync(site, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SiteDto(site.Id.ToString(), site.Name, site.Code, req.Description, req.Image ?? DefaultBuildingImage);
    }

    public async Task<SiteDto> UpdateSiteAsync(Guid id, CreateSiteRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Site>();
        var site = await repo.GetByIdAsync(id, cancellationToken);
        if (site == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.SiteNotFound);
        }

        site.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Code))
        {
            site.Code = req.Code;
        }

        repo.Update(site);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SiteDto(site.Id.ToString(), site.Name, site.Code, req.Description, req.Image ?? DefaultBuildingImage);
    }

    public async Task DeleteSiteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Site>();
        var site = await repo.GetByIdAsync(id, cancellationToken);
        if (site == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.SiteNotFound);
        }

        repo.Delete(site);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BuildingDto>> GetBuildingsAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Building>();
        var items = await repo.FindAsync(_ => true, cancellationToken);
        return items.Select(b => new BuildingDto(
            b.Id.ToString(),
            b.Name,
            $"Building Code: {b.BuildingCode}",
            DefaultBuildingImage,
            b.SiteId.ToString()
        )).ToList();
    }

    public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Building>();
        var siteRepo = _unitOfWork.Repository<Site>();

        var targetSiteId = Guid.Empty;
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
            var sites = await siteRepo.FindAsync(_ => true, cancellationToken);
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
        var uniqueBldgCode = existingBldgs.Count > 0 ? $"{baseBldgCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseBldgCode;

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

        return new BuildingDto(building.Id.ToString(), building.Name, req.Description, req.Image ?? DefaultBuildingImage, building.SiteId.ToString());
    }

    public async Task<BuildingDto> UpdateBuildingAsync(Guid id, CreateBuildingRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Building>();
        var siteRepo = _unitOfWork.Repository<Site>();
        var building = await repo.GetByIdAsync(id, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.BuildingNotFound);
        }

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

        return new BuildingDto(building.Id.ToString(), building.Name, req.Description, req.Image ?? DefaultBuildingImage, building.SiteId.ToString());
    }

    public async Task DeleteBuildingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Building>();
        var building = await repo.GetByIdAsync(id, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.BuildingNotFound);
        }

        repo.Delete(building);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FloorDto>> GetFloorsAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var items = await repo.FindAsync(_ => true, cancellationToken);
        return items.Select(f => new FloorDto(
            f.Id.ToString(),
            f.Name,
            $"Level {f.FloorLevel}",
            f.FloorPlanImageUrl ?? DefaultFloorImage,
            f.BuildingId.ToString()
        )).ToList();
    }

    public async Task<FloorDto> CreateFloorAsync(CreateFloorRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var buildingRepo = _unitOfWork.Repository<Building>();
        var siteRepo = _unitOfWork.Repository<Site>();

        var targetBuildingId = Guid.Empty;
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
            var existingBldgs = await buildingRepo.FindAsync(_ => true, cancellationToken);
            var firstBldg = existingBldgs.FirstOrDefault();
            if (firstBldg != null)
            {
                targetBuildingId = firstBldg.Id;
            }
            else
            {
                var sites = await siteRepo.FindAsync(_ => true, cancellationToken);
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
        var nextFloorLevel = existingFloors.Count > 0 ? existingFloors.Max(f => f.FloorLevel) + 1 : 1;

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

        return new FloorDto(floor.Id.ToString(), floor.Name, req.Description, req.Image ?? DefaultFloorImage, floor.BuildingId.ToString());
    }

    public async Task<FloorDto> UpdateFloorAsync(Guid id, CreateFloorRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var floor = await repo.GetByIdAsync(id, cancellationToken);
        if (floor == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.FloorNotFound);
        }

        floor.Name = req.Name;
        floor.FloorPlanImageUrl = req.Image;
        repo.Update(floor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FloorDto(floor.Id.ToString(), floor.Name, req.Description, req.Image ?? DefaultFloorImage, floor.BuildingId.ToString());
    }

    public async Task DeleteFloorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Floor>();
        var floor = await repo.GetByIdAsync(id, cancellationToken);
        if (floor == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.FloorNotFound);
        }

        repo.Delete(floor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<RoomDetailDto>> GetRoomsPagedAsync(RoomFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var query = repo.GetQueryable();

        if (filter.FloorId.HasValue && filter.FloorId.Value != Guid.Empty)
        {
            query = query.Where(r => r.FloorId == filter.FloorId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(term) || r.Code.ToLower().Contains(term) || (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        query = filter.SortBy?.ToLower() switch
        {
            "name" => filter.Desc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "code" => filter.Desc ? query.OrderByDescending(r => r.Code) : query.OrderBy(r => r.Code),
            "sortorder" => filter.Desc ? query.OrderByDescending(r => r.SortOrder) : query.OrderBy(r => r.SortOrder),
            "createdat" => filter.Desc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            _ => query.OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
        };

        return await query.ToPagedResultAsync(
            filter,
            r => new RoomDetailDto(
                r.Id.ToString(),
                r.Name,
                r.Code,
                r.Description ?? $"Room in Floor {r.FloorId}",
                r.SortOrder,
                r.IsActive,
                DefaultRoomImage,
                r.FloorId.ToString(),
                r.CreatedAt,
                r.UpdatedAt
            ),
            cancellationToken);
    }

    public async Task<RoomDetailDto?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id, cancellationToken);
        if (room == null)
        {
            return null;
        }

        return new RoomDetailDto(
            room.Id.ToString(),
            room.Name,
            room.Code,
            room.Description,
            room.SortOrder,
            room.IsActive,
            DefaultRoomImage,
            room.FloorId.ToString(),
            room.CreatedAt,
            room.UpdatedAt
        );
    }

    public async Task<IReadOnlyList<RoomHierarchyResponseDto>> GetRoomHierarchyAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var rooms = await repo.GetQueryable()
            .Include(r => r.Cameras)
                .ThenInclude(c => c.Zones)
            .ToListAsync(cancellationToken);

        return rooms.Select(r => new RoomHierarchyResponseDto(
            r.Id.ToString(),
            r.Name,
            r.Code,
            r.Description,
            r.FloorId.ToString(),
            r.SortOrder,
            r.IsActive,
            r.Cameras.Count,
            r.Cameras.Sum(c => c.Zones.Count),
            r.Cameras.Select(c => new CameraStatusDto(c.Id.ToString(), c.Name, c.Status.ToString().ToLower())).ToList()
        )).ToList();
    }

    public async Task<RoomDetailDto> CreateRoomAsync(CreateRoomRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var floorRepo = _unitOfWork.Repository<Floor>();

        var targetFloorId = Guid.Empty;
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
            var floors = await floorRepo.FindAsync(_ => true, cancellationToken);
            var firstFloor = floors.FirstOrDefault();
            if (firstFloor != null)
            {
                targetFloorId = firstFloor.Id;
            }
            else
            {
                var buildingRepo = _unitOfWork.Repository<Building>();
                var siteRepo = _unitOfWork.Repository<Site>();
                var sites = await siteRepo.FindAsync(_ => true, cancellationToken);
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
        var uniqueCode = existingRooms.Count > 0 ? $"{baseCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseCode;

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

        return new RoomDetailDto(
            room.Id.ToString(),
            room.Name,
            room.Code,
            room.Description,
            room.SortOrder,
            room.IsActive,
            req.Image ?? DefaultRoomImage,
            room.FloorId.ToString(),
            room.CreatedAt,
            room.UpdatedAt
        );
    }

    public async Task<RoomDetailDto> UpdateRoomAsync(Guid id, CreateRoomRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id, cancellationToken);
        if (room == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.RoomNotFound);
        }

        room.Name = req.Name;
        if (!string.IsNullOrWhiteSpace(req.Description))
        {
            room.Description = req.Description;
        }
        if (!string.IsNullOrWhiteSpace(req.Code))
        {
            room.Code = req.Code;
        }

        repo.Update(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoomDetailDto(
            room.Id.ToString(),
            room.Name,
            room.Code,
            room.Description,
            room.SortOrder,
            room.IsActive,
            req.Image ?? DefaultRoomImage,
            room.FloorId.ToString(),
            room.CreatedAt,
            room.UpdatedAt
        );
    }

    public async Task DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Room>();
        var room = await repo.GetByIdAsync(id, cancellationToken);
        if (room == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.RoomNotFound);
        }

        repo.Delete(room);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CameraDetailDto>> GetCamerasAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var cameras = await repo.FindAsync(_ => true, cancellationToken);
        return cameras.Select(c => new CameraDetailDto(
            c.Id.ToString(),
            c.Name,
            $"RTSP Endpoint Stream ({c.Resolution})",
            string.IsNullOrWhiteSpace(c.RtspStreamUrl) ? "rtsp://admin:Admin1234@192.168.1.64/Streaming/channels/2" : c.RtspStreamUrl,
            c.RoomId?.ToString(),
            c.Status.ToString().ToLower()
        )).ToList();
    }

    public async Task<CameraDetailDto> CreateCameraAsync(CreateCameraRequest req, CancellationToken cancellationToken = default)
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
        var uniqueDeviceCode = existingCams.Count > 0 ? $"{baseDeviceCode}_{Guid.NewGuid().ToString("N")[..6]}" : baseDeviceCode;

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

        return new CameraDetailDto(camera.Id.ToString(), camera.Name, req.Description, camera.RtspStreamUrl, camera.RoomId?.ToString() ?? req.RoomId, "online");
    }

    public async Task<CameraDetailDto> UpdateCameraAsync(Guid id, CreateCameraRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var roomRepo = _unitOfWork.Repository<Room>();
        var camera = await repo.GetByIdAsync(id, cancellationToken);
        if (camera == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.CameraNotFound);
        }

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

        return new CameraDetailDto(camera.Id.ToString(), camera.Name, req.Description, camera.RtspStreamUrl, camera.RoomId?.ToString() ?? req.RoomId, "online");
    }

    public async Task DeleteCameraAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Camera>();
        var camera = await repo.GetByIdAsync(id, cancellationToken);
        if (camera == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.CameraNotFound);
        }

        repo.Delete(camera);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ZoneDetailDto>> GetZonesAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var cameraRepo = _unitOfWork.Repository<Camera>();
        var zones = await repo.FindAsync(_ => true, cancellationToken);
        var cameras = await cameraRepo.FindAsync(_ => true, cancellationToken);
        var camDict = cameras.ToDictionary(c => c.Id);

        return zones.Select(z =>
        {
            var poly = ParsePolygonPoints(z.PolygonCoordinatesJson);
            camDict.TryGetValue(z.CameraId, out var cam);
            var actualRoomId = cam?.RoomId?.ToString() ?? string.Empty;

            return new ZoneDetailDto(
                z.Id.ToString(),
                z.Name,
                actualRoomId,
                new List<ZoneCameraConfigDto>
                {
                    new(z.CameraId.ToString(), poly)
                }
            );
        }).ToList();
    }

    public async Task<ZoneDetailDto> CreateZoneAsync(CreateZoneRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var cameraRepo = _unitOfWork.Repository<Camera>();

        var targetCameraId = await ResolveTargetCameraIdAsync(req, cameraRepo, cancellationToken);

        var baseZoneName = req.Name;
        var existingZones = await repo.FindAsync(z => z.CameraId == targetCameraId && z.Name == baseZoneName, cancellationToken);
        var uniqueZoneName = existingZones.Count > 0 ? $"{baseZoneName}_{Guid.NewGuid().ToString("N")[..4]}" : baseZoneName;

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

        return new ZoneDetailDto(
            zone.Id.ToString(),
            zone.Name,
            actualRoomId,
            new List<ZoneCameraConfigDto>
            {
                new(targetCameraId.ToString(), JsonSerializer.Deserialize<object>(jsonPolygon))
            }
        );
    }

    public async Task<ZoneDetailDto> UpdateZoneAsync(Guid id, CreateZoneRequest req, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var cameraRepo = _unitOfWork.Repository<Camera>();
        var zone = await repo.GetByIdAsync(id, cancellationToken);
        if (zone == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.ZoneNotFound);
        }

        zone.Name = req.Name;
        if (req.CameraConfigs != null)
        {
            zone.PolygonCoordinatesJson = ExtractPolygonJson(req.CameraConfigs);

            var extractedCameraId = ExtractCameraIdFromConfigs(req.CameraConfigs);
            if (extractedCameraId.HasValue)
            {
                var camExists = await cameraRepo.GetByIdAsync(extractedCameraId.Value, cancellationToken);
                if (camExists != null)
                {
                    zone.CameraId = camExists.Id;
                }
            }
        }

        repo.Update(zone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var cam = await cameraRepo.GetByIdAsync(zone.CameraId, cancellationToken);
        var actualRoomId = cam?.RoomId?.ToString() ?? (!string.IsNullOrWhiteSpace(req.RoomId) ? req.RoomId : string.Empty);

        return new ZoneDetailDto(
            zone.Id.ToString(),
            zone.Name,
            actualRoomId,
            new List<ZoneCameraConfigDto>
            {
                new(zone.CameraId.ToString(), JsonSerializer.Deserialize<object>(zone.PolygonCoordinatesJson))
            }
        );
    }

    public async Task DeleteZoneAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Zone>();
        var zone = await repo.GetByIdAsync(id, cancellationToken);
        if (zone == null)
        {
            throw new NotFoundException(ResponseMessages.Spatial.ZoneNotFound);
        }

        repo.Delete(zone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static object ParsePolygonPoints(string? coordinatesJson)
    {
        if (string.IsNullOrWhiteSpace(coordinatesJson))
        {
            return DefaultPolygon();
        }

        try
        {
            using var doc = JsonDocument.Parse(coordinatesJson);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.Object && root[0].TryGetProperty("polygon", out var p))
            {
                return JsonSerializer.Deserialize<object>(p.GetRawText()) ?? DefaultPolygon();
            }
            return JsonSerializer.Deserialize<object>(coordinatesJson) ?? DefaultPolygon();
        }
        catch
        {
            return DefaultPolygon();
        }
    }

    private static object DefaultPolygon() => new[] { new[] { 0.1, 0.2 }, new[] { 0.9, 0.2 }, new[] { 0.9, 0.8 }, new[] { 0.1, 0.8 } };

    private static string ExtractPolygonJson(object? cameraConfigs)
    {
        if (cameraConfigs == null) return "[]";

        try
        {
            var rawJson = JsonSerializer.Serialize(cameraConfigs);
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstEl = root[0];
                if (firstEl.ValueKind == JsonValueKind.Object && firstEl.TryGetProperty("polygon", out var polyProp))
                {
                    return polyProp.GetRawText();
                }
                if (firstEl.ValueKind == JsonValueKind.Array)
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

    private static Guid? ExtractCameraIdFromConfigs(object? cameraConfigs)
    {
        if (cameraConfigs == null) return null;

        try
        {
            var rawJson = JsonSerializer.Serialize(cameraConfigs);
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstEl = root[0];
                if (firstEl.ValueKind == JsonValueKind.Object &&
                    firstEl.TryGetProperty("cameraId", out var camIdProp) &&
                    Guid.TryParse(camIdProp.GetString(), out var parsedCamId) &&
                    parsedCamId != Guid.Empty)
                {
                    return parsedCamId;
                }
            }
        }
        catch
        {
            // Invalid JSON structure ignored
        }

        return null;
    }

    private async Task<Guid> ResolveTargetCameraIdAsync(CreateZoneRequest req, IGenericRepository<Camera> cameraRepo, CancellationToken cancellationToken)
    {
        var extractedCamId = ExtractCameraIdFromConfigs(req.CameraConfigs);
        if (extractedCamId.HasValue)
        {
            var camExists = await cameraRepo.GetByIdAsync(extractedCamId.Value, cancellationToken);
            if (camExists != null)
            {
                return camExists.Id;
            }
        }

        if (!string.IsNullOrWhiteSpace(req.RoomId) && Guid.TryParse(req.RoomId, out var parsedId) && parsedId != Guid.Empty)
        {
            var camExists = await cameraRepo.GetByIdAsync(parsedId, cancellationToken);
            if (camExists != null)
            {
                return camExists.Id;
            }

            var roomCams = await cameraRepo.FindAsync(c => c.RoomId == parsedId, cancellationToken);
            var roomCam = roomCams.FirstOrDefault();
            if (roomCam != null)
            {
                return roomCam.Id;
            }
        }

        var cameras = await cameraRepo.FindAsync(_ => true, cancellationToken);
        var firstCam = cameras.FirstOrDefault();
        if (firstCam != null)
        {
            return firstCam.Id;
        }

        var roomRepo = _unitOfWork.Repository<Room>();
        var rooms = await roomRepo.FindAsync(_ => true, cancellationToken);
        var firstRoom = rooms.FirstOrDefault();

        firstCam = new Camera
        {
            ApplicationId = _tenantProvider.ApplicationId,
            RoomId = firstRoom?.Id,
            Name = "Default Camera",
            DeviceCode = $"cam_default_{Guid.NewGuid().ToString("N")[..6]}",
            RtspStreamUrl = "rtsp://localhost/live"
        };
        await cameraRepo.AddAsync(firstCam, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return firstCam.Id;
    }
}
