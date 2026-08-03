namespace CCTV.Application.Orchestration.DTOs;

public record SiteOptionDto(string Id, string Name, string Code);
public record CameraOptionDto(string Id, string Name, string RtspUrl, string Status, string Resolution);
public record PipelineOptionDto(string Id, string PipelineId, string CameraName, string Status);
public record AnalyticsCountersDto(int PeopleCount, int QueueLength, int LoiteringAvgSeconds, double PpeCompliancePercent);

public record MonitoringOverviewDto(
    List<SiteOptionDto> Sites,
    List<CameraOptionDto> Cameras,
    List<PipelineOptionDto> Pipelines,
    AnalyticsCountersDto Counters
);

public record MonitoringEventDto(
    string Id,
    string Timestamp,
    string PluginCode,
    string PluginCategory,
    string CameraName,
    string ZoneName,
    string? TrackId,
    double Confidence,
    string Severity,
    string Description,
    bool IsIncident
);

public record MonitoringIncidentDto(
    string Id,
    string Timestamp,
    string PluginCode,
    string CameraName,
    string Severity,
    string Status,
    string Description
);
