namespace CCTV.Application.Abstractions.Messaging;

public interface IMessageConsumer
{
    Task SubscribeAsync<TMessage>(
        string queueName,
        Func<TMessage, IDictionary<string, object>, Task> messageHandler,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}
