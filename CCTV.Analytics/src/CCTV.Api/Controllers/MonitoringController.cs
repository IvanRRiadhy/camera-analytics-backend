using System.Text;
using CCTV.Api.Security;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/monitoring")]
[RequirePermission(AppPermissions.Monitoring.Read)]
public class MonitoringController : ControllerBase
{
    private static readonly Dictionary<string, (byte[] imageBytes, string overlayMetadataJson, long frameNumber)> _snapshotStore = new();
    private readonly IMonitoringService _monitoringService;
    private readonly IEngineTelemetryPublisher _telemetryPublisher;

    public MonitoringController(
        IMonitoringService monitoringService,
        IEngineTelemetryPublisher telemetryPublisher)
    {
        _monitoringService = monitoringService;
        _telemetryPublisher = telemetryPublisher;
    }

    /// <summary>
    /// Gets single source of truth monitoring overview (Sites, Cameras, Pipelines, Analytics Counters).
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var overview = await _monitoringService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    /// <summary>
    /// Gets realtime analytics events feed.
    /// </summary>
    [HttpGet("events")]
    public async Task<IActionResult> GetAnalyticsEvents([FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        var events = await _monitoringService.GetAnalyticsEventsAsync(take, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Gets escalated active incidents feed.
    /// </summary>
    [HttpGet("incidents")]
    public async Task<IActionResult> GetIncidents([FromQuery] int take = 20, CancellationToken cancellationToken = default)
    {
        var incidents = await _monitoringService.GetIncidentsAsync(take, cancellationToken);
        return Ok(incidents);
    }

    /// <summary>
    /// Gets the latest JPEG snapshot for a specific vision pipeline.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("snapshot/{pipelineId}")]
    public IActionResult GetLatestSnapshot(string pipelineId)
    {
        if (_snapshotStore.TryGetValue(pipelineId, out var snapshot) && snapshot.imageBytes != null && snapshot.imageBytes.Length > 0)
        {
            return File(snapshot.imageBytes, "image/jpeg");
        }

        // Generate dynamic SVG/JPEG fallback frame if no live engine frame uploaded yet
        byte[] fallbackBytes = GenerateFallbackJpeg(pipelineId);
        return File(fallbackBytes, "image/jpeg");
    }

    /// <summary>
    /// Uploads latest JPEG frame + overlay metadata from AI Engine Runtime and notifies SignalR subscribers.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("snapshot/{pipelineId}")]
    public async Task<IActionResult> PostSnapshot(string pipelineId, IFormFile imageFile, [FromForm] string? overlayMetadataJson, CancellationToken cancellationToken)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest("Image file is empty.");
        }

        using var ms = new MemoryStream();
        await imageFile.CopyToAsync(ms, cancellationToken);
        byte[] bytes = ms.ToArray();

        long frameNum = DateTime.UtcNow.Ticks;
        string overlayJson = overlayMetadataJson ?? "[]";

        _snapshotStore[pipelineId] = (bytes, overlayJson, frameNum);

        // Notify presentation clients via SignalR
        await _telemetryPublisher.PublishSnapshotUpdatedAsync(pipelineId, frameNum, overlayJson, cancellationToken);

        return Ok(new { pipelineId, frameNum, status = "SnapshotUpdated" });
    }

    private static byte[] GenerateFallbackJpeg(string pipelineId)
    {
        string svg = $@"<svg xmlns='http://www.w3.org/2000/svg' width='1280' height='720' viewBox='0 0 1280 720'>
            <rect width='1280' height='720' fill='#020617'/>
            <text x='50%' y='45%' dominant-baseline='middle' text-anchor='middle' fill='#38bdf8' font-size='28' font-family='monospace' font-weight='bold'>AI ENGINE RASPBERRY/CUDA RUNTIME ACTIVE</text>
            <text x='50%' y='55%' dominant-baseline='middle' text-anchor='middle' fill='#94a3b8' font-size='18' font-family='monospace'>Pipeline: {pipelineId} | 1 FPS Stream Engine</text>
        </svg>";
        return Encoding.UTF8.GetBytes(svg);
    }
}
