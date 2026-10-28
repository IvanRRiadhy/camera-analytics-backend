namespace CCTV.Contracts.Analytics;

public record AnalyticsModuleDto(
    Guid Id,
    string ModuleCode,
    string DisplayName,
    string? Description,
    string? Version,
    string? Vendor,
    string? Category,
    string? PythonEntryPoint,
    string? Icon,
    string? DocumentationUrl,
    string? MinimumCudaCapability,
    bool SupportsRealtime,
    bool SupportsBatch,
    string? DefaultConfigurationJson,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAnalyticsModuleDto(
    string ModuleCode,
    string DisplayName,
    string? Description = null,
    string? Version = null,
    string? Vendor = null,
    string? Category = null,
    string? PythonEntryPoint = null,
    string? Icon = null,
    string? DocumentationUrl = null,
    string? MinimumCudaCapability = null,
    bool SupportsRealtime = true,
    bool SupportsBatch = true,
    string? DefaultConfigurationJson = "{}",
    string? Status = "Active"
);

public record UpdateAnalyticsModuleDto(
    string? DisplayName = null,
    string? Description = null,
    string? Version = null,
    string? Vendor = null,
    string? Category = null,
    string? PythonEntryPoint = null,
    string? Icon = null,
    string? DocumentationUrl = null,
    string? MinimumCudaCapability = null,
    bool? SupportsRealtime = null,
    bool? SupportsBatch = null,
    string? DefaultConfigurationJson = null,
    string? Status = null
);

public record ToggleStatusDto(string Status);

public record ModuleEntitlementDto(
    Guid Id,
    Guid ModuleId,
    Guid ApplicationId,
    int MaxAllowedCameraStreams,
    DateTime ValidFrom,
    DateTime? ValidTo,
    bool IsSuspended
);

public record GrantEntitlementRequestDto(
    Guid ApplicationId,
    int MaxAllowedCameraStreams = 10,
    DateTime? ValidTo = null
);

public record PipelineModuleAssignmentDto(
    Guid ModuleId,
    string ModuleCode,
    string DisplayName,
    string? Category,
    string? PythonEntryPoint,
    bool IsEnabled,
    string? ParametersJson,
    DateTime AssignedAtUtc
);

public record PipelineAnalyticsOverviewDto(
    Guid Id,
    string PipelineId,
    string PipelineName,
    int Priority,
    string Status,
    Guid CameraId,
    string CameraName,
    string RtspUrl,
    Guid InstanceId,
    string EngineInstanceId,
    string EngineHostname,
    IReadOnlyList<PipelineModuleAssignmentDto> AssignedModules
);

public record UpdatePipelineModulesDto(List<Guid> ModuleIds);
