namespace CCTV.Contracts.Spatial;

public record SiteDto(string Id, string Name, string Code, string Description, string Image);

public record BuildingDto(string Id, string Name, string Description, string Image, string SiteId);

public record FloorDto(string Id, string Name, string Description, string Image, string BuildingId);

public record RoomDetailDto(
    string Id,
    string Name,
    string Code,
    string? Description,
    int SortOrder,
    bool IsActive,
    string Image,
    string FloorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CameraStatusDto(string Id, string Name, string Status);

public record RoomHierarchyResponseDto(
    string Id,
    string Name,
    string Code,
    string? Description,
    string FloorId,
    int SortOrder,
    bool IsActive,
    int CameraCount,
    int ZoneCount,
    IReadOnlyList<CameraStatusDto> Cameras
);

public record CameraDetailDto(string Id, string Name, string Description, string RtspUrl, string? RoomId, string Status);

public record ZoneCameraConfigDto(string CameraId, object? Polygon);

public record ZoneDetailDto(string Id, string Name, string RoomId, IReadOnlyList<ZoneCameraConfigDto> CameraConfigs);

public record CreateSiteRequest(string Name, string Description, string? Code = null, string? Image = null);

public record CreateBuildingRequest(string Name, string Description, string? Image = null, string? SiteId = null);

public record CreateFloorRequest(string Name, string Description, string? Image = null, string BuildingId = "");

public record CreateRoomRequest(string Name, string Description, string? Image = null, string FloorId = "", string? Code = null);

public record CreateCameraRequest(string Name, string Description, string RtspUrl, string RoomId);

public record CreateZoneRequest(string Name, string RoomId, object[]? CameraConfigs = null);
