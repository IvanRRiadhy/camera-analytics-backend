using CCTV.Application.Engine.Interfaces;
using CCTV.Application.Engine.Services;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Application.Orchestration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CCTV.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEngineRegistrationService, EngineRegistrationService>();
        services.AddScoped<IEngineHeartbeatProcessingService, EngineHeartbeatProcessingService>();
        services.AddScoped<IPipelineRegistryService, PipelineRegistryService>();
        services.AddSingleton<IEngineHealthEvaluationService, EngineHealthEvaluationService>();
        services.AddScoped<IEngineHealthQueryService, EngineHealthQueryService>();
        services.AddScoped<IMonitoringService, MonitoringService>();

        return services;
    }
}
