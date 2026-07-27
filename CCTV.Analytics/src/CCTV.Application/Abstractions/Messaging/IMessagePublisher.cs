namespace CCTV.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message,
        string exchange,
        string routingKey,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
