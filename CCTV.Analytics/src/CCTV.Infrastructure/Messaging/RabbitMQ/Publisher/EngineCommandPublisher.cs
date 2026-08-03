using CCTV.Application.Abstractions.Messaging;
using CCTV.Application.Common.Interfaces;
using CCTV.Contracts.Engine;
using CCTV.Infrastructure.Messaging.RabbitMQ.Constants;
using Microsoft.Extensions.Logging;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Publisher;

/// <summary>
/// RabbitMQ Command Publisher dispatching control commands to topic exchange `cctv.engine.commands`.
/// Routing Key Pattern: `engine.commands.{target_instance_id}`
/// </summary>
public class EngineCommandPublisher : IEngineCommandPublisher
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<EngineCommandPublisher> _logger;

    public EngineCommandPublisher(
        IMessagePublisher messagePublisher,
        ILogger<EngineCommandPublisher> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task DispatchCommandAsync(EngineCommandMessage command, CancellationToken cancellationToken = default)
    {
        var routingKey = $"engine.commands.{command.TargetInstanceId}";
        
        _logger.LogInformation(
            "[-->] Dispatching EngineCommandMessage ({CommandId}). Type: {CommandType}, Target: {TargetInstanceId}, RoutingKey: {RoutingKey}",
            command.CommandId,
            command.CommandType,
            command.TargetInstanceId,
            routingKey);

        await _messagePublisher.PublishAsync(
            exchange: RabbitMqTopology.Exchanges.EngineCommands,
            routingKey: routingKey,
            message: command,
            cancellationToken: cancellationToken);
    }
}
