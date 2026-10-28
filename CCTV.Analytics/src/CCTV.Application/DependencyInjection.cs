using CCTV.Application.Engine.Interfaces;
using CCTV.Application.Engine.Services;
using CCTV.Application.Identity.Interfaces;
using CCTV.Application.Identity.Services;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Application.Orchestration.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CCTV.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. FluentValidation assembly scanning
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // 2. Application Services
        services.AddScoped<IEngineRegistrationService, EngineRegistrationService>();
        services.AddScoped<IEngineHeartbeatProcessingService, EngineHeartbeatProcessingService>();
        services.AddScoped<IPipelineRegistryService, PipelineRegistryService>();
        services.AddSingleton<IEngineHealthEvaluationService, EngineHealthEvaluationService>();
        services.AddScoped<IEngineHealthQueryService, EngineHealthQueryService>();
        services.AddScoped<IMonitoringService, MonitoringService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
