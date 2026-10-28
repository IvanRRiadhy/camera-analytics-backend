using CCTV.Api.Security;
using CCTV.Application.Common.Interfaces;
using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCTV.Api.Controllers;

[ApiController]
[Route("api/v1/analytics/modules")]
[RequirePermission(AppPermissions.Analytics.Read)]
public class AnalyticsModulesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsModulesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all analytics modules in the plugin catalog.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetModules(CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var modules = await repo.FindAsync(x => true, cancellationToken);

        var dtos = modules.Select(m => new
        {
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
        }).OrderBy(m => m.DisplayName);

        return Ok(dtos);
    }

    /// <summary>
    /// Get a single analytics module by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetModuleById(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        if (module == null)
        {
            return NotFound(new { message = $"Analytics module with ID '{id}' not found." });
        }

        return Ok(new
        {
            module.Id,
            module.ModuleCode,
            module.DisplayName,
            module.Description,
            module.Version,
            module.Vendor,
            module.Category,
            module.PythonEntryPoint,
            module.Icon,
            module.DocumentationUrl,
            module.MinimumCudaCapability,
            module.SupportsRealtime,
            module.SupportsBatch,
            module.DefaultConfigurationJson,
            module.Status,
            module.CreatedAt,
            module.UpdatedAt
        });
    }

    /// <summary>
    /// Create a new analytics module in the catalog.
    /// </summary>
    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPost]
    public async Task<IActionResult> CreateModule([FromBody] CreateAnalyticsModuleDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ModuleCode) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { message = "ModuleCode and DisplayName are required." });
        }

        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var existing = await repo.FindAsync(m => m.ModuleCode == request.ModuleCode, cancellationToken);
        if (existing.Any())
        {
            return BadRequest(new { message = $"Module with code '{request.ModuleCode}' already exists." });
        }

        var module = new AnalyticsModule
        {
            ModuleCode = request.ModuleCode,
            DisplayName = request.DisplayName,
            Description = request.Description ?? "",
            Version = request.Version ?? "1.0.0",
            Vendor = request.Vendor ?? "Bionics AI",
            Category = request.Category ?? "SAFETY_SECURITY",
            PythonEntryPoint = request.PythonEntryPoint ?? "",
            Icon = request.Icon ?? "IconShield",
            DocumentationUrl = request.DocumentationUrl ?? "",
            MinimumCudaCapability = request.MinimumCudaCapability ?? "6.0",
            SupportsRealtime = request.SupportsRealtime,
            SupportsBatch = request.SupportsBatch,
            DefaultConfigurationJson = request.DefaultConfigurationJson ?? "{}",
            Status = request.Status ?? "Active"
        };

        await repo.AddAsync(module, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetModuleById), new { id = module.Id }, module);
    }

    /// <summary>
    /// Update an existing analytics module in the catalog.
    /// </summary>
    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateModule(Guid id, [FromBody] UpdateAnalyticsModuleDto request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        if (module == null)
        {
            return NotFound(new { message = $"Analytics module with ID '{id}' not found." });
        }

        module.DisplayName = request.DisplayName ?? module.DisplayName;
        module.Description = request.Description ?? module.Description;
        module.Version = request.Version ?? module.Version;
        module.Vendor = request.Vendor ?? module.Vendor;
        module.Category = request.Category ?? module.Category;
        module.PythonEntryPoint = request.PythonEntryPoint ?? module.PythonEntryPoint;
        module.Icon = request.Icon ?? module.Icon;
        module.DocumentationUrl = request.DocumentationUrl ?? module.DocumentationUrl;
        module.MinimumCudaCapability = request.MinimumCudaCapability ?? module.MinimumCudaCapability;
        module.SupportsRealtime = request.SupportsRealtime ?? module.SupportsRealtime;
        module.SupportsBatch = request.SupportsBatch ?? module.SupportsBatch;
        module.DefaultConfigurationJson = request.DefaultConfigurationJson ?? module.DefaultConfigurationJson;
        module.Status = request.Status ?? module.Status;

        repo.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(module);
    }

    /// <summary>
    /// Toggle status (Enable/Disable) of an analytics module.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] ToggleStatusDto request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        if (module == null)
        {
            return NotFound(new { message = $"Analytics module with ID '{id}' not found." });
        }

        module.Status = request.Status;
        repo.Update(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { id = module.Id, status = module.Status });
    }

    /// <summary>
    /// Delete an analytics module from the catalog.
    /// </summary>
    [RequirePermission(AppPermissions.Analytics.Write)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteModule(Guid id, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<AnalyticsModule>();
        var module = await repo.GetByIdAsync(id, cancellationToken);
        if (module == null)
        {
            return NotFound(new { message = $"Analytics module with ID '{id}' not found." });
        }

        repo.Delete(module);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public record CreateAnalyticsModuleDto(
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
    bool SupportsRealtime = true,
    bool SupportsBatch = true,
    string? DefaultConfigurationJson = "{}",
    string? Status = "Active"
);

public record UpdateAnalyticsModuleDto(
    string? DisplayName,
    string? Description,
    string? Version,
    string? Vendor,
    string? Category,
    string? PythonEntryPoint,
    string? Icon,
    string? DocumentationUrl,
    string? MinimumCudaCapability,
    bool? SupportsRealtime,
    bool? SupportsBatch,
    string? DefaultConfigurationJson,
    string? Status
);

public record ToggleStatusDto(string Status);
