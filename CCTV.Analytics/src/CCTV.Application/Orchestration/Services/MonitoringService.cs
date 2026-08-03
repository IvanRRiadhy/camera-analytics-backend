using CCTV.Application.Common.Interfaces;
using CCTV.Application.Orchestration.DTOs;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Application.Orchestration.Options;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Entities.Spatial;
using Microsoft.Extensions.Options;

namespace CCTV.Application.Orchestration.Services;

public class MonitoringService : IMonitoringService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly MonitoringOptions _options;

    public MonitoringService(IUnitOfWork unitOfWork, IOptions<MonitoringOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<MonitoringOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var siteRepo = _unitOfWork.Repository<Site>();
        var cameraRepo = _unitOfWork.Repository<Camera>();
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();

        var sites = await siteRepo.FindAsync(x => true, cancellationToken);
        var cameras = await cameraRepo.FindAsync(x => true, cancellationToken);
        var pipelines = await pipelineRepo.FindAsync(x => true, cancellationToken);

        var siteDtos = sites.Select(s => new SiteOptionDto(s.Id.ToString(), s.Name, s.Code)).ToList();
        var cameraDtos = cameras.Select(c => new CameraOptionDto(
            c.Id.ToString(),
            c.Name,
            string.IsNullOrWhiteSpace(c.RtspStreamUrl) ? _options.DefaultRtspUrl : c.RtspStreamUrl,
            c.Status.ToString(),
            c.Resolution ?? "1920x1080"
        )).ToList();

        var pipelineDtos = pipelines.Select(p => new PipelineOptionDto(
            p.Id.ToString(),
            p.PipelineId,
            cameras.FirstOrDefault(c => c.Id == p.CameraId)?.Name ?? p.PipelineName,
            p.Status.ToString()
        )).ToList();

        // Default empty counters or demo mode fallback
        var counters = _options.DemoMode
            ? new AnalyticsCountersDto(142, 5, 45, 98.2)
            : new AnalyticsCountersDto(0, 0, 0, 100.0);

        return new MonitoringOverviewDto(siteDtos, cameraDtos, pipelineDtos, counters);
    }

    public Task<List<MonitoringEventDto>> GetAnalyticsEventsAsync(int take = 50, CancellationToken cancellationToken = default)
    {
        if (!_options.DemoMode)
        {
            return Task.FromResult(new List<MonitoringEventDto>());
        }

        var now = DateTime.UtcNow;
        var list = new List<MonitoringEventDto>
        {
            new MonitoringEventDto(
                "evt_101",
                now.AddMinutes(-5).ToString("HH:mm:ss"),
                "people_counting",
                "BUSINESS_INTELLIGENCE",
                "CAM-01 Main Lobby",
                "Entrance_Zone_A",
                "TRK-102",
                0.984,
                "INFO",
                "Person entered zone (Total In: 142 | Occupancy: 18)",
                false
            ),
            new MonitoringEventDto(
                "evt_102",
                now.AddMinutes(-3).ToString("HH:mm:ss"),
                "queue_detection",
                "BUSINESS_INTELLIGENCE",
                "CAM-02 Service Counter",
                "Queue_Line_01",
                "TRK-108",
                0.942,
                "WARNING",
                "Queue length limit exceeded (5 persons waiting > 2 mins)",
                false
            ),
            new MonitoringEventDto(
                "evt_103",
                now.AddMinutes(-1).ToString("HH:mm:ss"),
                "loitering_detection",
                "SAFETY_SECURITY",
                "CAM-04 Vault Corridor",
                "Restricted_Vault_Zone",
                "TRK-114",
                0.968,
                "CRITICAL",
                "Track #114 loitering duration > 45s in restricted zone",
                true
            ),
            new MonitoringEventDto(
                "evt_104",
                now.ToString("HH:mm:ss"),
                "ppe_compliance",
                "OPERATIONAL_COMPLIANCE",
                "CAM-08 Loading Dock",
                "Dock_Safety_Zone",
                "TRK-120",
                0.915,
                "WARNING",
                "Hard Hat non-compliance detected on worker #120",
                false
            )
        };

        return Task.FromResult(list);
    }

    public Task<List<MonitoringIncidentDto>> GetIncidentsAsync(int take = 20, CancellationToken cancellationToken = default)
    {
        if (!_options.DemoMode)
        {
            return Task.FromResult(new List<MonitoringIncidentDto>());
        }

        var now = DateTime.UtcNow;
        var list = new List<MonitoringIncidentDto>
        {
            new MonitoringIncidentDto(
                "inc_201",
                now.AddMinutes(-1).ToString("HH:mm:ss"),
                "loitering_detection",
                "CAM-04 Vault Corridor",
                "CRITICAL",
                "OPEN",
                "Loitering detected near restricted vault door"
            ),
            new MonitoringIncidentDto(
                "inc_202",
                now.AddMinutes(-10).ToString("HH:mm:ss"),
                "fire_smoke_detection",
                "CAM-12 Warehouse B",
                "CRITICAL",
                "OPEN",
                "Smoke density threshold breached near Rack 04"
            )
        };

        return Task.FromResult(list);
    }
}
