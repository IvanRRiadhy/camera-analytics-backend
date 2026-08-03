using System.Text;
using System.Text.Json;
using CCTV.Application.Abstractions.Messaging;
using CCTV.Infrastructure.Messaging.RabbitMQ.Connection;
using CCTV.Infrastructure.Messaging.RabbitMQ.Constants;
using CCTV.Infrastructure.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CCTV.Infrastructure.Messaging.RabbitMQ.Consumer;

public class RabbitMqConsumer : IMessageConsumer
{
    private readonly IRabbitMqPersistentConnection _persistentConnection;
    private readonly RabbitMqOptions _options;

    public RabbitMqConsumer(IRabbitMqPersistentConnection persistentConnection, IOptions<RabbitMqOptions> options)
    {
        _persistentConnection = persistentConnection;
        _options = options.Value;
    }

    public Task SubscribeAsync<TMessage>(
        string queueName,
        Func<TMessage, IDictionary<string, object>, Task> messageHandler,
        CancellationToken cancellationToken = default)
        where TMessage : class
    {
        if (!_persistentConnection.IsConnected)
        {
            _persistentConnection.TryConnect();
        }

        var channel = _persistentConnection.CreateModel();
        channel.BasicQos(prefetchSize: 0, prefetchCount: (ushort)_options.PrefetchCount, global: false);

        if (queueName == RabbitMqTopology.Queues.AnalyticsEvents)
        {
            channel.QueueBind(queue: queueName, exchange: RabbitMqTopology.Exchanges.Analytics, routingKey: "analytics.runtime.snapshot");
            channel.QueueBind(queue: queueName, exchange: RabbitMqTopology.Exchanges.Analytics, routingKey: "analytics.runtime.#");
            channel.QueueBind(queue: queueName, exchange: RabbitMqTopology.Exchanges.Analytics, routingKey: "analytics.event");
            channel.QueueBind(queue: queueName, exchange: RabbitMqTopology.Exchanges.Analytics, routingKey: "analytics.event.#");
            channel.QueueBind(queue: queueName, exchange: RabbitMqTopology.Exchanges.Analytics, routingKey: "analytics.#");
        }

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var headers = ea.BasicProperties.Headers ?? new Dictionary<string, object>();
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                var message = JsonSerializer.Deserialize<TMessage>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (message != null)
                {
                    await messageHandler(message, headers);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception exc)
            {
                int retryCount = 0;
                if (headers.TryGetValue("x-retry-count", out var retryObj) && retryObj != null)
                {
                    int.TryParse(retryObj.ToString(), out retryCount);
                }

                Console.WriteLine($"[RabbitMqConsumer ERROR] Deserialization/Handling failed for queue '{queueName}': {exc.Message}");

                if (retryCount < 3)
                {
                    // Increment retry count header and NACK with requeue for immediate/short retry
                    headers["x-retry-count"] = retryCount + 1;
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
                else
                {
                    // Exhausted max 3 retries: NACK with requeue=false to route message to DLQ
                    channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }
}
