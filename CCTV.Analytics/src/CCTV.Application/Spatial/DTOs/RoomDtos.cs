namespace CCTV.Application.Spatial.DTOs;

public record RoomDto(
    Guid Id,
    Guid FloorId,
    string Name,
    string Code,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record RoomHierarchyDto(
    Guid Id,
    Guid FloorId,
    string Name,
    string Code,
    string? Description,
    int SortOrder,
    bool IsActive,
    int CameraCount,
    int ZoneCount
);

public record CreateRoomRequest(
    Guid FloorId,
    string Name,
    string? Code,
    string? Description,
    int SortOrder = 0,
    bool IsActive = true
);

public record UpdateRoomRequest(
    string Name,
    string? Code,
    string? Description,
    int SortOrder,
    bool IsActive
);

public record RoomQueryParams(
    Guid? FloorId,
    bool? IsActive,
    string? SearchTerm,
    string? SortBy,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
);
