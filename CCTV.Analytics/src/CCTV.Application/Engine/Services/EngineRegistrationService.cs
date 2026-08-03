using System.Text.Json;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Engine.Interfaces;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;

namespace CCTV.Application.Engine.Services;

public class EngineRegistrationService : IEngineRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;

    public EngineRegistrationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterOrUpdateEngineAsync(EngineRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var nodeRepository = _unitOfWork.Repository<WorkerNode>();
        var instanceRepository = _unitOfWork.Repository<EngineInstance>();
        var capabilityRepository = _unitOfWork.Repository<EngineCapability>();

        var targetNodeId = request.WorkerNodeId != Guid.Empty ? request.WorkerNodeId : request.NodeId;
        if (targetNodeId == Guid.Empty)
        {
            targetNodeId = Guid.NewGuid();
        }

        var existingNode = await nodeRepository.FirstOrDefaultAsync(
            n => n.ApplicationId == request.ApplicationId && n.NodeId == targetNodeId,
            cancellationToken);

        if (existingNode == null)
        {
            existingNode = new WorkerNode
            {
                ApplicationId = request.ApplicationId,
                NodeId = targetNodeId,
                Hostname = !string.IsNullOrWhiteSpace(request.Hostname) ? request.Hostname : "unknown-host",
                IpAddress = !string.IsNullOrWhiteSpace(request.IpAddress) ? request.IpAddress : "127.0.0.1",
                MaxCapacityPipelineSlots = request.MaxPipelineCapacity > 0 ? request.MaxPipelineCapacity : 8,
                TotalGpuMemoryMb = request.GpuInfo.TotalVramMb > 0 ? request.GpuInfo.TotalVramMb : request.TotalGpuMemoryMb,
                Status = "Active",
                LastSeenAt = DateTime.UtcNow
            };

            await nodeRepository.AddAsync(existingNode, cancellationToken);
        }
        else
        {
            var vramMb = request.GpuInfo.TotalVramMb > 0 ? request.GpuInfo.TotalVramMb : request.TotalGpuMemoryMb;
            existingNode.UpdateHeartbeat(request.IpAddress, request.MaxPipelineCapacity, vramMb);
            nodeRepository.Update(existingNode);
        }

        var instanceIdStr = !string.IsNullOrWhiteSpace(request.InstanceId) ? request.InstanceId : request.EngineId.ToString();

        var existingInstance = await instanceRepository.FirstOrDefaultAsync(
            i => (i.InstanceId == instanceIdStr || i.EngineId == request.EngineId) && i.ApplicationId == request.ApplicationId,
            cancellationToken);

        var pluginsJson = JsonSerializer.Serialize(request.InstalledPlugins ?? new List<PluginMetadataDto>());

        EngineInstance targetInstance;

        if (existingInstance == null)
        {
            targetInstance = new EngineInstance
            {
                ApplicationId = request.ApplicationId,
                WorkerNodeId = existingNode.Id,
                InstanceId = instanceIdStr,
                EngineId = request.EngineId != Guid.Empty ? request.EngineId : Guid.NewGuid(),
                Hostname = request.Hostname,
                IpAddress = request.IpAddress,
                OsPlatform = request.OsPlatform,
                EngineVersion = !string.IsNullOrWhiteSpace(request.EngineVersion) ? request.EngineVersion : "12.6.0",
                GpuModel = request.GpuInfo?.DeviceName ?? string.Empty,
                TotalVramMb = request.GpuInfo?.TotalVramMb ?? 0,
                TotalRamMb = request.CpuInfo?.TotalRamMb ?? 0,
                InstalledPluginsJson = pluginsJson,
                RegisteredAtUtc = request.RegisteredAtUtc != default ? request.RegisteredAtUtc : DateTime.UtcNow,
                LastHeartbeatUtc = DateTime.UtcNow,
                ProcessId = 1,
                Port = 50051,
                GpuDeviceId = 0,
                AllocatedGpuMemoryMb = request.GpuInfo?.TotalVramMb ?? request.TotalGpuMemoryMb,
                ActivePipelinesCount = 0,
                State = EngineState.Online,
                Status = "Online"
            };

            await instanceRepository.AddAsync(targetInstance, cancellationToken);
        }
        else
        {
            targetInstance = existingInstance;
            targetInstance.WorkerNodeId = existingNode.Id;
            targetInstance.InstanceId = instanceIdStr;
            targetInstance.Hostname = request.Hostname;
            targetInstance.IpAddress = request.IpAddress;
            targetInstance.OsPlatform = request.OsPlatform;
            targetInstance.EngineVersion = request.EngineVersion;
            targetInstance.GpuModel = request.GpuInfo?.DeviceName ?? targetInstance.GpuModel;
            targetInstance.TotalVramMb = request.GpuInfo?.TotalVramMb ?? targetInstance.TotalVramMb;
            targetInstance.TotalRamMb = request.CpuInfo?.TotalRamMb ?? targetInstance.TotalRamMb;
            targetInstance.InstalledPluginsJson = pluginsJson;
            targetInstance.MarkOnline(request.GpuInfo?.TotalVramMb ?? request.TotalGpuMemoryMb);
            instanceRepository.Update(targetInstance);
        }

        // Sync Capability records
        if (request.InstalledPlugins != null && request.InstalledPlugins.Any())
        {
            var existingCaps = await capabilityRepository.FindAsync(
                c => c.EngineInstanceId == targetInstance.Id && c.ApplicationId == request.ApplicationId,
                cancellationToken);

            foreach (var oldCap in existingCaps)
            {
                capabilityRepository.Delete(oldCap);
            }

            foreach (var plugin in request.InstalledPlugins)
            {
                var newCap = new EngineCapability
                {
                    ApplicationId = request.ApplicationId,
                    EngineInstanceId = targetInstance.Id,
                    CapabilityCode = plugin.Name,
                    Name = plugin.Name,
                    Version = !string.IsNullOrWhiteSpace(plugin.Version) ? plugin.Version : "1.0.0",
                    SdkVersion = !string.IsNullOrWhiteSpace(plugin.SdkVersion) ? plugin.SdkVersion : "12.0"
                };
                await capabilityRepository.AddAsync(newCap, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
