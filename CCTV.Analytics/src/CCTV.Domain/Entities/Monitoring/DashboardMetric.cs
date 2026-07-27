using CCTV.Domain.Shared;

namespace CCTV.Domain.Entities.Monitoring;

public class DashboardMetric : TenantEntity
{
    public string MetricType { get; set; } = string.Empty;
    public string Granularity { get; set; } = "Hourly";
    public DateTime BucketStartTime { get; set; } = DateTime.UtcNow;
    public Guid? EntityId { get; set; }
    public decimal MetricValue { get; set; }
}
