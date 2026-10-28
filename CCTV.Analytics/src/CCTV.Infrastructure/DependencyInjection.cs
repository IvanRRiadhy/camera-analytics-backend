using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Identity.Interfaces;
using CCTV.Infrastructure.Messaging.BackgroundServices;
using CCTV.Infrastructure.Messaging.RabbitMQ;
using CCTV.Infrastructure.Messaging.RabbitMQ.Connection;
using CCTV.Infrastructure.Messaging.RabbitMQ.Consumer;
using CCTV.Infrastructure.Messaging.RabbitMQ.Health;
using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using CCTV.Infrastructure.Messaging.RabbitMQ.Publisher;
using CCTV.Infrastructure.Security;
using CCTV.Infrastructure.Security.Options;
using CCTV.Infrastructure.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CCTV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Configure RabbitMQ Options
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        // 2. Persistent Single Connection
        services.AddSingleton<IRabbitMqPersistentConnection, RabbitMqPersistentConnection>();

        // 3. Register Publisher & Consumer
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
        services.AddSingleton<IEngineCommandPublisher, EngineCommandPublisher>();

        // 4. Register SignalR Services
        services.AddSignalR();
        services.AddTransient<IEngineTelemetryPublisher, EngineTelemetryPublisher>();

        // 5. Background Hosted Service Consumers & Health Monitor
        services.AddHostedService<EngineRegistrationConsumerHostedService>();
        services.AddHostedService<EngineHeartbeatConsumerHostedService>();
        services.AddHostedService<EngineHealthMonitorHostedService>();
        services.AddHostedService<RuntimeTickConsumerHostedService>();
        services.AddHostedService<AnalyticsSnapshotConsumerHostedService>();

        // 6. Object Storage (S3 / SeaweedFS)
        services.Configure<CCTV.Infrastructure.Storage.StorageOptions>(
            configuration.GetSection(CCTV.Infrastructure.Storage.StorageOptions.SectionName));
        services.AddHttpClient<CCTV.Application.Abstractions.Storage.IObjectStorageService, CCTV.Infrastructure.Storage.S3CompatibleStorageService>();

        // 7. Health Checks
        services.AddHealthChecks()
                .AddCheck<RabbitMqHealthCheck>("rabbitmq_health_check");

        // 8. Security Services
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
