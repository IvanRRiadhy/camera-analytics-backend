using CCTV.Contracts.Engine;

namespace CCTV.Application.Engine.Interfaces;

public interface IEngineHeartbeatProcessingService
{
    Task ProcessHeartbeatAsync(EngineHeartbeatMessage message, CancellationToken cancellationToken = default);
    Task EvaluateNodeTimeoutsAsync(CancellationToken cancellationToken = default);
}
