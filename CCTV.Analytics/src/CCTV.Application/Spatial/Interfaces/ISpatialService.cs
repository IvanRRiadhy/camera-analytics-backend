using CCTV.Contracts.Common;
using CCTV.Contracts.Spatial;

namespace CCTV.Application.Spatial.Interfaces;

public interface ISpatialService
{
    // Sites
    Task<IReadOnlyList<SiteDto>> GetSitesAsync(CancellationToken cancellationToken = default);
    Task<SiteDto> CreateSiteAsync(CreateSiteRequest req, CancellationToken cancellationToken = default);
    Task<SiteDto> UpdateSiteAsync(Guid id, CreateSiteRequest req, CancellationToken cancellationToken = default);
    Task DeleteSiteAsync(Guid id, CancellationToken cancellationToken = default);

    // Buildings
    Task<IReadOnlyList<BuildingDto>> GetBuildingsAsync(CancellationToken cancellationToken = default);
    Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest req, CancellationToken cancellationToken = default);
    Task<BuildingDto> UpdateBuildingAsync(Guid id, CreateBuildingRequest req, CancellationToken cancellationToken = default);
    Task DeleteBuildingAsync(Guid id, CancellationToken cancellationToken = default);

    // Floors
    Task<IReadOnlyList<FloorDto>> GetFloorsAsync(CancellationToken cancellationToken = default);
    Task<FloorDto> CreateFloorAsync(CreateFloorRequest req, CancellationToken cancellationToken = default);
    Task<FloorDto> UpdateFloorAsync(Guid id, CreateFloorRequest req, CancellationToken cancellationToken = default);
    Task DeleteFloorAsync(Guid id, CancellationToken cancellationToken = default);

    // Rooms
    Task<PagedResult<RoomDetailDto>> GetRoomsPagedAsync(RoomFilterRequest filter, CancellationToken cancellationToken = default);
    Task<RoomDetailDto?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoomHierarchyResponseDto>> GetRoomHierarchyAsync(CancellationToken cancellationToken = default);
    Task<RoomDetailDto> CreateRoomAsync(CreateRoomRequest req, CancellationToken cancellationToken = default);
    Task<RoomDetailDto> UpdateRoomAsync(Guid id, CreateRoomRequest req, CancellationToken cancellationToken = default);
    Task DeleteRoomAsync(Guid id, CancellationToken cancellationToken = default);

    // Cameras
    Task<IReadOnlyList<CameraDetailDto>> GetCamerasAsync(CancellationToken cancellationToken = default);
    Task<CameraDetailDto> CreateCameraAsync(CreateCameraRequest req, CancellationToken cancellationToken = default);
    Task<CameraDetailDto> UpdateCameraAsync(Guid id, CreateCameraRequest req, CancellationToken cancellationToken = default);
    Task DeleteCameraAsync(Guid id, CancellationToken cancellationToken = default);

    // Zones
    Task<IReadOnlyList<ZoneDetailDto>> GetZonesAsync(CancellationToken cancellationToken = default);
    Task<ZoneDetailDto> CreateZoneAsync(CreateZoneRequest req, CancellationToken cancellationToken = default);
    Task<ZoneDetailDto> UpdateZoneAsync(Guid id, CreateZoneRequest req, CancellationToken cancellationToken = default);
    Task DeleteZoneAsync(Guid id, CancellationToken cancellationToken = default);
}
