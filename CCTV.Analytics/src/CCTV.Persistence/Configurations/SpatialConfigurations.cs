using CCTV.Domain.Entities.Spatial;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CCTV.Persistence.Configurations;

public class SiteConfiguration : TenantEntityConfiguration<Site>
{
    public override void Configure(EntityTypeBuilder<Site> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sites");

        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.Address).HasMaxLength(250);
        builder.Property(s => s.Latitude).HasColumnType("decimal(9,6)");
        builder.Property(s => s.Longitude).HasColumnType("decimal(9,6)");
        builder.Property(s => s.TimeZone).IsRequired().HasMaxLength(100).HasDefaultValue("UTC");

        builder.HasIndex(s => new { s.ApplicationId, s.Code }).IsUnique();

        builder.HasMany(s => s.Buildings)
            .WithOne(b => b.Site)
            .HasForeignKey(b => b.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Cameras)
            .WithOne(c => c.Site)
            .HasForeignKey(c => c.SiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BuildingConfiguration : TenantEntityConfiguration<Building>
{
    public override void Configure(EntityTypeBuilder<Building> builder)
    {
        base.Configure(builder);

        builder.ToTable("Buildings");

        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.BuildingCode).IsRequired().HasMaxLength(50);

        builder.HasIndex(b => new { b.ApplicationId, b.SiteId, b.BuildingCode }).IsUnique();

        builder.HasMany(b => b.Floors)
            .WithOne(f => f.Building)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FloorConfiguration : TenantEntityConfiguration<Floor>
{
    public override void Configure(EntityTypeBuilder<Floor> builder)
    {
        base.Configure(builder);

        builder.ToTable("Floors");

        builder.Property(f => f.Name).IsRequired().HasMaxLength(100);
        builder.Property(f => f.FloorPlanImageUrl).HasMaxLength(1000);

        builder.HasIndex(f => new { f.ApplicationId, f.BuildingId, f.FloorLevel }).IsUnique();

        builder.HasMany(f => f.Cameras)
            .WithOne(c => c.Floor)
            .HasForeignKey(c => c.FloorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CameraConfiguration : TenantEntityConfiguration<Camera>
{
    public override void Configure(EntityTypeBuilder<Camera> builder)
    {
        base.Configure(builder);

        builder.ToTable("Cameras");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.DeviceCode).IsRequired().HasMaxLength(100);
        builder.Property(c => c.RtspStreamUrl).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.IpAddress).HasMaxLength(45);
        builder.Property(c => c.MacAddress).HasMaxLength(17);
        builder.Property(c => c.Resolution).HasMaxLength(20).HasDefaultValue("1080p");
        builder.Property(c => c.ScaleFactorMetersPerPixel).HasColumnType("decimal(10,6)");

        builder.HasIndex(c => new { c.ApplicationId, c.DeviceCode }).IsUnique();
        builder.HasIndex(c => new { c.ApplicationId, c.SiteId });
        builder.HasIndex(c => new { c.ApplicationId, c.Status });

        builder.HasMany(c => c.Zones)
            .WithOne(z => z.Camera)
            .HasForeignKey(z => z.CameraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ZoneConfiguration : TenantEntityConfiguration<Zone>
{
    public override void Configure(EntityTypeBuilder<Zone> builder)
    {
        base.Configure(builder);

        builder.ToTable("Zones");

        builder.Property(z => z.Name).IsRequired().HasMaxLength(100);
        builder.Property(z => z.PolygonCoordinatesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(z => z.ColorHex).HasMaxLength(9).HasDefaultValue("#FF0000");

        builder.HasIndex(z => new { z.ApplicationId, z.CameraId, z.Name }).IsUnique();
    }
}

public class CameraGroupConfiguration : TenantEntityConfiguration<CameraGroup>
{
    public override void Configure(EntityTypeBuilder<CameraGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("CameraGroups");

        builder.Property(cg => cg.Name).IsRequired().HasMaxLength(100);
        builder.Property(cg => cg.Description).HasMaxLength(500);

        builder.HasIndex(cg => new { cg.ApplicationId, cg.Name }).IsUnique();

        builder.HasMany(cg => cg.Members)
            .WithOne(m => m.CameraGroup)
            .HasForeignKey(m => m.CameraGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CameraGroupMemberConfiguration : TenantEntityConfiguration<CameraGroupMember>
{
    public override void Configure(EntityTypeBuilder<CameraGroupMember> builder)
    {
        base.Configure(builder);

        builder.ToTable("CameraGroupMembers");

        builder.HasIndex(m => new { m.ApplicationId, m.CameraGroupId, m.CameraId }).IsUnique();

        builder.HasOne(m => m.Camera)
            .WithMany(c => c.CameraGroupMembers)
            .HasForeignKey(m => m.CameraId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
