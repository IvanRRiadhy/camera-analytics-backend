using CCTV.Domain.Entities.Orchestration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CCTV.Persistence.Configurations;

public class WorkerNodeConfiguration : TenantEntityConfiguration<WorkerNode>
{
    public override void Configure(EntityTypeBuilder<WorkerNode> builder)
    {
        base.Configure(builder);

        builder.ToTable("WorkerNodes");

        builder.Property(w => w.Hostname).IsRequired().HasMaxLength(150);
        builder.Property(w => w.IpAddress).IsRequired().HasMaxLength(45);
        builder.Property(w => w.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(w => new { w.ApplicationId, w.NodeId }).IsUnique();
        builder.HasIndex(w => new { w.ApplicationId, w.Hostname });

        builder.HasMany(w => w.Instances)
            .WithOne(i => i.Node)
            .HasForeignKey(i => i.WorkerNodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EngineInstanceConfiguration : TenantEntityConfiguration<EngineInstance>
{
    public override void Configure(EntityTypeBuilder<EngineInstance> builder)
    {
        base.Configure(builder);

        builder.ToTable("EngineInstances");

        builder.Property(i => i.Status).IsRequired().HasMaxLength(30);

        builder.HasIndex(i => new { i.ApplicationId, i.EngineId }).IsUnique();
        builder.HasIndex(i => new { i.ApplicationId, i.WorkerNodeId });

        builder.HasMany(i => i.Connections)
            .WithOne(c => c.Instance)
            .HasForeignKey(c => c.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Assignments)
            .WithOne(a => a.Instance)
            .HasForeignKey(a => a.InstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Heartbeats)
            .WithOne(h => h.Instance)
            .HasForeignKey(h => h.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EngineConnectionConfiguration : TenantEntityConfiguration<EngineConnection>
{
    public override void Configure(EntityTypeBuilder<EngineConnection> builder)
    {
        base.Configure(builder);

        builder.ToTable("EngineConnections");

        builder.Property(c => c.ConnectionProtocol).IsRequired().HasMaxLength(20);
        builder.Property(c => c.ClientIp).HasMaxLength(45);
    }
}

public class AnalyticsSessionConfiguration : TenantEntityConfiguration<AnalyticsSession>
{
    public override void Configure(EntityTypeBuilder<AnalyticsSession> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnalyticsSessions");

        builder.HasIndex(s => new { s.ApplicationId, s.CameraId, s.SessionState });

        builder.HasOne(s => s.Camera)
            .WithMany()
            .HasForeignKey(s => s.CameraId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(s => s.Configuration)
            .WithMany()
            .HasForeignKey(s => s.ConfigurationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(s => s.Assignment)
            .WithOne(a => a.Session)
            .HasForeignKey<PipelineAssignment>(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PipelineAssignmentConfiguration : TenantEntityConfiguration<PipelineAssignment>
{
    public override void Configure(EntityTypeBuilder<PipelineAssignment> builder)
    {
        base.Configure(builder);

        builder.ToTable("PipelineAssignments");

        builder.Property(a => a.AssignmentStatus).IsRequired().HasMaxLength(30);

        builder.HasIndex(a => new { a.ApplicationId, a.InstanceId, a.AssignmentStatus });
    }
}

public class EngineHeartbeatConfiguration : TenantEntityConfiguration<EngineHeartbeat>
{
    public override void Configure(EntityTypeBuilder<EngineHeartbeat> builder)
    {
        base.Configure(builder);

        builder.ToTable("EngineHeartbeats");

        builder.Property(h => h.CpuUsagePercentage).HasColumnType("decimal(5,2)");
        builder.Property(h => h.GpuUsagePercentage).HasColumnType("decimal(5,2)");
        builder.Property(h => h.InferenceFps).HasColumnType("decimal(5,2)");

        builder.HasIndex(h => new { h.ApplicationId, h.InstanceId, h.Timestamp });
    }
}
