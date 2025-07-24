using BMAP.Core.Data.Commands;
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.EntityFramework.Services;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Handlers;

/// <summary>
/// Generic handler for deleting entities using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext based on entity configuration.
/// </summary>
/// <typeparam name="TEntity">The type of entity to delete.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class DeleteEntityHandler<TEntity, TId> : ICommandHandler<DeleteEntityCommand<TId>, BMAP.Core.Result.Result>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IContextResolver _contextResolver;
    private readonly ILogger<DeleteEntityHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the DeleteEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="logger">The logger instance.</param>
    public DeleteEntityHandler(IContextResolver contextResolver, ILogger<DeleteEntityHandler<TEntity, TId>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the delete entity command asynchronously.
    /// </summary>
    /// <param name="request">The delete entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<BMAP.Core.Result.Result> HandleAsync(DeleteEntityCommand<TId> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            // Find the entity to delete
            var query = context.Set<TEntity>().AsQueryable();
            query = FilterById(query, request.Id);

            var entity = await query.FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("No entity of type {EntityType} found with ID {EntityId} to delete", typeof(TEntity).Name, request.Id);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {typeof(TEntity).Name} with ID {request.Id} was not found"));
            }

            // Remove entity
            context.Set<TEntity>().Remove(entity);

            // Save changes
            var rowsAffected = await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted entity of type {EntityType} with ID {EntityId}. Rows affected: {RowsAffected} using context {ContextType}", 
                typeof(TEntity).Name, request.Id, rowsAffected, context.GetType().Name);

            return BMAP.Core.Result.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);
            return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("DELETE_ERROR", $"Error deleting entity: {ex.Message}"));
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
/// Handler for deleting entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to delete.</typeparam>
public class DeleteEntityHandler<TEntity> : ICommandHandler<DeleteEntityCommand, BMAP.Core.Result.Result>
    where TEntity : class
{
    private readonly DeleteEntityHandler<TEntity, int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the DeleteEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public DeleteEntityHandler(IContextResolver contextResolver, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<DeleteEntityHandler<TEntity, int>>();
        _baseHandler = new DeleteEntityHandler<TEntity, int>(contextResolver, logger);
    }

    /// <summary>
    /// Handles the delete entity command asynchronously.
    /// </summary>
    /// <param name="request">The delete entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<BMAP.Core.Result.Result> HandleAsync(DeleteEntityCommand request, CancellationToken cancellationToken = default)
    {
        return await _baseHandler.HandleAsync(request, cancellationToken);
    }
}

/// <summary>
/// Generic handler for soft deleting entities using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext and applies audit fields.
/// </summary>
/// <typeparam name="TEntity">The type of entity to soft delete.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class SoftDeleteEntityHandler<TEntity, TId> : ICommandHandler<SoftDeleteEntityCommand<TId>, BMAP.Core.Result.Result>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IContextResolver _contextResolver;
    private readonly IAuditService _auditService;
    private readonly ILogger<SoftDeleteEntityHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the SoftDeleteEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="auditService">The audit service for automatic audit field management.</param>
    /// <param name="logger">The logger instance.</param>
    public SoftDeleteEntityHandler(IContextResolver contextResolver, IAuditService auditService, ILogger<SoftDeleteEntityHandler<TEntity, TId>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            throw new ArgumentException($"Entity type {typeof(TEntity).Name} does not implement ISoftDeletable", nameof(TEntity));
        }
    }

    /// <summary>
    /// Handles the soft delete entity command asynchronously.
    /// </summary>
    /// <param name="request">The soft delete entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<BMAP.Core.Result.Result> HandleAsync(SoftDeleteEntityCommand<TId> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Soft deleting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            // Find the entity to soft delete (excluding already soft deleted ones)
            var query = context.Set<TEntity>().AsQueryable();
            query = FilterById(query, request.Id);
            query = query.Where(e => !((ISoftDeletable)e).IsDeleted);

            var entity = await query.FirstOrDefaultAsync(cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("No entity of type {EntityType} found with ID {EntityId} to soft delete", typeof(TEntity).Name, request.Id);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {typeof(TEntity).Name} with ID {request.Id} was not found"));
            }

            // Mark as soft deleted using audit service
            _auditService.SetAuditFieldsForSoftDelete(entity);

            // Save changes (this will trigger additional audit field processing in the DbContext)
            var rowsAffected = await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully soft deleted entity of type {EntityType} with ID {EntityId}. Rows affected: {RowsAffected} using context {ContextType} by user {UserId}", 
                typeof(TEntity).Name, request.Id, rowsAffected, context.GetType().Name, _auditService.GetCurrentUserId());

            return BMAP.Core.Result.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);
            return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("SOFT_DELETE_ERROR", $"Error soft deleting entity: {ex.Message}"));
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
/// Handler for soft deleting entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to soft delete.</typeparam>
public class SoftDeleteEntityHandler<TEntity> : ICommandHandler<SoftDeleteEntityCommand, BMAP.Core.Result.Result>
    where TEntity : class
{
    private readonly SoftDeleteEntityHandler<TEntity, int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the SoftDeleteEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="auditService">The audit service for automatic audit field management.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public SoftDeleteEntityHandler(IContextResolver contextResolver, IAuditService auditService, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<SoftDeleteEntityHandler<TEntity, int>>();
        _baseHandler = new SoftDeleteEntityHandler<TEntity, int>(contextResolver, auditService, logger);
    }

    /// <summary>
    /// Handles the soft delete entity command asynchronously.
    /// </summary>
    /// <param name="request">The soft delete entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<BMAP.Core.Result.Result> HandleAsync(SoftDeleteEntityCommand request, CancellationToken cancellationToken = default)
    {
        return await _baseHandler.HandleAsync(request, cancellationToken);
    }
}