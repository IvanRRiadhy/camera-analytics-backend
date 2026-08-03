using CCTV.Application.Orchestration.DTOs;

namespace CCTV.Application.Orchestration.Interfaces;

public interface IPipelineRegistryService
{
    Task<PipelineAssignmentDto> AssignPipelineAsync(AssignPipelineRequestDto request, CancellationToken cancellationToken = default);
    Task<PipelineAssignmentDto> MovePipelineAsync(MovePipelineRequestDto request, CancellationToken cancellationToken = default);
    Task<PipelineAssignmentDto> StopPipelineAsync(Guid applicationId, Guid assignmentId, string? reason = null, CancellationToken cancellationToken = default);
    
    Task<PipelineAssignmentDto?> GetByIdAsync(Guid applicationId, Guid assignmentId, CancellationToken cancellationToken = default);
    Task<PipelineAssignmentDto?> GetByCameraIdAsync(Guid applicationId, Guid cameraId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PipelineAssignmentDto>> GetByEngineIdAsync(Guid applicationId, Guid engineInstanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PipelineAssignmentDto>> ListPipelinesAsync(Guid applicationId, string? statusFilter = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PipelineHistoryDto>> GetPipelineHistoryAsync(Guid applicationId, Guid assignmentId, CancellationToken cancellationToken = default);
}
