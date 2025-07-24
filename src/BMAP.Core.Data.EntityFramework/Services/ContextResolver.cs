using System.Collections.Concurrent;
using System.Reflection;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.EntityFramework.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Services;

/// <summary>
/// Default implementation of IContextResolver that resolves DbContext instances based on entity ContextAttribute.
/// </summary>
public class ContextResolver : IContextResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContextResolver> _logger;
    private readonly ConcurrentDictionary<Type, Type> _entityContextMappings;
    private readonly ConcurrentDictionary<Type, DbContext> _contextCache;

    /// <summary>
    /// Initializes a new instance of the ContextResolver class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve DbContext instances.</param>
    /// <param name="logger">The logger instance.</param>
    public ContextResolver(IServiceProvider serviceProvider, ILogger<ContextResolver> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entityContextMappings = new ConcurrentDictionary<Type, Type>();
        _contextCache = new ConcurrentDictionary<Type, DbContext>();

        // Pre-populate the mappings by scanning all registered DbContext types
        PopulateEntityContextMappings();
    }

    /// <summary>
    /// Resolves the DbContext for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The DbContext that should be used for the entity type.</returns>
    public DbContext ResolveContext<TEntity>() where TEntity : class
    {
        return ResolveContext(typeof(TEntity));
    }

    /// <summary>
    /// Resolves the DbContext for the specified entity type.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <returns>The DbContext that should be used for the entity type.</returns>
    public DbContext ResolveContext(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        _logger.LogDebug("Resolving DbContext for entity type {EntityType}", entityType.Name);

        // Check if we have a cached mapping
        if (_entityContextMappings.TryGetValue(entityType, out var contextType))
        {
            return GetOrCreateContext(contextType);
        }

        // Try to resolve using ContextAttribute
        var contextAttribute = entityType.GetCustomAttribute<ContextAttribute>();
        if (contextAttribute != null)
        {
            var resolvedContextType = ResolveContextTypeFromAttribute(contextAttribute);
            if (resolvedContextType != null)
            {
                _entityContextMappings.TryAdd(entityType, resolvedContextType);
                return GetOrCreateContext(resolvedContextType);
            }
        }

        // Fallback: try to find any DbContext that contains this entity type
        var fallbackContextType = FindContextContainingEntity(entityType);
        if (fallbackContextType != null)
        {
            _entityContextMappings.TryAdd(entityType, fallbackContextType);
            return GetOrCreateContext(fallbackContextType);
        }

        // If no specific context found, try to get the default DbContext
        var defaultContext = _serviceProvider.GetService<DbContext>();
        if (defaultContext != null)
        {
            _logger.LogWarning("No specific context found for entity {EntityType}, using default DbContext {ContextType}", 
                entityType.Name, defaultContext.GetType().Name);
            return defaultContext;
        }

        throw new InvalidOperationException($"No DbContext found for entity type {entityType.Name}. " +
            "Ensure the entity has a ContextAttribute or is registered in a DbContext.");
    }

    /// <summary>
    /// Checks if the specified entity type has a registered context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>True if a context is registered for the entity type; otherwise, false.</returns>
    public bool HasContext<TEntity>() where TEntity : class
    {
        return HasContext(typeof(TEntity));
    }

    /// <summary>
    /// Checks if the specified entity type has a registered context.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <returns>True if a context is registered for the entity type; otherwise, false.</returns>
    public bool HasContext(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        try
        {
            ResolveContext(entityType);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void PopulateEntityContextMappings()
    {
        _logger.LogDebug("Populating entity-context mappings");

        // Get all registered DbContext types from the service provider
        var contextTypes = GetRegisteredDbContextTypes();

        foreach (var contextType in contextTypes)
        {
            try
            {
                // Create a temporary instance to scan its entity types
                var context = (DbContext)_serviceProvider.GetRequiredService(contextType);
                var entityTypes = context.Model.GetEntityTypes().Select(et => et.ClrType);

                foreach (var entityType in entityTypes)
                {
                    _entityContextMappings.TryAdd(entityType, contextType);
                    _logger.LogTrace("Mapped entity {EntityType} to context {ContextType}", 
                        entityType.Name, contextType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scan context {ContextType} for entity mappings", contextType.Name);
            }
        }

        _logger.LogDebug("Populated {MappingCount} entity-context mappings", _entityContextMappings.Count);
    }

    private IEnumerable<Type> GetRegisteredDbContextTypes()
    {
        // This is a simplified approach - in a real scenario, you might want to
        // use reflection to scan for all DbContext types in the DI container
        var contextTypes = new List<Type>();

        // Try to get common DbContext registrations
        var services = _serviceProvider.GetServices<DbContext>();
        foreach (var service in services)
        {
            contextTypes.Add(service.GetType());
        }

        return contextTypes.Distinct();
    }

    private Type? ResolveContextTypeFromAttribute(ContextAttribute contextAttribute)
    {
        foreach (var contextName in contextAttribute.ContextNames)
        {
            var contextType = GetRegisteredDbContextTypes()
                .FirstOrDefault(t => t.Name == contextName);

            if (contextType != null)
            {
                _logger.LogDebug("Resolved context type {ContextType} from attribute", contextType.Name);
                return contextType;
            }
        }

        _logger.LogWarning("No registered context found for names: {ContextNames}", 
            string.Join(", ", contextAttribute.ContextNames));
        return null;
    }

    private Type? FindContextContainingEntity(Type entityType)
    {
        var contextTypes = GetRegisteredDbContextTypes();

        foreach (var contextType in contextTypes)
        {
            try
            {
                var context = (DbContext)_serviceProvider.GetService(contextType)!;
                if (context?.Model.FindEntityType(entityType) != null)
                {
                    _logger.LogDebug("Found entity {EntityType} in context {ContextType}", 
                        entityType.Name, contextType.Name);
                    return contextType;
                }
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "Error checking if entity {EntityType} exists in context {ContextType}", 
                    entityType.Name, contextType.Name);
            }
        }

        return null;
    }

    private DbContext GetOrCreateContext(Type contextType)
    {
        // For transient contexts, always get a new instance
        return (DbContext)_serviceProvider.GetRequiredService(contextType);
    }
}