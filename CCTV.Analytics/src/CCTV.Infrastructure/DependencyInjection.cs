using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Common.Interfaces;
using CCTV.Infrastructure.Messaging.BackgroundServices;
using CCTV.Infrastructure.Messaging.RabbitMQ;
using CCTV.Infrastructure.Messaging.RabbitMQ.Connection;
using CCTV.Infrastructure.Messaging.RabbitMQ.Consumer;
using CCTV.Infrastructure.Messaging.RabbitMQ.Health;
using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using CCTV.Infrastructure.Messaging.RabbitMQ.Publisher;
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

        // 6. Health Checks
        services.AddHealthChecks()
                .AddCheck<RabbitMqHealthCheck>("rabbitmq_health_check");

        return services;
    }
}
