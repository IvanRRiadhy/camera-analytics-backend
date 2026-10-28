using CCTV.Application.Analytics.Interfaces;
using CCTV.Application.Analytics.Services;
using CCTV.Application.Engine.Interfaces;
using CCTV.Application.Engine.Services;
using CCTV.Application.Identity.Interfaces;
using CCTV.Application.Identity.Services;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Application.Orchestration.Services;
using CCTV.Application.Spatial.Interfaces;
using CCTV.Application.Spatial.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CCTV.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IEngineRegistrationService, EngineRegistrationService>();
        services.AddScoped<IEngineHeartbeatProcessingService, EngineHeartbeatProcessingService>();
        services.AddScoped<IPipelineRegistryService, PipelineRegistryService>();
        services.AddSingleton<IEngineHealthEvaluationService, EngineHealthEvaluationService>();
        services.AddScoped<IEngineHealthQueryService, EngineHealthQueryService>();
        services.AddScoped<IMonitoringService, MonitoringService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISpatialService, SpatialService>();
        services.AddScoped<IAnalyticsCatalogService, AnalyticsCatalogService>();
        services.AddScoped<IRuntimeObservabilityService, RuntimeObservabilityService>();

        return services;
    }
}
