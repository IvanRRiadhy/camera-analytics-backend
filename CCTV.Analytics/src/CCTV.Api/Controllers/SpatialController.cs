using CCTV.Api.Security;
using CCTV.Application.Spatial.Interfaces;
using CCTV.Contracts.Common;
using CCTV.Contracts.Spatial;
using CCTV.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/spatial")]
[RequirePermission(AppPermissions.Spatial.Read)]
public class SpatialController : ControllerBase
{
    private readonly ISpatialService _spatialService;

    public SpatialController(ISpatialService spatialService)
    {
        _spatialService = spatialService;
    }

    // Sites
    [HttpGet("sites")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SiteDto>>>> GetSites(CancellationToken cancellationToken)
    {
        var result = await _spatialService.GetSitesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SiteDto>>.SuccessResponse(result));
    }

    [HttpPost("sites")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<SiteDto>>> CreateSite([FromBody] CreateSiteRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.CreateSiteAsync(req, cancellationToken);
        return Ok(ApiResponse<SiteDto>.SuccessResponse(result, ResponseMessages.Spatial.SiteCreated, 201));
    }

    [HttpPut("sites/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<SiteDto>>> UpdateSite(Guid id, [FromBody] CreateSiteRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.UpdateSiteAsync(id, req, cancellationToken);
        return Ok(ApiResponse<SiteDto>.SuccessResponse(result, ResponseMessages.Spatial.SiteUpdated));
    }

    [HttpDelete("sites/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSite(Guid id, CancellationToken cancellationToken)
    {
        await _spatialService.DeleteSiteAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Spatial.SiteDeleted));
    }

    // Buildings
    [HttpGet("buildings")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BuildingDto>>>> GetBuildings(CancellationToken cancellationToken)
    {
        var result = await _spatialService.GetBuildingsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BuildingDto>>.SuccessResponse(result));
    }

    [HttpPost("buildings")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<BuildingDto>>> CreateBuilding([FromBody] CreateBuildingRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.CreateBuildingAsync(req, cancellationToken);
        return Ok(ApiResponse<BuildingDto>.SuccessResponse(result, ResponseMessages.Spatial.BuildingCreated, 201));
    }

    [HttpPut("buildings/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<BuildingDto>>> UpdateBuilding(Guid id, [FromBody] CreateBuildingRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.UpdateBuildingAsync(id, req, cancellationToken);
        return Ok(ApiResponse<BuildingDto>.SuccessResponse(result, ResponseMessages.Spatial.BuildingUpdated));
    }

    [HttpDelete("buildings/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBuilding(Guid id, CancellationToken cancellationToken)
    {
        await _spatialService.DeleteBuildingAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Spatial.BuildingDeleted));
    }

    // Floors
    [HttpGet("floors")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FloorDto>>>> GetFloors(CancellationToken cancellationToken)
    {
        var result = await _spatialService.GetFloorsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FloorDto>>.SuccessResponse(result));
    }

    [HttpPost("floors")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<FloorDto>>> CreateFloor([FromBody] CreateFloorRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.CreateFloorAsync(req, cancellationToken);
        return Ok(ApiResponse<FloorDto>.SuccessResponse(result, ResponseMessages.Spatial.FloorCreated, 201));
    }

    [HttpPut("floors/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<FloorDto>>> UpdateFloor(Guid id, [FromBody] CreateFloorRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.UpdateFloorAsync(id, req, cancellationToken);
        return Ok(ApiResponse<FloorDto>.SuccessResponse(result, ResponseMessages.Spatial.FloorUpdated));
    }

    [HttpDelete("floors/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteFloor(Guid id, CancellationToken cancellationToken)
    {
        await _spatialService.DeleteFloorAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Spatial.FloorDeleted));
    }

    // Rooms
    [HttpGet("rooms")]
    public async Task<ActionResult<ApiResponse<List<RoomDetailDto>>>> GetRooms(
        [FromQuery] RoomFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _spatialService.GetRoomsPagedAsync(filter, cancellationToken);
        return Ok(ApiResponse<List<RoomDetailDto>>.PaginatedResponse(pagedResult));
    }

    [HttpGet("rooms/{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> GetRoomById(Guid id, CancellationToken cancellationToken)
    {
        var room = await _spatialService.GetRoomByIdAsync(id, cancellationToken);
        if (room == null)
        {
            return NotFound(ApiResponse.NotFound(ResponseMessages.Spatial.RoomNotFound));
        }

        return Ok(ApiResponse<RoomDetailDto>.SuccessResponse(room));
    }

    [HttpGet("rooms/hierarchy")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RoomHierarchyResponseDto>>>> GetRoomHierarchy(CancellationToken cancellationToken)
    {
        var result = await _spatialService.GetRoomHierarchyAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RoomHierarchyResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("rooms")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> CreateRoom([FromBody] CreateRoomRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.CreateRoomAsync(req, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.SuccessResponse(result, ResponseMessages.Spatial.RoomCreated, 201));
    }

    [HttpPut("rooms/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<RoomDetailDto>>> UpdateRoom(Guid id, [FromBody] CreateRoomRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.UpdateRoomAsync(id, req, cancellationToken);
        return Ok(ApiResponse<RoomDetailDto>.SuccessResponse(result, ResponseMessages.Spatial.RoomUpdated));
    }

    [HttpDelete("rooms/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRoom(Guid id, CancellationToken cancellationToken)
    {
        await _spatialService.DeleteRoomAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Spatial.RoomDeleted));
    }

    // Cameras
    [HttpGet("cameras")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CameraDetailDto>>>> GetCameras(CancellationToken cancellationToken)
    {
        var result = await _spatialService.GetCamerasAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CameraDetailDto>>.SuccessResponse(result));
    }

    [HttpPost("cameras")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<CameraDetailDto>>> CreateCamera([FromBody] CreateCameraRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.CreateCameraAsync(req, cancellationToken);
        return Ok(ApiResponse<CameraDetailDto>.SuccessResponse(result, ResponseMessages.Spatial.CameraCreated, 201));
    }

    [HttpPut("cameras/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<CameraDetailDto>>> UpdateCamera(Guid id, [FromBody] CreateCameraRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.UpdateCameraAsync(id, req, cancellationToken);
        return Ok(ApiResponse<CameraDetailDto>.SuccessResponse(result, ResponseMessages.Spatial.CameraUpdated));
    }

    [HttpDelete("cameras/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCamera(Guid id, CancellationToken cancellationToken)
    {
        await _spatialService.DeleteCameraAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Spatial.CameraDeleted));
    }

    // Zones
    [HttpGet("zones")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ZoneDetailDto>>>> GetZones(CancellationToken cancellationToken)
    {
        var result = await _spatialService.GetZonesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ZoneDetailDto>>.SuccessResponse(result));
    }

    [HttpPost("zones")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<ZoneDetailDto>>> CreateZone([FromBody] CreateZoneRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.CreateZoneAsync(req, cancellationToken);
        return Ok(ApiResponse<ZoneDetailDto>.SuccessResponse(result, ResponseMessages.Spatial.ZoneCreated, 201));
    }

    [HttpPut("zones/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<ZoneDetailDto>>> UpdateZone(Guid id, [FromBody] CreateZoneRequest req, CancellationToken cancellationToken)
    {
        var result = await _spatialService.UpdateZoneAsync(id, req, cancellationToken);
        return Ok(ApiResponse<ZoneDetailDto>.SuccessResponse(result, ResponseMessages.Spatial.ZoneUpdated));
    }

    [HttpDelete("zones/{id:guid}")]
    [RequirePermission(AppPermissions.Spatial.Write)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteZone(Guid id, CancellationToken cancellationToken)
    {
        await _spatialService.DeleteZoneAsync(id, cancellationToken);
        return Ok(ApiResponse.SuccessResponse(ResponseMessages.Spatial.ZoneDeleted));
    }
}
