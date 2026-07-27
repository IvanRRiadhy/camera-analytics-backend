using CCTV.Application.Abstractions.Messaging;

namespace CCTV.Infrastructure.Messaging.RabbitMQ;

public class RabbitMqMessageBus : IMessageBus
{
    private readonly IMessagePublisher _publisher;
    private readonly IMessageConsumer _consumer;

    public RabbitMqMessageBus(IMessagePublisher publisher, IMessageConsumer consumer)
    {
        _publisher = publisher;
        _consumer = consumer;
    }

    public Task PublishAsync<TMessage>(TMessage message, string exchange, string routingKey, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        return _publisher.PublishAsync(message, exchange, routingKey, cancellationToken);
    }

    public Task SubscribeAsync<TMessage>(string queueName, Func<TMessage, IDictionary<string, object>, Task> messageHandler, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        return _consumer.SubscribeAsync(queueName, messageHandler, cancellationToken);
    }
}
