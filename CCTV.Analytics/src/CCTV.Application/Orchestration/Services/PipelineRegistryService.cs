using CCTV.Application.Common.Interfaces;
using CCTV.Application.Orchestration.DTOs;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Enumerations;

namespace CCTV.Application.Orchestration.Services;

public class PipelineRegistryService : IPipelineRegistryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEngineTelemetryPublisher? _telemetryPublisher;

    public PipelineRegistryService(
        IUnitOfWork unitOfWork,
        IEngineTelemetryPublisher? telemetryPublisher = null)
    {
        _unitOfWork = unitOfWork;
        _telemetryPublisher = telemetryPublisher;
    }

    public async Task<PipelineAssignmentDto> AssignPipelineAsync(AssignPipelineRequestDto request, CancellationToken cancellationToken = default)
    {
        var cameraRepo = _unitOfWork.Repository<Camera>();
        var engineRepo = _unitOfWork.Repository<EngineInstance>();
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();

        var camera = await cameraRepo.FirstOrDefaultAsync(c => c.Id == request.CameraId && c.ApplicationId == request.ApplicationId, cancellationToken);
        if (camera == null)
        {
            throw new KeyNotFoundException($"Camera with ID '{request.CameraId}' was not found.");
        }

        var engine = await engineRepo.FirstOrDefaultAsync(e => e.Id == request.EngineInstanceId && e.ApplicationId == request.ApplicationId, cancellationToken);
        if (engine == null)
        {
            throw new KeyNotFoundException($"EngineInstance with ID '{request.EngineInstanceId}' was not found.");
        }

        // Domain Guard: Enforce 1 Active Assignment per Camera
        var activeExisting = await pipelineRepo.FirstOrDefaultAsync(
            p => p.CameraId == request.CameraId && p.ApplicationId == request.ApplicationId &&
                 (p.Status == PipelineStatus.Assigned || p.Status == PipelineStatus.Starting ||
                  p.Status == PipelineStatus.Running || p.Status == PipelineStatus.Stopping || p.Status == PipelineStatus.Recovering),
            cancellationToken);

        if (activeExisting != null)
        {
            throw new InvalidOperationException($"Camera '{request.CameraId}' already has an active pipeline assignment '{activeExisting.PipelineId}' (Status: {activeExisting.Status}).");
        }

        var pipelineIdStr = !string.IsNullOrWhiteSpace(request.PipelineId) ? request.PipelineId : $"pipeline_{camera.Name.ToLowerInvariant().Replace(' ', '_')}";

        var assignment = new PipelineAssignment
        {
            ApplicationId = request.ApplicationId,
            CameraId = request.CameraId,
            InstanceId = request.EngineInstanceId,
            PipelineId = pipelineIdStr,
            PipelineName = !string.IsNullOrWhiteSpace(request.PipelineName) ? request.PipelineName : camera.Name,
            Priority = request.Priority > 0 ? request.Priority : 1,
            RequiredCapabilities = request.RequiredCapabilities ?? new List<string>(),
            AnalyticsModules = request.AnalyticsModules ?? new List<string>(),
            Status = PipelineStatus.Assigned,
            AssignedAtUtc = DateTime.UtcNow
        };

        assignment.TransitionState(PipelineStatus.Assigned, $"Assigned to Engine Instance '{engine.InstanceId}'");

        await pipelineRepo.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(assignment, engine.InstanceId, engine.Hostname);

        if (_telemetryPublisher != null)
        {
            await _telemetryPublisher.PublishPipelineEventAsync("PipelineAssigned", dto, cancellationToken);
        }

        return dto;
    }

    public async Task<PipelineAssignmentDto> MovePipelineAsync(MovePipelineRequestDto request, CancellationToken cancellationToken = default)
    {
        var engineRepo = _unitOfWork.Repository<EngineInstance>();
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();

        var assignment = await pipelineRepo.FirstOrDefaultAsync(p => p.Id == request.AssignmentId && p.ApplicationId == request.ApplicationId, cancellationToken);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"PipelineAssignment with ID '{request.AssignmentId}' was not found.");
        }

        var newEngine = await engineRepo.FirstOrDefaultAsync(e => e.Id == request.NewEngineInstanceId && e.ApplicationId == request.ApplicationId, cancellationToken);
        if (newEngine == null)
        {
            throw new KeyNotFoundException($"Target EngineInstance with ID '{request.NewEngineInstanceId}' was not found.");
        }

        assignment.Move(request.NewEngineInstanceId, request.Reason);
        pipelineRepo.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(assignment, newEngine.InstanceId, newEngine.Hostname);

        if (_telemetryPublisher != null)
        {
            await _telemetryPublisher.PublishPipelineEventAsync("PipelineMoved", dto, cancellationToken);
        }

        return dto;
    }

    public async Task<PipelineAssignmentDto> StopPipelineAsync(Guid applicationId, Guid assignmentId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var engineRepo = _unitOfWork.Repository<EngineInstance>();

        var assignment = await pipelineRepo.FirstOrDefaultAsync(p => p.Id == assignmentId && p.ApplicationId == applicationId, cancellationToken);
        if (assignment == null)
        {
            throw new KeyNotFoundException($"PipelineAssignment with ID '{assignmentId}' was not found.");
        }

        assignment.MarkStopped();
        pipelineRepo.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var engine = await engineRepo.GetByIdAsync(assignment.InstanceId, cancellationToken);
        var dto = MapToDto(assignment, engine?.InstanceId ?? string.Empty, engine?.Hostname ?? string.Empty);

        if (_telemetryPublisher != null)
        {
            await _telemetryPublisher.PublishPipelineEventAsync("PipelineStopped", dto, cancellationToken);
        }

        return dto;
    }

    public async Task<PipelineAssignmentDto?> GetByIdAsync(Guid applicationId, Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var engineRepo = _unitOfWork.Repository<EngineInstance>();

        var assignment = await pipelineRepo.FirstOrDefaultAsync(p => p.Id == assignmentId && p.ApplicationId == applicationId, cancellationToken);
        if (assignment == null) return null;

        var engine = await engineRepo.GetByIdAsync(assignment.InstanceId, cancellationToken);
        return MapToDto(assignment, engine?.InstanceId ?? string.Empty, engine?.Hostname ?? string.Empty);
    }

    public async Task<PipelineAssignmentDto?> GetByCameraIdAsync(Guid applicationId, Guid cameraId, CancellationToken cancellationToken = default)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var engineRepo = _unitOfWork.Repository<EngineInstance>();

        var assignment = await pipelineRepo.FirstOrDefaultAsync(p => p.CameraId == cameraId && p.ApplicationId == applicationId, cancellationToken);
        if (assignment == null) return null;

        var engine = await engineRepo.GetByIdAsync(assignment.InstanceId, cancellationToken);
        return MapToDto(assignment, engine?.InstanceId ?? string.Empty, engine?.Hostname ?? string.Empty);
    }

    public async Task<IReadOnlyList<PipelineAssignmentDto>> GetByEngineIdAsync(Guid applicationId, Guid engineInstanceId, CancellationToken cancellationToken = default)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var engineRepo = _unitOfWork.Repository<EngineInstance>();

        var engine = await engineRepo.GetByIdAsync(engineInstanceId, cancellationToken);
        var assignments = await pipelineRepo.FindAsync(p => p.InstanceId == engineInstanceId && p.ApplicationId == applicationId, cancellationToken);

        return assignments.Select(a => MapToDto(a, engine?.InstanceId ?? string.Empty, engine?.Hostname ?? string.Empty)).ToList();
    }

    public async Task<IReadOnlyList<PipelineAssignmentDto>> ListPipelinesAsync(Guid applicationId, string? statusFilter = null, CancellationToken cancellationToken = default)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var engineRepo = _unitOfWork.Repository<EngineInstance>();

        IReadOnlyList<PipelineAssignment> assignments;
        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<PipelineStatus>(statusFilter, true, out var statusEnum))
        {
            assignments = await pipelineRepo.FindAsync(p => p.ApplicationId == applicationId && p.Status == statusEnum, cancellationToken);
        }
        else
        {
            assignments = await pipelineRepo.FindAsync(p => p.ApplicationId == applicationId, cancellationToken);
        }

        var engines = await engineRepo.FindAsync(e => e.ApplicationId == applicationId, cancellationToken);
        var engineDict = engines.ToDictionary(e => e.Id);

        return assignments.Select(a =>
        {
            var inst = engineDict.TryGetValue(a.InstanceId, out var e) ? e : null;
            return MapToDto(a, inst?.InstanceId ?? string.Empty, inst?.Hostname ?? string.Empty);
        }).ToList();
    }

    public async Task<IReadOnlyList<PipelineHistoryDto>> GetPipelineHistoryAsync(Guid applicationId, Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var historyRepo = _unitOfWork.Repository<PipelineHistory>();
        var histories = await historyRepo.FindAsync(h => h.PipelineAssignmentId == assignmentId && h.ApplicationId == applicationId, cancellationToken);

        return histories.Select(h => new PipelineHistoryDto
        {
            Id = h.Id,
            PipelineAssignmentId = h.PipelineAssignmentId,
            PreviousStatus = h.PreviousStatus,
            NewStatus = h.NewStatus,
            Reason = h.Reason,
            TimestampUtc = h.TimestampUtc
        }).OrderBy(h => h.TimestampUtc).ToList();
    }

    private static PipelineAssignmentDto MapToDto(PipelineAssignment assignment, string instanceIdStr, string hostname)
    {
        return new PipelineAssignmentDto
        {
            Id = assignment.Id,
            PipelineId = assignment.PipelineId,
            CameraId = assignment.CameraId,
            EngineInstanceId = assignment.InstanceId,
            InstanceIdString = instanceIdStr,
            Hostname = hostname,
            PipelineName = assignment.PipelineName,
            Priority = assignment.Priority,
            RequiredCapabilities = assignment.RequiredCapabilities,
            AnalyticsModules = assignment.AnalyticsModules,
            Status = assignment.Status,
            AssignedAtUtc = assignment.AssignedAtUtc,
            StartedAtUtc = assignment.StartedAtUtc,
            StoppedAtUtc = assignment.StoppedAtUtc
        };
    }
}
