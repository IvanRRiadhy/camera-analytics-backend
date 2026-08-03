namespace CCTV.Infrastructure.Messaging.RabbitMQ.Constants;

public static class RabbitMqTopology
{
    public static class Exchanges
    {
        public const string Engine = "cctv.engine";
        public const string EngineCommands = "cctv.engine.commands";
        public const string EngineRegister = "cctv.engine.register";
        public const string EngineHeartbeat = "cctv.engine.heartbeat";
        public const string Analytics = "cctv.analytics";
        public const string Notification = "cctv.notification";
        public const string System = "cctv.system";
        public const string DeadLetter = "cctv.deadletter";
    }

    public static class Queues
    {
        public const string EngineEvents = "engine.events.queue";
        public const string EngineHeartbeat = "cctv.engine.heartbeat.queue";
        public const string EngineCommands = "engine.commands.queue";
        public const string AnalyticsEvents = "analytics.events.queue";
        public const string NotificationDispatches = "notification.dispatches.queue";
    }

    public static class DeadLetterQueues
    {
        public const string EngineEventsDlq = "engine.events.dlq";
        public const string EngineHeartbeatDlq = "cctv.engine.heartbeat.dlq";
        public const string EngineCommandsDlq = "engine.commands.dlq";
        public const string AnalyticsEventsDlq = "analytics.events.dlq";
        public const string NotificationDispatchesDlq = "notification.dispatches.dlq";
    }

    public static class RoutingKeys
    {
        public const string EngineRegister = "engine.register.#";
        public const string EngineHeartbeat = "engine.heartbeat.#";
        public const string EngineCommands = "engine.commands.#";
        public const string AnalyticsEvents = "analytics.event.#";
        public const string NotificationSend = "notification.send.#";
        public const string WildcardAll = "#";
    }
}
