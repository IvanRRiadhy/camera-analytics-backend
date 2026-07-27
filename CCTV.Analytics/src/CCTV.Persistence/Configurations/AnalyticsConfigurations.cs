using CCTV.Domain.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CCTV.Persistence.Configurations;

public class AnalyticsModuleConfiguration : BaseEntityConfiguration<AnalyticsModule>
{
    public override void Configure(EntityTypeBuilder<AnalyticsModule> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnalyticsModules");

        builder.Property(m => m.ModuleCode).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Version).IsRequired().HasMaxLength(30);
        builder.Property(m => m.Vendor).HasMaxLength(100);
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.RequiredMinCudaCapability).HasMaxLength(20);

        builder.HasIndex(m => new { m.ModuleCode, m.Version }).IsUnique();

        builder.HasMany(m => m.Entitlements)
            .WithOne(e => e.Module)
            .HasForeignKey(e => e.ModuleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ModuleEntitlementConfiguration : TenantEntityConfiguration<ModuleEntitlement>
{
    public override void Configure(EntityTypeBuilder<ModuleEntitlement> builder)
    {
        base.Configure(builder);

        builder.ToTable("ModuleEntitlements");

        builder.HasIndex(e => new { e.ApplicationId, e.ModuleId }).IsUnique();

        builder.HasOne(e => e.Module)
            .WithMany(m => m.Entitlements)
            .HasForeignKey(e => e.ModuleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Configurations)
            .WithOne(c => c.Entitlement)
            .HasForeignKey(c => c.EntitlementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AnalyticsConfigurationConfiguration : TenantEntityConfiguration<AnalyticsConfiguration>
{
    public override void Configure(EntityTypeBuilder<AnalyticsConfiguration> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnalyticsConfigurations");

        builder.Property(ac => ac.SensitivityThreshold).HasColumnType("decimal(5,2)");
        builder.Property(ac => ac.ScheduleCron).HasMaxLength(100);

        builder.HasIndex(ac => new { ac.ApplicationId, ac.CameraId, ac.EntitlementId, ac.ZoneId }).IsUnique();

        builder.HasOne(ac => ac.Camera)
            .WithMany()
            .HasForeignKey(ac => ac.CameraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ac => ac.Zone)
            .WithMany()
            .HasForeignKey(ac => ac.ZoneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
