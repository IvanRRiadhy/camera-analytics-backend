using CCTV.Application.Common.Interfaces;
using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/analytics/pipelines")]
public class AnalyticsPipelinesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public AnalyticsPipelinesController(IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    /// <summary>
    /// Get all camera pipeline assignments along with their active assigned analytics modules.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPipelines(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<PipelineAssignment>();
        var query = repo.GetQueryable()
            .Include(p => p.Camera)
            .Include(p => p.Instance)
            .Include(p => p.ModuleAssignments)
                .ThenInclude(ma => ma.Module);

        var items = await query.ToListAsync(cancellationToken);

        var dtos = items.Select(p => new
        {
            p.Id,
            p.PipelineId,
            p.PipelineName,
            p.Priority,
            p.Status,
            p.CameraId,
            CameraName = p.Camera?.Name ?? "Unknown Camera",
            RtspUrl = p.Camera?.RtspStreamUrl ?? "",
            p.InstanceId,
            EngineInstanceId = p.Instance?.InstanceId ?? "",
            EngineHostname = p.Instance?.Hostname ?? "",
            AssignedModules = p.ModuleAssignments.Select(ma => new
            {
                ma.ModuleId,
                ma.Module.ModuleCode,
                ma.Module.DisplayName,
                ma.Module.Category,
                ma.Module.PythonEntryPoint,
                ma.IsEnabled,
                ma.ParametersJson,
                ma.AssignedAtUtc
            }).ToList()
        });

        return Ok(dtos);
    }

    /// <summary>
    /// Get a single camera pipeline assignment with assigned modules.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPipelineById(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<PipelineAssignment>();
        var pipeline = await repo.GetQueryable()
            .Include(p => p.Camera)
            .Include(p => p.Instance)
            .Include(p => p.ModuleAssignments)
                .ThenInclude(ma => ma.Module)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (pipeline == null)
        {
            return NotFound(new { message = $"Pipeline assignment '{id}' not found." });
        }

        return Ok(new
        {
            pipeline.Id,
            pipeline.PipelineId,
            pipeline.PipelineName,
            pipeline.Priority,
            pipeline.Status,
            pipeline.CameraId,
            CameraName = pipeline.Camera?.Name ?? "",
            RtspUrl = pipeline.Camera?.RtspStreamUrl ?? "",
            pipeline.InstanceId,
            EngineInstanceId = pipeline.Instance?.InstanceId ?? "",
            AssignedModules = pipeline.ModuleAssignments.Select(ma => new
            {
                ma.ModuleId,
                ma.Module.ModuleCode,
                ma.Module.DisplayName,
                ma.Module.Category,
                ma.Module.PythonEntryPoint,
                ma.IsEnabled,
                ma.ParametersJson,
                ma.AssignedAtUtc
            }).ToList()
        });
    }

    /// <summary>
    /// Assign/Update active analytics modules for a camera pipeline assignment.
    /// </summary>
    [HttpPut("{id:guid}/modules")]
    public async Task<IActionResult> UpdatePipelineModules(
        Guid id,
        [FromBody] UpdatePipelineModulesDto request,
        CancellationToken cancellationToken)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var pipeline = await pipelineRepo.GetQueryable()
            .Include(p => p.ModuleAssignments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (pipeline == null)
        {
            return NotFound(new { message = $"Pipeline assignment '{id}' not found." });
        }

        var moduleRepo = _unitOfWork.Repository<AnalyticsModule>();
        var assignmentRepo = _unitOfWork.Repository<PipelineModuleAssignment>();

        // Remove existing assignments
        foreach (var existing in pipeline.ModuleAssignments.ToList())
        {
            assignmentRepo.Delete(existing);
        }

        var assignedModuleCodes = new List<string>();

        // Create new assignments
        if (request.ModuleIds != null && request.ModuleIds.Count > 0)
        {
            var validModules = await moduleRepo.FindAsync(m => request.ModuleIds.Contains(m.Id), cancellationToken);

            foreach (var mod in validModules)
            {
                var newAssign = new PipelineModuleAssignment
                {
                    ApplicationId = _tenantProvider.ApplicationId,
                    PipelineAssignmentId = pipeline.Id,
                    ModuleId = mod.Id,
                    IsEnabled = true,
                    AssignedAtUtc = DateTime.UtcNow
                };
                await assignmentRepo.AddAsync(newAssign, cancellationToken);
                assignedModuleCodes.Add(mod.ModuleCode);
            }
        }

        // Synchronize legacy AnalyticsModules string collection for backwards compatibility
        pipeline.AnalyticsModules = assignedModuleCodes;
        pipelineRepo.Update(pipeline);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            pipeline.Id,
            pipeline.PipelineId,
            AssignedModuleCount = assignedModuleCodes.Count,
            AssignedModules = assignedModuleCodes
        });
    }
}

public record UpdatePipelineModulesDto(List<Guid> ModuleIds);
