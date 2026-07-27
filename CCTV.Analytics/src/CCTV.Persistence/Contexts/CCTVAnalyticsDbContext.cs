using System.Linq.Expressions;
using CCTV.Domain.Entities.Analytics;
using CCTV.Domain.Entities.Identity;
using CCTV.Domain.Entities.Incident;
using CCTV.Domain.Entities.Monitoring;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Entities.Spatial;
using CCTV.Domain.Shared;
using CCTV.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CCTV.Persistence.Contexts;

public class CCTVAnalyticsDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPublisher? _publisher;

    public CCTVAnalyticsDbContext(
        DbContextOptions<CCTVAnalyticsDbContext> options,
        ITenantProvider tenantProvider,
        IPublisher? publisher = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _publisher = publisher;
    }

    // Spatial & Asset
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Camera> Cameras => Set<Camera>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<CameraGroup> CameraGroups => Set<CameraGroup>();
    public DbSet<CameraGroupMember> CameraGroupMembers => Set<CameraGroupMember>();

    // Analytics Catalog & Entitlements
    public DbSet<AnalyticsModule> AnalyticsModules => Set<AnalyticsModule>();
    public DbSet<ModuleEntitlement> ModuleEntitlements => Set<ModuleEntitlement>();
    public DbSet<AnalyticsConfiguration> AnalyticsConfigurations => Set<AnalyticsConfiguration>();

    // AI Engine Orchestration
    public DbSet<WorkerNode> WorkerNodes => Set<WorkerNode>();
    public DbSet<EngineInstance> EngineInstances => Set<EngineInstance>();
    public DbSet<EngineConnection> EngineConnections => Set<EngineConnection>();
    public DbSet<AnalyticsSession> AnalyticsSessions => Set<AnalyticsSession>();
    public DbSet<PipelineAssignment> PipelineAssignments => Set<PipelineAssignment>();
    public DbSet<EngineHeartbeat> EngineHeartbeats => Set<EngineHeartbeat>();

    // Incident, Trajectory & Evidence
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<TrackSummary> TrackSummaries => Set<TrackSummary>();
    public DbSet<ObjectVisit> ObjectVisits => Set<ObjectVisit>();
    public DbSet<DetectionEvidence> DetectionEvidences => Set<DetectionEvidence>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<EventArchive> EventArchives => Set<EventArchive>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Identity & Access (IAM)
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Monitoring & Telemetry
    public DbSet<DeviceHealth> DeviceHealths => Set<DeviceHealth>();
    public DbSet<DashboardMetric> DashboardMetrics => Set<DashboardMetric>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Global Precision Convention: Fallback decimal precision for all unconfigured decimal properties
        configurationBuilder.Properties<decimal>().HavePrecision(18, 4);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Apply Parameterless Entity Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CCTVAnalyticsDbContext).Assembly);

        // 2. Dynamic Expression Tree Multi-Tenant Isolation + Soft Delete Global Query Filters
        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                // Expression: e.ApplicationId == _tenantProvider.ApplicationId
                var appIdProperty = Expression.Property(parameter, nameof(TenantEntity.ApplicationId));
                var tenantProviderRef = Expression.Constant(_tenantProvider);
                var currentAppIdProperty = Expression.Property(tenantProviderRef, nameof(ITenantProvider.ApplicationId));
                var tenantFilter = Expression.Equal(appIdProperty, currentAppIdProperty);

                // Expression: !e.IsDeleted
                var isDeletedProperty = Expression.Property(parameter, nameof(TenantEntity.IsDeleted));
                var softDeleteFilter = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                // Combined Expression: e.ApplicationId == _tenantProvider.ApplicationId && !e.IsDeleted
                var combinedFilter = Expression.AndAlso(tenantFilter, softDeleteFilter);

                var lambda = Expression.Lambda(combinedFilter, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            else if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var softDeleteFilter = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                var lambda = Expression.Lambda(softDeleteFilter, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Extract domain events from tracked entities
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        // Clear domain events from entities immediately to prevent duplicate dispatches
        foreach (var entity in domainEntities)
        {
            entity.Entity.ClearDomainEvents();
        }

        // Commit state changes to Database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events AFTER successful database transaction commit
        if (_publisher != null && domainEvents.Any())
        {
            foreach (var domainEvent in domainEvents)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }

        return result;
    }
}
