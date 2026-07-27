using CCTV.Infrastructure.Messaging.RabbitMQ.Connection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Health;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IRabbitMqPersistentConnection _persistentConnection;

    public RabbitMqHealthCheck(IRabbitMqPersistentConnection persistentConnection)
    {
        _persistentConnection = persistentConnection;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_persistentConnection.IsConnected)
        {
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection is healthy and responsive."));
        }

        var isConnected = _persistentConnection.TryConnect();
        if (isConnected)
        {
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection re-established successfully."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection is unreachable."));
    }
}
