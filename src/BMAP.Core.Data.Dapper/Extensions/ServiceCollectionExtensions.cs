using System.Data;
using System.Reflection;
using BMAP.Core.Data.Dapper.Handlers;
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.Dapper.Extensions;

/// <summary>
/// Extension methods for registering Dapper data services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Dapper data services to the service collection.
    /// This includes registering the mediator with CQRS support and all generic CRUD handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Additional assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDapperDataServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Add mediator with CQRS support
        services.AddMediatorWithCqrs();

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
    /// Registers generic CRUD handlers for a specific entity type.
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

        // Register delete handlers with entity type
        services.AddTransient<DeleteEntityHandler<TId>>(provider =>
            new DeleteEntityHandler<TId>(
                provider.GetRequiredService<IDbConnection>(),
                provider.GetRequiredService<ILogger<DeleteEntityHandler<TId>>>(),
                typeof(TEntity)));

        // Register soft delete handlers if entity supports it
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            services.AddTransient<SoftDeleteEntityHandler<TId>>(provider =>
                new SoftDeleteEntityHandler<TId>(
                    provider.GetRequiredService<IDbConnection>(),
                    provider.GetRequiredService<ILogger<SoftDeleteEntityHandler<TId>>>(),
                    typeof(TEntity)));
        }

        return services;
    }

    /// <summary>
    /// Registers generic CRUD handlers for an entity type with integer identifier.
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

        services.AddTransient<DeleteEntityHandler>(provider =>
            new DeleteEntityHandler(
                provider.GetRequiredService<IDbConnection>(),
                provider.GetRequiredService<ILoggerFactory>(),
                typeof(TEntity)));

        // Register soft delete handlers if entity supports it
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            services.AddTransient<SoftDeleteEntityHandler>(provider =>
                new SoftDeleteEntityHandler(
                    provider.GetRequiredService<IDbConnection>(),
                    provider.GetRequiredService<ILoggerFactory>(),
                    typeof(TEntity)));
        }

        return services;
    }

    /// <summary>
    /// Adds a database connection factory to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionFactory">A factory function that creates database connections.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDbConnection(this IServiceCollection services, Func<IServiceProvider, IDbConnection> connectionFactory)
    {
        services.AddTransient(connectionFactory);
        return services;
    }

    /// <summary>
    /// Adds a database connection using a connection string.
    /// This method uses System.Data.SqlClient.SqlConnection by default.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerConnection(this IServiceCollection services, string connectionString)
    {
        services.AddTransient<IDbConnection>(_ => new System.Data.SqlClient.SqlConnection(connectionString));
        return services;
    }
}