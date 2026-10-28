using CCTV.Application.Common.Interfaces;
using CCTV.Domain.Entities.Orchestration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/engine/runtime")]
public class EngineBootstrapController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public EngineBootstrapController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Runtime bootstrap endpoint for Python AI Analytics Engine.
    /// Returns assigned cameras, stream URIs, full Spatial Hierarchy (Site -> Building -> Floor -> Room -> Camera -> Zone),
    /// pipelines, active analytics modules, and engine metadata.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("bootstrap")]
    public async Task<IActionResult> GetRuntimeBootstrap([FromQuery] string? engineInstanceId, CancellationToken cancellationToken)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var query = pipelineRepo.GetQueryable()
            .Include(p => p.Camera)
                .ThenInclude(c => c!.Room)
                    .ThenInclude(r => r!.Floor)
                        .ThenInclude(f => f!.Building)
                            .ThenInclude(b => b!.Site)
            .Include(p => p.Camera)
                .ThenInclude(c => c!.Zones)
            .Include(p => p.Instance)
                .ThenInclude(i => i.Node)
            .Include(p => p.ModuleAssignments)
                .ThenInclude(ma => ma.Module)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(engineInstanceId))
        {
            query = query.Where(p => p.Instance.InstanceId == engineInstanceId);
        }

        var pipelines = await query.ToListAsync(cancellationToken);

        var response = new
        {
            EngineInstanceId = engineInstanceId ?? "engine_edge_01",
            TimestampUtc = DateTime.UtcNow,
            TotalAssignedPipelines = pipelines.Count,
            Pipelines = pipelines.Select(p => new
            {
                PipelineId = p.PipelineId,
                Priority = p.Priority,
                Status = p.Status.ToString(),
                SpatialHierarchy = new
                {
                    SiteId = p.Camera?.Room?.Floor?.Building?.SiteId.ToString(),
                    SiteCode = p.Camera?.Room?.Floor?.Building?.Site?.Code,
                    SiteName = p.Camera?.Room?.Floor?.Building?.Site?.Name,
                    BuildingId = p.Camera?.Room?.Floor?.BuildingId.ToString(),
                    BuildingCode = p.Camera?.Room?.Floor?.Building?.BuildingCode,
                    BuildingName = p.Camera?.Room?.Floor?.Building?.Name,
                    FloorId = p.Camera?.Room?.FloorId.ToString(),
                    FloorLevel = p.Camera?.Room?.Floor?.FloorLevel,
                    FloorName = p.Camera?.Room?.Floor?.Name,
                    RoomId = p.Camera?.RoomId?.ToString(),
                    RoomCode = p.Camera?.Room?.Code,
                    RoomName = p.Camera?.Room?.Name
                },
                Camera = new
                {
                    CameraId = p.CameraId.ToString(),
                    Name = p.Camera?.Name ?? "CAM-01 Main Lobby",
                    DeviceCode = p.Camera?.DeviceCode ?? "cam_01",
                    RtspUrl = p.Camera?.RtspStreamUrl ?? "rtsp://admin:Admin1234@192.168.1.64/Streaming/channels/2",
                    Width = 1920,
                    Height = 1080,
                    Fps = 30.0,
                    RoomId = p.Camera?.RoomId?.ToString(),
                    Zones = p.Camera?.Zones.Select(z => new
                    {
                        ZoneId = z.Id.ToString(),
                        Name = z.Name,
                        Type = z.ZoneType.ToString(),
                        PolygonCoordinatesJson = z.PolygonCoordinatesJson
                    }).ToList()
                },
                EngineInstance = new
                {
                    InstanceId = p.Instance?.InstanceId ?? "engine_edge_01",
                    Hostname = p.Instance?.Hostname ?? "edge-server-01",
                    IpAddress = p.Instance?.IpAddress ?? "192.168.1.120",
                    GpuModel = p.Instance?.GpuModel ?? "NVIDIA RTX 4090"
                },
                AssignedModules = p.ModuleAssignments.Select(ma => new
                {
                    ModuleCode = ma.Module.ModuleCode,
                    DisplayName = ma.Module.DisplayName,
                    Category = ma.Module.Category,
                    PythonEntryPoint = ma.Module.PythonEntryPoint,
                    MinimumCudaCapability = ma.Module.MinimumCudaCapability,
                    IsEnabled = ma.IsEnabled,
                    ParametersJson = ma.ParametersJson
                }).ToList()
            }).ToList()
        };

        return Ok(response);
    }
}
