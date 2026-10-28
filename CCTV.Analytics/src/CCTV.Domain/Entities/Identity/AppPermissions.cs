namespace CCTV.Domain.Entities.Identity;

public static class AppPermissions
{
    public static class Spatial
    {
        public const string Read = "spatial.read";
        public const string Write = "spatial.write";
    }

    public static class Analytics
    {
        public const string Read = "analytics.read";
        public const string Write = "analytics.write";
    }

    public static class Orchestrator
    {
        public const string Read = "orchestrator.read";
        public const string Write = "orchestrator.write";
        public const string Execute = "orchestrator.execute";
    }

    public static class Monitoring
    {
        public const string Read = "monitoring.read";
        public const string Write = "monitoring.write";
    }

    public static class Users
    {
        public const string Read = "users.read";
        public const string Write = "users.write";
    }

    public const string AllAccess = "*";

    public static readonly IReadOnlyList<string> All = new[]
    {
        Spatial.Read,
        Spatial.Write,
        Analytics.Read,
        Analytics.Write,
        Orchestrator.Read,
        Orchestrator.Write,
        Orchestrator.Execute,
        Monitoring.Read,
        Monitoring.Write,
        Users.Read,
        Users.Write
    };

    public static readonly IReadOnlyList<string> OperatorDefaults = new[]
    {
        Spatial.Read,
        Analytics.Read,
        Orchestrator.Read,
        Orchestrator.Execute,
        Monitoring.Read,
        Monitoring.Write
    };
}
