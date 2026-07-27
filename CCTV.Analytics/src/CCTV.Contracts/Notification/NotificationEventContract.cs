using CCTV.Contracts.Common;

namespace CCTV.Contracts.Notification;

public record NotificationEventContract : BaseMessage
{
    public Guid AlertId { get; init; }
    public string Channel { get; init; } = "SignalR"; // SignalR, WebPush, Email, SMS
    public string Recipient { get; init; } = string.Empty;
    public string Severity { get; init; } = "High";
    public string Title { get; init; } = string.Empty;
    public string MessageBody { get; init; } = string.Empty;
    public string? PayloadJson { get; init; }
}
