using CCTV.Application.Analytics.Interfaces;
using CCTV.Application.Common.Interfaces;
using CCTV.Contracts.Analytics;
using CCTV.Contracts.Common;
using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Application.Analytics.Services;

public class AnalyticsCatalogService : IAnalyticsCatalogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsCatalogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<AnalyticsModuleDto>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var modules = await repo.FindAsync(_ => true, cancellationToken);

        return modules
            .OrderBy(m => m.DisplayName)
            .Select(MapToModuleDto)
            .ToList();
    }

    public async Task<AnalyticsModuleDto?> GetModuleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        return module != null ? MapToModuleDto(module) : null;
    }

    public async Task<AnalyticsModuleDto> CreateModuleAsync(CreateAnalyticsModuleDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var existing = await repo.FirstOrDefaultAsync(m => m.ModuleCode == dto.ModuleCode, cancellationToken);
        if (existing != null)
        {
            throw new BusinessException(ResponseMessages.Analytics.ModuleAlreadyExists);
        }

        var module = new AnalyticsModule
        {
            ModuleCode = dto.ModuleCode,
            DisplayName = dto.DisplayName,
            Description = dto.Description ?? string.Empty,
            Version = dto.Version ?? "1.0.0",
            Vendor = dto.Vendor ?? "BionicsAI",
            Category = dto.Category ?? "Detection",
            PythonEntryPoint = dto.PythonEntryPoint ?? $"src.analytics.plugins.{dto.ModuleCode.ToLowerInvariant()}_pipeline",
            Icon = dto.Icon ?? "Cpu",
            DocumentationUrl = dto.DocumentationUrl ?? string.Empty,
            MinimumCudaCapability = dto.MinimumCudaCapability ?? "7.5",
            SupportsRealtime = dto.SupportsRealtime,
            SupportsBatch = dto.SupportsBatch,
            DefaultConfigurationJson = dto.DefaultConfigurationJson ?? "{}",
            Status = dto.Status ?? "Active"
        };

        await repo.AddAsync(module, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToModuleDto(module);
    }

    public async Task<AnalyticsModuleDto> UpdateModuleAsync(Guid id, UpdateAnalyticsModuleDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        if (module == null)
        {
            throw new NotFoundException(ResponseMessages.Analytics.ModuleNotFound);
        }

        if (dto.DisplayName != null) module.DisplayName = dto.DisplayName;
        if (dto.Description != null) module.Description = dto.Description;
        if (dto.Version != null) module.Version = dto.Version;
        if (dto.Vendor != null) module.Vendor = dto.Vendor;
        if (dto.Category != null) module.Category = dto.Category;
        if (dto.PythonEntryPoint != null) module.PythonEntryPoint = dto.PythonEntryPoint;
        if (dto.Icon != null) module.Icon = dto.Icon;
        if (dto.DocumentationUrl != null) module.DocumentationUrl = dto.DocumentationUrl;
        if (dto.MinimumCudaCapability != null) module.MinimumCudaCapability = dto.MinimumCudaCapability;
        if (dto.SupportsRealtime.HasValue) module.SupportsRealtime = dto.SupportsRealtime.Value;
        if (dto.SupportsBatch.HasValue) module.SupportsBatch = dto.SupportsBatch.Value;
        if (dto.DefaultConfigurationJson != null) module.DefaultConfigurationJson = dto.DefaultConfigurationJson;
        if (dto.Status != null) module.Status = dto.Status;

        repo.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToModuleDto(module);
    }

    public async Task DeleteModuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        if (module == null)
        {
            throw new NotFoundException(ResponseMessages.Analytics.ModuleNotFound);
        }

        repo.Delete(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var modules = await repo.FindAsync(_ => true, cancellationToken);
        return modules
            .Select(m => m.Category)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<IReadOnlyList<ModuleEntitlementDto>> GetEntitlementsAsync(Guid moduleId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<ModuleEntitlement>();
        var entitlements = await repo.FindAsync(e => e.ModuleId == moduleId, cancellationToken);

        return entitlements.Select(e => new ModuleEntitlementDto(
            e.Id,
            e.ModuleId,
            e.ApplicationId,
            e.MaxAllowedCameraStreams,
            e.ValidFrom,
            e.ValidTo,
            e.IsSuspended
        )).ToList();
    }

    public async Task<ModuleEntitlementDto> GrantEntitlementAsync(Guid moduleId, GrantEntitlementRequestDto request, CancellationToken cancellationToken = default)
    {
        var moduleRepo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await moduleRepo.GetByIdAsync(moduleId, cancellationToken);
        if (module == null)
        {
            throw new NotFoundException(ResponseMessages.Analytics.ModuleNotFound);
        }

        var entitlementRepo = _unitOfWork.Repository<ModuleEntitlement>();
        var existing = await entitlementRepo.FirstOrDefaultAsync(
            e => e.ModuleId == moduleId && e.ApplicationId == request.ApplicationId,
            cancellationToken);

        if (existing != null)
        {
            existing.MaxAllowedCameraStreams = request.MaxAllowedCameraStreams;
            existing.ValidTo = request.ValidTo;
            existing.IsSuspended = false;
            entitlementRepo.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new ModuleEntitlementDto(existing.Id, existing.ModuleId, existing.ApplicationId, existing.MaxAllowedCameraStreams, existing.ValidFrom, existing.ValidTo, existing.IsSuspended);
        }

        var entitlement = new ModuleEntitlement
        {
            ApplicationId = request.ApplicationId,
            ModuleId = moduleId,
            MaxAllowedCameraStreams = request.MaxAllowedCameraStreams,
            ValidFrom = DateTime.UtcNow,
            ValidTo = request.ValidTo,
            IsSuspended = false
        };

        await entitlementRepo.AddAsync(entitlement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ModuleEntitlementDto(entitlement.Id, entitlement.ModuleId, entitlement.ApplicationId, entitlement.MaxAllowedCameraStreams, entitlement.ValidFrom, entitlement.ValidTo, entitlement.IsSuspended);
    }

    public async Task RevokeEntitlementAsync(Guid moduleId, Guid applicationId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<ModuleEntitlement>();
        var entitlement = await repo.FirstOrDefaultAsync(
            e => e.ModuleId == moduleId && e.ApplicationId == applicationId,
            cancellationToken);

        if (entitlement == null)
        {
            throw new NotFoundException(ResponseMessages.Analytics.EntitlementNotFound);
        }

        repo.Delete(entitlement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PipelineAnalyticsOverviewDto>> GetPipelinesWithModulesAsync(CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<PipelineAssignment>();
        var items = await repo.GetQueryable()
            .Include(p => p.Camera)
            .Include(p => p.Instance)
            .Include(p => p.ModuleAssignments)
                .ThenInclude(ma => ma.Module)
            .ToListAsync(cancellationToken);

        return items.Select(MapToPipelineOverview).ToList();
    }

    public async Task<PipelineAnalyticsOverviewDto?> GetPipelineWithModulesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<PipelineAssignment>();
        var pipeline = await repo.GetQueryable()
            .Include(p => p.Camera)
            .Include(p => p.Instance)
            .Include(p => p.ModuleAssignments)
                .ThenInclude(ma => ma.Module)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return pipeline != null ? MapToPipelineOverview(pipeline) : null;
    }

    public async Task UpdatePipelineModulesAsync(Guid pipelineAssignmentId, UpdatePipelineModulesDto request, CancellationToken cancellationToken = default)
    {
        var pipelineRepo = _unitOfWork.Repository<PipelineAssignment>();
        var pipeline = await pipelineRepo.GetByIdAsync(pipelineAssignmentId, cancellationToken);
        if (pipeline == null)
        {
            throw new NotFoundException(ResponseMessages.Pipelines.NotFound);
        }

        var moduleAssignRepo = _unitOfWork.Repository<PipelineModuleAssignment>();
        var existingAssignments = await moduleAssignRepo.FindAsync(
            ma => ma.PipelineAssignmentId == pipelineAssignmentId,
            cancellationToken);

        foreach (var assignment in existingAssignments)
        {
            moduleAssignRepo.Delete(assignment);
        }

        var moduleRepo = _unitOfWork.Repository<AnalyticsModule>();
        var requestedModuleIds = request.ModuleIds.Distinct().ToList();

        foreach (var modId in requestedModuleIds)
        {
            var moduleExists = await moduleRepo.GetByIdAsync(modId, cancellationToken);
            if (moduleExists != null)
            {
                await moduleAssignRepo.AddAsync(new PipelineModuleAssignment
                {
                    ApplicationId = pipeline.ApplicationId,
                    PipelineAssignmentId = pipelineAssignmentId,
                    ModuleId = modId,
                    IsEnabled = true,
                    AssignedAtUtc = DateTime.UtcNow
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AnalyticsModuleDto MapToModuleDto(AnalyticsModule m) =>
        new(
            m.Id,
            m.ModuleCode,
            m.DisplayName,
            m.Description,
            m.Version,
            m.Vendor,
            m.Category,
            m.PythonEntryPoint,
            m.Icon,
            m.DocumentationUrl,
            m.MinimumCudaCapability,
            m.SupportsRealtime,
            m.SupportsBatch,
            m.DefaultConfigurationJson,
            m.Status,
            m.CreatedAt,
            m.UpdatedAt
        );

    private static PipelineAnalyticsOverviewDto MapToPipelineOverview(PipelineAssignment p) =>
        new(
            p.Id,
            p.PipelineId,
            p.PipelineName,
            p.Priority,
            p.Status.ToString(),
            p.CameraId,
            p.Camera?.Name ?? "Unknown Camera",
            p.Camera?.RtspStreamUrl ?? "",
            p.InstanceId,
            p.Instance?.InstanceId ?? "",
            p.Instance?.Hostname ?? "",
            p.ModuleAssignments.Select(ma => new PipelineModuleAssignmentDto(
                ma.ModuleId,
                ma.Module.ModuleCode,
                ma.Module.DisplayName,
                ma.Module.Category,
                ma.Module.PythonEntryPoint,
                ma.IsEnabled,
                ma.ParametersJson,
                ma.AssignedAtUtc
            )).ToList()
        );
}
