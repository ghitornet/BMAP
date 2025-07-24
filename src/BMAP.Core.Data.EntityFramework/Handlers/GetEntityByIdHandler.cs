using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Handlers;

/// <summary>
/// Generic handler for retrieving entities by ID using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext based on entity configuration.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class GetEntityByIdHandler<TEntity, TId> : IQueryHandler<GetEntityByIdQuery<TEntity, TId>, BMAP.Core.Result.Result<TEntity>>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IContextResolver _contextResolver;
    private readonly ILogger<GetEntityByIdHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the GetEntityByIdHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="logger">The logger instance.</param>
    public GetEntityByIdHandler(IContextResolver contextResolver, ILogger<GetEntityByIdHandler<TEntity, TId>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the get entity by ID query asynchronously.
    /// </summary>
    /// <param name="request">The get entity by ID query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the retrieved entity or an error.</returns>
    public async Task<BMAP.Core.Result.Result<TEntity>> HandleAsync(GetEntityByIdQuery<TEntity, TId> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            var query = context.Set<TEntity>().AsQueryable();

            // Filter by ID
            query = FilterById(query, request.Id);

            // Apply soft delete filter if supported
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !((ISoftDeletable)e).IsDeleted);
            }

            var entity = await query.FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found", typeof(TEntity).Name, request.Id);
                return BMAP.Core.Result.Result<TEntity>.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {typeof(TEntity).Name} with ID {request.Id} was not found"));
            }

            _logger.LogDebug("Successfully retrieved entity of type {EntityType} with ID {EntityId} using context {ContextType}", 
                typeof(TEntity).Name, request.Id, context.GetType().Name);

            return BMAP.Core.Result.Result<TEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);
            return BMAP.Core.Result.Result<TEntity>.Failure(BMAP.Core.Result.Error.Internal("RETRIEVE_ERROR", $"Error retrieving entity: {ex.Message}"));
        }
    }

    private static IQueryable<TEntity> FilterById(IQueryable<TEntity> query, TId id)
    {
        // Use generic approach to filter by ID
        if (typeof(TEntity).GetInterface(typeof(IEntity<TId>).Name) != null)
        {
            return query.Where(e => ((IEntity<TId>)e).Id.Equals(id));
        }

        // Fallback for entities that don't implement IEntity<TId>
        var idProperty = typeof(TEntity).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(TId))
        {
            return query.Where(e => EF.Property<TId>(e, "Id").Equals(id));
        }

        throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have a compatible Id property");
    }
}

/// <summary>
/// Handler for retrieving entities by integer ID.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetEntityByIdHandler<TEntity> : IQueryHandler<GetEntityByIdQuery<TEntity>, BMAP.Core.Result.Result<TEntity>>
    where TEntity : class
{
    private readonly GetEntityByIdHandler<TEntity, int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the GetEntityByIdHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public GetEntityByIdHandler(IContextResolver contextResolver, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<GetEntityByIdHandler<TEntity, int>>();
        _baseHandler = new GetEntityByIdHandler<TEntity, int>(contextResolver, logger);
    }

    /// <summary>
    /// Handles the get entity by ID query asynchronously.
    /// </summary>
    /// <param name="request">The get entity by ID query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the retrieved entity or an error.</returns>
    public async Task<BMAP.Core.Result.Result<TEntity>> HandleAsync(GetEntityByIdQuery<TEntity> request, CancellationToken cancellationToken = default)
    {
        return await _baseHandler.HandleAsync(request, cancellationToken);
    }
}