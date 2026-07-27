using CCTV.Contracts.Engine;

namespace CCTV.Application.Engine.Interfaces;

public interface IEngineRegistrationService
{
    Task RegisterOrUpdateEngineAsync(EngineRegistrationRequest request, CancellationToken cancellationToken = default);
}
