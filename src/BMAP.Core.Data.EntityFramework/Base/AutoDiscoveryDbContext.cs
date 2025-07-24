using System.Reflection;
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.EntityFramework.Extensions;
using BMAP.Core.Data.EntityFramework.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Base;

/// <summary>
/// Base DbContext that automatically discovers and configures entities based on ContextAttribute
/// and provides automatic audit field management.
/// </summary>
public abstract class AutoDiscoveryDbContext : DbContext
{
    private readonly Assembly[] _entityAssemblies;
    private readonly IServiceProvider? _serviceProvider;
    private IAuditService? _auditService;
    private ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the AutoDiscoveryDbContext class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="entityAssemblies">The assemblies to scan for entities.</param>
    protected AutoDiscoveryDbContext(DbContextOptions options, params Assembly[] entityAssemblies) 
        : base(options)
    {
        _entityAssemblies = entityAssemblies ?? new[] { GetType().Assembly };
        
        // Service provider will be set externally when available
        _serviceProvider = null;
    }

    /// <summary>
    /// Initializes a new instance of the AutoDiscoveryDbContext class with service provider.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="entityAssemblies">The assemblies to scan for entities.</param>
    protected AutoDiscoveryDbContext(DbContextOptions options, IServiceProvider serviceProvider, params Assembly[] entityAssemblies) 
        : base(options)
    {
        _entityAssemblies = entityAssemblies ?? new[] { GetType().Assembly };
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Sets the service provider for this context.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public virtual void SetServiceProvider(IServiceProvider serviceProvider)
    {
        // Only set if not already set
        if (_serviceProvider == null)
        {
            // Use reflection to set the private field
            var field = GetType().BaseType?.GetField("_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, serviceProvider);
        }
    }

    /// <summary>
    /// Gets the audit service for automatic audit field management.
    /// </summary>
    protected IAuditService AuditService
    {
        get
        {
            if (_auditService == null && _serviceProvider != null)
            {
                _auditService = _serviceProvider.GetService<IAuditService>();
                if (_auditService == null)
                {
                    // Create a fallback audit service with system user context
                    var systemUserContext = new SystemUserContext();
                    var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
                    var logger = loggerFactory?.CreateLogger<AuditService>() ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<AuditService>.Instance;
                    _auditService = new AuditService(systemUserContext, logger);
                }
            }
            return _auditService ?? new AuditService(new SystemUserContext(), Microsoft.Extensions.Logging.Abstractions.NullLogger<AuditService>.Instance);
        }
    }

    /// <summary>
    /// Gets the logger for this context.
    /// </summary>
    protected ILogger Logger
    {
        get
        {
            if (_logger == null && _serviceProvider != null)
            {
                var loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
                _logger = loggerFactory?.CreateLogger(GetType()) ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            }
            return _logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }
    }

    /// <summary>
    /// Configures the model by automatically discovering entities with ContextAttribute.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities from assemblies based on ContextAttribute
        modelBuilder.ConfigureEntitiesFromAssemblies(GetType(), _entityAssemblies);

        // Configure tenant filters if any entities support tenancy
        ConfigureTenantFilters(modelBuilder);

        // Call the derived class implementation
        OnModelCreatingCore(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Override this method to perform operations before saving changes.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply audit fields automatically
            ApplyAuditFields();

            // Apply tenant filtering
            ApplyTenantFiltering();

            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while saving changes to {ContextType}", GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Override this method to perform operations before saving changes synchronously.
    /// </summary>
    /// <returns>The number of affected records.</returns>
    public override int SaveChanges()
    {
        try
        {
            // Apply audit fields automatically
            ApplyAuditFields();

            // Apply tenant filtering
            ApplyTenantFiltering();

            return base.SaveChanges();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while saving changes to {ContextType}", GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Override this method in derived classes to provide additional model configuration.
    /// </summary>
    /// <param name="modelBuilder">The model builder to configure.</param>
    protected virtual void OnModelCreatingCore(ModelBuilder modelBuilder)
    {
        // Default implementation does nothing
        // Derived classes can override this to add custom configuration
    }

    /// <summary>
    /// Applies audit fields to entities based on their state.
    /// </summary>
    protected virtual void ApplyAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || 
                       e.State == EntityState.Modified || 
                       e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            try
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        AuditService.SetAuditFieldsForCreate(entry.Entity);
                        Logger.LogTrace("Applied audit fields for creation on {EntityType}", entry.Entity.GetType().Name);
                        break;

                    case EntityState.Modified:
                        // Check if this is a soft delete operation
                        if (entry.Entity is ISoftDeletable softDeletable && 
                            entry.Property(nameof(ISoftDeletable.IsDeleted)).IsModified &&
                            softDeletable.IsDeleted)
                        {
                            AuditService.SetAuditFieldsForSoftDelete(entry.Entity);
                            Logger.LogTrace("Applied audit fields for soft delete on {EntityType}", entry.Entity.GetType().Name);
                        }
                        else
                        {
                            AuditService.SetAuditFieldsForUpdate(entry.Entity);
                            Logger.LogTrace("Applied audit fields for update on {EntityType}", entry.Entity.GetType().Name);
                        }
                        break;

                    case EntityState.Deleted:
                        // For hard deletes, we don't need to set audit fields
                        // as the entity will be removed from the database
                        Logger.LogTrace("Entity {EntityType} marked for hard deletion", entry.Entity.GetType().Name);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to apply audit fields to entity {EntityType}", entry.Entity.GetType().Name);
                // Continue processing other entities
            }
        }
    }

    /// <summary>
    /// Applies tenant filtering to ensure data isolation in multi-tenant scenarios.
    /// </summary>
    protected virtual void ApplyTenantFiltering()
    {
        var auditService = AuditService;
        if (auditService is AuditService concreteAuditService)
        {
            var userContext = _serviceProvider?.GetService<IUserContext>();
            if (userContext?.TenantId != null)
            {
                var entries = ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added && e.Entity is ITenantAware)
                    .ToList();

                foreach (var entry in entries)
                {
                    if (entry.Entity is ITenantAware tenantAware && string.IsNullOrEmpty(tenantAware.TenantId))
                    {
                        tenantAware.TenantId = userContext.TenantId;
                        Logger.LogTrace("Applied tenant ID {TenantId} to entity {EntityType}", 
                            userContext.TenantId, entry.Entity.GetType().Name);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Configures tenant-based query filters for entities that implement ITenantAware.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected virtual void ConfigureTenantFilters(ModelBuilder modelBuilder)
    {
        // This method can be overridden in derived classes to configure tenant filters
        // based on the current user's tenant ID
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantAware).IsAssignableFrom(entityType.ClrType))
            {
                var userContext = _serviceProvider?.GetService<IUserContext>();
                if (userContext?.TenantId != null)
                {
                    // Note: Global query filters with dynamic values are complex in EF Core
                    // This is a placeholder for tenant filtering implementation
                    Logger.LogTrace("Entity {EntityType} supports tenant filtering", entityType.ClrType.Name);
                }
            }
        }
    }
}

/// <summary>
/// Generic base DbContext that automatically discovers and configures entities based on ContextAttribute.
/// </summary>
/// <typeparam name="TContext">The type of the derived context.</typeparam>
public abstract class AutoDiscoveryDbContext<TContext> : AutoDiscoveryDbContext
    where TContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AutoDiscoveryDbContext class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="entityAssemblies">The assemblies to scan for entities.</param>
    protected AutoDiscoveryDbContext(DbContextOptions<TContext> options, params Assembly[] entityAssemblies) 
        : base(options, entityAssemblies)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AutoDiscoveryDbContext class with service provider.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="entityAssemblies">The assemblies to scan for entities.</param>
    protected AutoDiscoveryDbContext(DbContextOptions<TContext> options, IServiceProvider serviceProvider, params Assembly[] entityAssemblies) 
        : base(options, serviceProvider, entityAssemblies)
    {
    }
}