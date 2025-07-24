using System.Reflection;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.EntityFramework.Attributes;
using BMAP.Core.Data.EntityFramework.Handlers;
using BMAP.Core.Data.EntityFramework.Services;
using BMAP.Core.Data.Entities;
using BMAP.Core.Mediator.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Extensions;

/// <summary>
/// Extension methods for registering Entity Framework Core data services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Entity Framework Core data services to the service collection.
    /// This includes registering the mediator with CQRS support and all generic CRUD handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Additional assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEntityFrameworkDataServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Add mediator with CQRS support
        services.AddMediatorWithCqrs();

        // Register the context resolver
        services.AddScoped<IContextResolver, ContextResolver>();

        // Register the audit service with fallback to system user context
        services.AddScoped<IAuditService>(provider =>
        {
            var userContext = provider.GetService<IUserContext>() ?? new SystemUserContext();
            var logger = provider.GetRequiredService<ILogger<AuditService>>();
            return new AuditService(userContext, logger);
        });

        // Add assemblies for scanning
        var allAssemblies = new List<Assembly> { typeof(ServiceCollectionExtensions).Assembly };
        if (assemblies?.Length > 0)
        {
            allAssemblies.AddRange(assemblies);
        }

        // Scan assemblies for additional handlers
        foreach (var assembly in allAssemblies)
        {
            services.RegisterHandlersFromAssembly(assembly);
        }

        return services;
    }

    /// <summary>
    /// Adds Entity Framework Core data services with automatic entity scanning and user context.
    /// This method scans the specified assemblies for entities with ContextAttribute and automatically registers handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types to scan.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEntityFrameworkDataServicesWithAutoScan(this IServiceCollection services, params Assembly[] entityAssemblies)
    {
        // Add the basic services first
        services.AddEntityFrameworkDataServices();

        // Scan and register entity handlers automatically
        foreach (var assembly in entityAssemblies)
        {
            RegisterEntityHandlersFromAssembly(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Adds Entity Framework Core data services with a custom user context.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="userContextImplementation">The user context implementation type.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types to scan.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEntityFrameworkDataServicesWithUserContext<TUserContext>(this IServiceCollection services, params Assembly[] entityAssemblies)
        where TUserContext : class, IUserContext
    {
        // Register the custom user context
        services.AddScoped<IUserContext, TUserContext>();

        // Add the rest of the services
        return services.AddEntityFrameworkDataServicesWithAutoScan(entityAssemblies);
    }

    /// <summary>
    /// Adds Entity Framework Core data services with web user context for ASP.NET Core applications.
    /// Note: This method requires Microsoft.AspNetCore.Http package for HttpContextAccessor.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="entityAssemblies">The assemblies containing entity types to scan.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEntityFrameworkDataServicesForWeb(this IServiceCollection services, params Assembly[] entityAssemblies)
    {
        // Try to add HTTP context accessor for web scenarios (if available)
        try
        {
            // Use reflection to call AddHttpContextAccessor if available
            var httpExtensionsType = Type.GetType("Microsoft.Extensions.DependencyInjection.HttpServiceCollectionExtensions, Microsoft.AspNetCore.Http");
            var addHttpContextAccessorMethod = httpExtensionsType?.GetMethod("AddHttpContextAccessor", new[] { typeof(IServiceCollection) });
            addHttpContextAccessorMethod?.Invoke(null, new object[] { services });
        }
        catch
        {
            // HttpContextAccessor not available, continue without it
        }

        // Register the web user context
        services.AddScoped<IUserContext, WebUserContext>();

        // Add the rest of the services
        return services.AddEntityFrameworkDataServicesWithAutoScan(entityAssemblies);
    }

    /// <summary>
    /// Registers generic CRUD handlers for a specific entity type using Entity Framework Core.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <typeparam name="TId">The type of the entity identifier.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEntityHandlers<TEntity, TId>(this IServiceCollection services)
        where TEntity : class
        where TId : IEquatable<TId>
    {
        // Register concrete handler implementations
        services.AddTransient<CreateEntityHandler<TEntity, TId>>();
        services.AddTransient<GetEntityByIdHandler<TEntity, TId>>();
        services.AddTransient<GetAllEntitiesHandler<TEntity>>();
        services.AddTransient<GetEntitiesPagedHandler<TEntity>>();
        services.AddTransient<UpdateEntityHandler<TEntity, TId>>();

        // Register delete handlers for specific entity type
        services.AddTransient<DeleteEntityHandler<TEntity, TId>>();

        // Register soft delete handlers if entity supports it
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            services.AddTransient<SoftDeleteEntityHandler<TEntity, TId>>();
        }

        return services;
    }

    /// <summary>
    /// Registers generic CRUD handlers for an entity type with integer identifier using Entity Framework Core.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEntityHandlers<TEntity>(this IServiceCollection services)
        where TEntity : class
    {
        services.AddEntityHandlers<TEntity, int>();

        // Additional registrations for integer-specific handlers
        services.AddTransient<CreateEntityHandler<TEntity>>();
        services.AddTransient<GetEntityByIdHandler<TEntity>>();
        services.AddTransient<UpdateEntityHandler<TEntity>>();
        services.AddTransient<DeleteEntityHandler<TEntity>>();

        // Register soft delete handlers if entity supports it
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            services.AddTransient<SoftDeleteEntityHandler<TEntity>>();
        }

        return services;
    }

    /// <summary>
    /// Adds a DbContext to the service collection with automatic entity discovery and configuration.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">An action to configure the DbContext options.</param>
    /// <param name="entityAssemblies">Assemblies to scan for entities with ContextAttribute.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDbContextWithAutoDiscovery<TContext>(this IServiceCollection services, 
        Action<DbContextOptionsBuilder> optionsAction, 
        params Assembly[] entityAssemblies)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(optionsAction);
        
        // Register the DbContext as the base DbContext for the handlers
        services.AddTransient<DbContext>(provider => provider.GetRequiredService<TContext>());

        // Auto-register entities for this context
        var contextName = typeof(TContext).Name;
        foreach (var assembly in entityAssemblies)
        {
            var entityTypes = GetEntitiesForContext(assembly, contextName);
            foreach (var entityType in entityTypes)
            {
                RegisterHandlersForEntityType(services, entityType);
            }
        }

        return services;
    }

    /// <summary>
    /// Adds SQL Server support for Entity Framework Core with automatic entity discovery.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="entityAssemblies">Assemblies to scan for entities with ContextAttribute.</param>
    /// <param name="optionsAction">Additional configuration for DbContext options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerContextWithAutoDiscovery<TContext>(this IServiceCollection services, 
        string connectionString, 
        Assembly[] entityAssemblies,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        return services.AddDbContextWithAutoDiscovery<TContext>(options =>
        {
            options.UseSqlServer(connectionString);
            optionsAction?.Invoke(options);
        }, entityAssemblies);
    }

    /// <summary>
    /// Adds SQLite support for Entity Framework Core with automatic entity discovery.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQLite connection string.</param>
    /// <param name="entityAssemblies">Assemblies to scan for entities with ContextAttribute.</param>
    /// <param name="optionsAction">Additional configuration for DbContext options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqliteContextWithAutoDiscovery<TContext>(this IServiceCollection services, 
        string connectionString, 
        Assembly[] entityAssemblies,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        return services.AddDbContextWithAutoDiscovery<TContext>(options =>
        {
            options.UseSqlite(connectionString);
            optionsAction?.Invoke(options);
        }, entityAssemblies);
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    public static IServiceCollection AddSqlServerContext<TContext>(this IServiceCollection services, string connectionString, Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlServer(connectionString);
            optionsAction?.Invoke(options);
        });

        services.AddTransient<DbContext>(provider => provider.GetRequiredService<TContext>());
        return services;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    public static IServiceCollection AddSqliteContext<TContext>(this IServiceCollection services, string connectionString, Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseSqlite(connectionString);
            optionsAction?.Invoke(options);
        });

        services.AddTransient<DbContext>(provider => provider.GetRequiredService<TContext>());
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core with automatic soft delete global query filters.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The model builder for chaining.</returns>
    public static ModelBuilder ConfigureSoftDeleteFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ServiceCollectionExtensions)
                    .GetMethod(nameof(AddSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                
                method.Invoke(null, new object[] { modelBuilder });
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures Entity Framework Core with automatic entity discovery and configuration.
    /// This method automatically discovers entities with ContextAttribute and configures them in the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="contextType">The type of the DbContext.</param>
    /// <param name="entityAssemblies">Assemblies to scan for entities.</param>
    /// <returns>The model builder for chaining.</returns>
    public static ModelBuilder ConfigureEntitiesFromAssemblies(this ModelBuilder modelBuilder, Type contextType, params Assembly[] entityAssemblies)
    {
        var contextName = contextType.Name;

        foreach (var assembly in entityAssemblies)
        {
            var entityTypes = GetEntitiesForContext(assembly, contextName);
            
            foreach (var entityType in entityTypes)
            {
                // Add entity to model if not already added
                if (modelBuilder.Model.FindEntityType(entityType) == null)
                {
                    modelBuilder.Entity(entityType);
                }
            }
        }

        // Configure soft delete filters
        modelBuilder.ConfigureSoftDeleteFilters();

        return modelBuilder;
    }

    #region Private Helper Methods

    private static void RegisterEntityHandlersFromAssembly(IServiceCollection services, Assembly assembly)
    {
        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetCustomAttribute<ContextAttribute>() != null);

        foreach (var entityType in entityTypes)
        {
            RegisterHandlersForEntityType(services, entityType);
        }
    }

    private static void RegisterHandlersForEntityType(IServiceCollection services, Type entityType)
    {
        // Use reflection to call the generic registration method
        var method = typeof(ServiceCollectionExtensions)
            .GetMethod(nameof(RegisterHandlersForEntityTypeGeneric), BindingFlags.NonPublic | BindingFlags.Static);

        // Determine the ID type - default to int
        var idType = GetEntityIdType(entityType) ?? typeof(int);

        var genericMethod = method!.MakeGenericMethod(entityType, idType);
        genericMethod.Invoke(null, new object[] { services });
    }

    private static void RegisterHandlersForEntityTypeGeneric<TEntity, TId>(IServiceCollection services)
        where TEntity : class
        where TId : IEquatable<TId>
    {
        services.AddEntityHandlers<TEntity, TId>();
    }

    private static Type? GetEntityIdType(Type entityType)
    {
        // Look for IEntity<T> interface to determine ID type
        var entityInterface = entityType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

        return entityInterface?.GetGenericArguments().FirstOrDefault();
    }

    private static IEnumerable<Type> GetEntitiesForContext(Assembly assembly, string contextName)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t =>
            {
                var contextAttribute = t.GetCustomAttribute<ContextAttribute>();
                return contextAttribute?.BelongsToContext(contextName) == true;
            });
    }

    private static void AddSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    #endregion
}