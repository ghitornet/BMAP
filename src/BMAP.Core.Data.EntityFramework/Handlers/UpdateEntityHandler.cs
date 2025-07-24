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
/// Generic handler for updating entities using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext and applies audit fields.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class UpdateEntityHandler<TEntity, TId> : ICommandHandler<UpdateEntityCommand<TEntity, TId>, BMAP.Core.Result.Result>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IContextResolver _contextResolver;
    private readonly IAuditService _auditService;
    private readonly ILogger<UpdateEntityHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="auditService">The audit service for automatic audit field management.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateEntityHandler(IContextResolver contextResolver, IAuditService auditService, ILogger<UpdateEntityHandler<TEntity, TId>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the update entity command asynchronously.
    /// </summary>
    /// <param name="request">The update entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<BMAP.Core.Result.Result> HandleAsync(UpdateEntityCommand<TEntity, TId> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            // Find the existing entity
            var query = context.Set<TEntity>().AsQueryable();
            
            // Filter by ID
            query = FilterById(query, request.Id);

            // Apply soft delete filter if supported
            if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !((ISoftDeletable)e).IsDeleted);
            }

            var existingEntity = await query.FirstOrDefaultAsync(cancellationToken);

            if (existingEntity == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found for update", typeof(TEntity).Name, request.Id);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {typeof(TEntity).Name} with ID {request.Id} was not found"));
            }

            // Update entity properties using EF Core's entry API
            var entry = context.Entry(existingEntity);
            entry.CurrentValues.SetValues(request.Entity);

            // Set audit fields using the audit service
            _auditService.SetAuditFieldsForUpdate(existingEntity);

            // Save changes (this will trigger additional audit field processing in the DbContext)
            var rowsAffected = await context.SaveChangesAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No changes detected for entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("UPDATE_FAILED", "No changes were detected for the entity"));
            }

            _logger.LogInformation("Successfully updated entity of type {EntityType} with ID {EntityId}. Rows affected: {RowsAffected} using context {ContextType} by user {UserId}", 
                typeof(TEntity).Name, request.Id, rowsAffected, context.GetType().Name, _auditService.GetCurrentUserId());

            return BMAP.Core.Result.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);
            return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("UPDATE_ERROR", $"Error updating entity: {ex.Message}"));
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
/// Handler for updating entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
public class UpdateEntityHandler<TEntity> : ICommandHandler<UpdateEntityCommand<TEntity>, BMAP.Core.Result.Result>
    where TEntity : class
{
    private readonly UpdateEntityHandler<TEntity, int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the UpdateEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="auditService">The audit service for automatic audit field management.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public UpdateEntityHandler(IContextResolver contextResolver, IAuditService auditService, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<UpdateEntityHandler<TEntity, int>>();
        _baseHandler = new UpdateEntityHandler<TEntity, int>(contextResolver, auditService, logger);
    }

    /// <summary>
    /// Handles the update entity command asynchronously.
    /// </summary>
    /// <param name="request">The update entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<BMAP.Core.Result.Result> HandleAsync(UpdateEntityCommand<TEntity> request, CancellationToken cancellationToken = default)
    {
        return await _baseHandler.HandleAsync(request, cancellationToken);
    }
}