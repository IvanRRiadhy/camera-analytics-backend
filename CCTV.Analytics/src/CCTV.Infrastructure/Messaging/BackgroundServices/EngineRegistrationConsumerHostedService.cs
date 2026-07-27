using System.Diagnostics;
using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Engine.Interfaces;
using CCTV.Contracts.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCTV.Infrastructure.Messaging.BackgroundServices;

public class EngineRegistrationConsumerHostedService : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EngineRegistrationConsumerHostedService> _logger;

    public EngineRegistrationConsumerHostedService(
        IMessageConsumer messageConsumer,
        IServiceProvider serviceProvider,
        ILogger<EngineRegistrationConsumerHostedService> logger)
    {
        _messageConsumer = messageConsumer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[+] Starting EngineRegistrationConsumerHostedService listening on queue 'engine.events.queue'...");

        await _messageConsumer.SubscribeAsync<EngineRegistrationRequest>(
            queueName: "engine.events.queue",
            messageHandler: async (request, headers) =>
            {
                var stopwatch = Stopwatch.StartNew();

                _logger.LogInformation(
                    "[-->] Received EngineRegistrationRequest. EngineId: {EngineId}, Hostname: {Hostname}, AppId: {ApplicationId}, CorrelationId: {CorrelationId}",
                    request.EngineId,
                    request.Hostname,
                    request.ApplicationId,
                    request.CorrelationId);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var registrationService = scope.ServiceProvider.GetRequiredService<IEngineRegistrationService>();
                    await registrationService.RegisterOrUpdateEngineAsync(request, stoppingToken);
                }

                stopwatch.Stop();

                _logger.LogInformation(
                    "[<--] Successfully registered/updated EngineId: {EngineId} in {ElapsedMs}ms.",
                    request.EngineId,
                    stopwatch.ElapsedMilliseconds);
            },
            cancellationToken: stoppingToken);
    }
}
