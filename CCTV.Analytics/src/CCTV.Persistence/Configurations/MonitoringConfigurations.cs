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

public class EngineRuntimeConfiguration : TenantEntityConfiguration<EngineRuntime>
{
    public override void Configure(EntityTypeBuilder<EngineRuntime> builder)
    {
        base.Configure(builder);

        builder.ToTable("EngineRuntimes");

        builder.Property(er => er.Hostname).IsRequired().HasMaxLength(150);
        builder.Property(er => er.IpAddress).HasMaxLength(45);
        builder.Property(er => er.EngineVersion).HasMaxLength(30);
        builder.Property(er => er.CudaVersion).HasMaxLength(30);
        builder.Property(er => er.GpuModel).HasMaxLength(128);
        builder.Property(er => er.GpuDriver).HasMaxLength(100);
        builder.Property(er => er.CpuModel).HasMaxLength(128);
        builder.Property(er => er.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(er => new { er.ApplicationId, er.EngineInstanceId }).IsUnique();
    }
}

public class PluginRuntimeConfiguration : TenantEntityConfiguration<PluginRuntime>
{
    public override void Configure(EntityTypeBuilder<PluginRuntime> builder)
    {
        base.Configure(builder);

        builder.ToTable("PluginRuntimes");

        builder.Property(pr => pr.PluginName).IsRequired().HasMaxLength(150);
        builder.Property(pr => pr.PluginVersion).HasMaxLength(30);
        builder.Property(pr => pr.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(pr => new { pr.ApplicationId, pr.PipelineAssignmentId, pr.PluginName });
    }
}

public class AnalyticsRuntimeLogConfiguration : TenantEntityConfiguration<AnalyticsRuntimeLog>
{
    public override void Configure(EntityTypeBuilder<AnalyticsRuntimeLog> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnalyticsRuntimeLogs");

        builder.Property(l => l.EngineInstanceId).IsRequired().HasMaxLength(128);
        builder.Property(l => l.CameraId).IsRequired().HasMaxLength(128);
        builder.Property(l => l.PipelineId).IsRequired().HasMaxLength(128);
        builder.Property(l => l.PluginName).HasMaxLength(150);
        builder.Property(l => l.MetricName).HasMaxLength(150);
        builder.Property(l => l.JsonSnapshot).HasColumnType("nvarchar(max)");

        builder.HasIndex(l => new { l.ApplicationId, l.CameraId, l.TimestampUtc });
        builder.HasIndex(l => new { l.ApplicationId, l.PipelineId, l.TimestampUtc });
    }
}
