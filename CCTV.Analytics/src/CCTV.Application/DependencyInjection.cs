using CCTV.Application.Engine.Interfaces;
using CCTV.Application.Engine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CCTV.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEngineRegistrationService, EngineRegistrationService>();

        return services;
    }
}
