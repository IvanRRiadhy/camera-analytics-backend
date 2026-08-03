using System.Diagnostics;
using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Engine.Interfaces;
using CCTV.Contracts.Engine;
using CCTV.Infrastructure.Messaging.RabbitMQ.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCTV.Infrastructure.Messaging.BackgroundServices;

public class EngineHeartbeatConsumerHostedService : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EngineHeartbeatConsumerHostedService> _logger;

    public EngineHeartbeatConsumerHostedService(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<EngineHeartbeatConsumerHostedService> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[+] Starting EngineHeartbeatConsumerHostedService listening on queue '{QueueName}'...", RabbitMqTopology.Queues.EngineHeartbeat);

        // Start background periodic timer loop evaluating node timeout states every 10 seconds
        _ = Task.Run(() => EvaluateTimeoutsLoopAsync(stoppingToken), stoppingToken);

        await _messageConsumer.SubscribeAsync<EngineHeartbeatMessage>(
            queueName: RabbitMqTopology.Queues.EngineHeartbeat,
            messageHandler: async (heartbeat, headers) =>
            {
                var stopwatch = Stopwatch.StartNew();

                _logger.LogDebug(
                    "[-->] Received EngineHeartbeatMessage. InstanceId: {InstanceId}, Seq: {Seq}, GPU: {GpuPct}%, Pipelines: {Count}",
                    heartbeat.InstanceId,
                    heartbeat.SequenceNumber,
                    heartbeat.SystemMetrics?.GpuMetrics?.GpuUsagePercent,
                    heartbeat.Pipelines?.Count);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var heartbeatService = scope.ServiceProvider.GetRequiredService<IEngineHeartbeatProcessingService>();
                    await heartbeatService.ProcessHeartbeatAsync(heartbeat, stoppingToken);
                }

                stopwatch.Stop();

                _logger.LogDebug(
                    "[<--] Processed Heartbeat InstanceId: {InstanceId} Seq: {Seq} in {ElapsedMs}ms.",
                    heartbeat.InstanceId,
                    heartbeat.SequenceNumber,
                    stopwatch.ElapsedMilliseconds);
            },
            cancellationToken: stoppingToken);
    }

    private async Task EvaluateTimeoutsLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var heartbeatService = scope.ServiceProvider.GetRequiredService<IEngineHeartbeatProcessingService>();
                await heartbeatService.EvaluateNodeTimeoutsAsync(stoppingToken);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error evaluating engine node heartbeat timeouts");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
