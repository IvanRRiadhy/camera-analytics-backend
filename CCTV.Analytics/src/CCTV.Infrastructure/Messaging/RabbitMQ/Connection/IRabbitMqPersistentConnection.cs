using RabbitMQ.Client;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Connection;

public interface IRabbitMqPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    bool TryConnect();
    IModel CreateModel();
}
