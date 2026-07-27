using CCTV.Domain.Entities.Monitoring;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CCTV.Persistence.Configurations;

public class DeviceHealthConfiguration : TenantEntityConfiguration<DeviceHealth>
{
    public override void Configure(EntityTypeBuilder<DeviceHealth> builder)
    {
        base.Configure(builder);

        builder.ToTable("DeviceHealths");

        builder.Property(dh => dh.Fps).HasColumnType("decimal(5,2)");
        builder.Property(dh => dh.PacketLossPercentage).HasColumnType("decimal(5,2)");
        builder.Property(dh => dh.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(dh => new { dh.ApplicationId, dh.CameraId, dh.CreatedAt });
    }
}

public class DashboardMetricConfiguration : TenantEntityConfiguration<DashboardMetric>
{
    public override void Configure(EntityTypeBuilder<DashboardMetric> builder)
    {
        base.Configure(builder);

        builder.ToTable("DashboardMetrics");

        builder.Property(dm => dm.MetricType).IsRequired().HasMaxLength(100);
        builder.Property(dm => dm.MetricValue).HasColumnType("decimal(18,4)");

        builder.HasIndex(dm => new { dm.ApplicationId, dm.MetricType, dm.BucketStartTime });
    }
}
