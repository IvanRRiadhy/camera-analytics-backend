using CCTV.Application.Abstractions.Messaging;
using CCTV.Infrastructure.Messaging.BackgroundServices;
using CCTV.Infrastructure.Messaging.RabbitMQ;
using CCTV.Infrastructure.Messaging.RabbitMQ.Connection;
using CCTV.Infrastructure.Messaging.RabbitMQ.Consumer;
using CCTV.Infrastructure.Messaging.RabbitMQ.Health;
using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using CCTV.Infrastructure.Messaging.RabbitMQ.Publisher;
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

        // 4. Background Hosted Service Consumer
        services.AddHostedService<EngineRegistrationConsumerHostedService>();

        // 5. Health Checks
        services.AddHealthChecks()
                .AddCheck<RabbitMqHealthCheck>("rabbitmq_health_check");

        return services;
    }
}
