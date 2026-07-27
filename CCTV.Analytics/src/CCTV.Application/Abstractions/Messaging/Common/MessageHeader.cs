namespace CCTV.Application.Abstractions.Messaging.Common;

public record MessageHeader(
    string MessageVersion = "1.0.0",
    Guid CorrelationId = default,
    Guid ApplicationId = default,
    DateTime Timestamp = default
);

public record MessageEnvelope<TMessage>(
    MessageHeader Header,
    TMessage Body
) where TMessage : class;
