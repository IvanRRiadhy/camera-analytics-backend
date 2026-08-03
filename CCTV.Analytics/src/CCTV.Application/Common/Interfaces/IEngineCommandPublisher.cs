using CCTV.Contracts.Engine;

namespace CCTV.Application.Common.Interfaces;

/// <summary>
/// Application Abstraction interface for publishing remote control commands to AI Engine nodes over message broker.
/// </summary>
public interface IEngineCommandPublisher
{
    Task DispatchCommandAsync(EngineCommandMessage command, CancellationToken cancellationToken = default);
}
