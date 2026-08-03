using CCTV.Application.Engine.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCTV.Infrastructure.Messaging.BackgroundServices;

public class EngineHealthMonitorHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EngineHealthMonitorHostedService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

    public EngineHealthMonitorHostedService(
        IServiceProvider serviceProvider,
        ILogger<EngineHealthMonitorHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EngineHealthMonitorHostedService started.");

        using var timer = new PeriodicTimer(_checkInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<IEngineHeartbeatProcessingService>();

                await processingService.EvaluateNodeTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during background engine health timeout evaluation.");
            }
        }
    }
}
