using CCTV.Domain.Entities.Incident;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CCTV.Persistence.Configurations;

public class AnalyticsEventConfiguration : TenantEntityConfiguration<AnalyticsEvent>
{
    public override void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnalyticsEvents");

        builder.Property(e => e.EventType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Confidence).HasColumnType("decimal(5,2)");
        builder.Property(e => e.RawPayloadJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(e => new { e.ApplicationId, e.CameraId, e.Timestamp });
        builder.HasIndex(e => new { e.ApplicationId, e.EventType, e.Timestamp });

        builder.HasOne(e => e.Camera)
            .WithMany()
            .HasForeignKey(e => e.CameraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Session)
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Zone)
            .WithMany()
            .HasForeignKey(e => e.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Evidence)
            .WithOne(ev => ev.Event)
            .HasForeignKey<DetectionEvidence>(ev => ev.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DetectionEvidenceConfiguration : TenantEntityConfiguration<DetectionEvidence>
{
    public override void Configure(EntityTypeBuilder<DetectionEvidence> builder)
    {
        base.Configure(builder);

        builder.ToTable("DetectionEvidences");

        builder.Property(ev => ev.BucketName).IsRequired().HasMaxLength(150);
        builder.Property(ev => ev.ObjectKey).IsRequired().HasMaxLength(500);
        builder.Property(ev => ev.MimeType).IsRequired().HasMaxLength(100).HasDefaultValue("image/jpeg");
        builder.Property(ev => ev.Sha256Checksum).HasMaxLength(64);
    }
}

public class TrackSummaryConfiguration : TenantEntityConfiguration<TrackSummary>
{
    public override void Configure(EntityTypeBuilder<TrackSummary> builder)
    {
        base.Configure(builder);

        builder.ToTable("TrackSummaries");

        builder.Property(t => t.ObjectClass).IsRequired().HasMaxLength(100);
        builder.Property(t => t.AverageConfidence).HasColumnType("decimal(5,2)");
        builder.Property(t => t.DwellDurationSeconds).HasColumnType("decimal(10,2)");
        builder.Property(t => t.SpeedKmh).HasColumnType("decimal(6,2)");

        builder.HasIndex(t => new { t.ApplicationId, t.CameraId, t.FirstSeenAt });

        builder.HasOne(t => t.Camera)
            .WithMany()
            .HasForeignKey(t => t.CameraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Session)
            .WithMany()
            .HasForeignKey(t => t.SessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Visits)
            .WithOne(v => v.TrackSummary)
            .HasForeignKey(v => v.TrackSummaryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ObjectVisitConfiguration : TenantEntityConfiguration<ObjectVisit>
{
    public override void Configure(EntityTypeBuilder<ObjectVisit> builder)
    {
        base.Configure(builder);

        builder.ToTable("ObjectVisits");

        builder.Property(v => v.DwellTimeSeconds).HasColumnType("decimal(10,2)");
        builder.Property(v => v.MaxConfidenceInZone).HasColumnType("decimal(5,2)");

        builder.HasIndex(v => new { v.ApplicationId, v.ZoneId, v.EntryTime });

        builder.HasOne(v => v.Zone)
            .WithMany()
            .HasForeignKey(v => v.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AlertConfiguration : TenantEntityConfiguration<Alert>
{
    public override void Configure(EntityTypeBuilder<Alert> builder)
    {
        base.Configure(builder);

        builder.ToTable("Alerts");

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);

        builder.HasIndex(a => new { a.ApplicationId, a.Severity, a.Status, a.TriggeredAt });

        builder.HasOne(a => a.Camera)
            .WithMany()
            .HasForeignKey(a => a.CameraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Zone)
            .WithMany()
            .HasForeignKey(a => a.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class IncidentConfiguration : TenantEntityConfiguration<Incident>
{
    public override void Configure(EntityTypeBuilder<Incident> builder)
    {
        base.Configure(builder);

        builder.ToTable("Incidents");

        builder.Property(i => i.Title).IsRequired().HasMaxLength(200);
        builder.Property(i => i.CaseNumber).IsRequired().HasMaxLength(50);

        builder.HasIndex(i => new { i.ApplicationId, i.CaseNumber }).IsUnique();

        builder.HasMany(i => i.Archives)
            .WithOne(a => a.Incident)
            .HasForeignKey(a => a.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EventArchiveConfiguration : TenantEntityConfiguration<EventArchive>
{
    public override void Configure(EntityTypeBuilder<EventArchive> builder)
    {
        base.Configure(builder);

        builder.ToTable("EventArchives");

        builder.Property(ea => ea.ArchiveBatchCode).IsRequired().HasMaxLength(100);
        builder.Property(ea => ea.Sha256Checksum).HasMaxLength(64);
    }
}

public class NotificationConfiguration : TenantEntityConfiguration<Notification>
{
    public override void Configure(EntityTypeBuilder<Notification> builder)
    {
        base.Configure(builder);

        builder.ToTable("Notifications");

        builder.Property(n => n.Channel).IsRequired().HasMaxLength(50);
        builder.Property(n => n.Recipient).IsRequired().HasMaxLength(250);

        builder.HasIndex(n => new { n.ApplicationId, n.Status, n.SentAt });

        builder.HasOne(n => n.Alert)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.AlertId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
