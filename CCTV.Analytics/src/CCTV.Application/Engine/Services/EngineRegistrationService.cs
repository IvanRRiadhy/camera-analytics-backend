using CCTV.Application.Common.Interfaces;
using CCTV.Application.Engine.Interfaces;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;

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

        var existingNode = await nodeRepository.FirstOrDefaultAsync(
            n => n.ApplicationId == request.ApplicationId && n.NodeId == request.NodeId,
            cancellationToken);

        if (existingNode == null)
        {
            existingNode = new WorkerNode
            {
                ApplicationId = request.ApplicationId,
                NodeId = request.NodeId,    
                Hostname = request.Hostname,
                IpAddress = request.IpAddress,
                MaxCapacityPipelineSlots = request.MaxPipelineCapacity,
                TotalGpuMemoryMb = request.TotalGpuMemoryMb,
                Status = "Active",
                LastSeenAt = DateTime.UtcNow
            };

            await nodeRepository.AddAsync(existingNode, cancellationToken);
        }
        else
        {
            existingNode.UpdateHeartbeat(request.IpAddress, request.MaxPipelineCapacity, request.TotalGpuMemoryMb);
            nodeRepository.Update(existingNode);
        }

        // Register/Update EngineInstance
        var existingInstance = await instanceRepository.FirstOrDefaultAsync(
            i => i.EngineId == request.EngineId,
            cancellationToken);

        if (existingInstance == null)
        {
            var newInstance = new EngineInstance
            {
                ApplicationId = request.ApplicationId,
                WorkerNodeId = existingNode.Id,
                EngineId = request.EngineId,
                ProcessId = 1,
                Port = 50051,
                GpuDeviceId = 0,
                AllocatedGpuMemoryMb = request.TotalGpuMemoryMb,
                ActivePipelinesCount = 0,
                Status = "Online"
            };

            await instanceRepository.AddAsync(newInstance, cancellationToken);
        }
        else
        {
            existingInstance.WorkerNodeId = existingNode.Id;
            existingInstance.MarkOnline(request.TotalGpuMemoryMb);
            instanceRepository.Update(existingInstance);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
