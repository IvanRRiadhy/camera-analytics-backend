using CCTV.Application.Common.Interfaces;
using CCTV.Persistence.Contexts;
using CCTV.Persistence.Interceptors;
using CCTV.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CCTV.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        services.AddDbContext<CCTVAnalyticsDbContext>((sp, options) =>
        {
            var auditableInterceptor = sp.GetRequiredService<AuditableEntityInterceptor>();
            var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();

            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\mssqllocaldb;Database=CCTVAnalyticsDb;Trusted_Connection=True;MultipleActiveResultSets=true";

            options.UseSqlServer(connectionString)
                   .AddInterceptors(auditableInterceptor, softDeleteInterceptor);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
