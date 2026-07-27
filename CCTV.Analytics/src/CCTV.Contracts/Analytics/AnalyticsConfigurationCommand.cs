using CCTV.Contracts.Common;

namespace CCTV.Contracts.Analytics;

public record AnalyticsConfigDto
{
    public Guid ConfigurationId { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public Guid? ZoneId { get; init; }
    public decimal Sensitivity { get; init; } = 0.75m;
    public string? ParametersJson { get; init; }
    public string? ScheduleCron { get; init; }
    public bool IsEnabled { get; init; } = true;
}

public record AnalyticsConfigurationCommand : BaseMessage
{
    public Guid ConfigurationId { get; init; }
    public Guid CameraId { get; init; }
    public string ModuleCode { get; init; } = string.Empty;
    public Guid? ZoneId { get; init; }
    public decimal Sensitivity { get; init; } = 0.75m;
    public string? ParametersJson { get; init; }
    public string? ScheduleCron { get; init; }
    public bool IsEnabled { get; init; } = true;
}
