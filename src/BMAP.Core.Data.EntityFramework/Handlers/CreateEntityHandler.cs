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
/// Generic handler for creating entities using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext and applies audit fields.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class CreateEntityHandler<TEntity, TId> : ICommandHandler<CreateEntityCommand<TEntity, TId>, BMAP.Core.Result.Result<TId>>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IContextResolver _contextResolver;
    private readonly IAuditService _auditService;
    private readonly ILogger<CreateEntityHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="auditService">The audit service for automatic audit field management.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateEntityHandler(IContextResolver contextResolver, IAuditService auditService, ILogger<CreateEntityHandler<TEntity, TId>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the create entity command asynchronously.
    /// </summary>
    /// <param name="request">The create entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the created entity's identifier.</returns>
    public async Task<BMAP.Core.Result.Result<TId>> HandleAsync(CreateEntityCommand<TEntity, TId> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating entity of type {EntityType}", typeof(TEntity).Name);

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            // Set audit fields using the audit service
            _auditService.SetAuditFieldsForCreate(request.Entity);

            // Add entity to context
            var entry = await context.Set<TEntity>().AddAsync(request.Entity, cancellationToken);
            
            // Save changes (this will trigger additional audit field processing in the DbContext)
            await context.SaveChangesAsync(cancellationToken);

            // Get the ID
            TId entityId;
            if (request.Entity is IEntity<TId> entityWithId)
            {
                entityId = entityWithId.Id;
            }
            else
            {
                return BMAP.Core.Result.Result<TId>.Failure(BMAP.Core.Result.Error.Internal("CREATE_FAILED", "Cannot retrieve ID from created entity"));
            }

            _logger.LogInformation("Successfully created entity of type {EntityType} with ID {EntityId} using context {ContextType} by user {UserId}", 
                typeof(TEntity).Name, entityId, context.GetType().Name, _auditService.GetCurrentUserId());

            return BMAP.Core.Result.Result<TId>.Success(entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity of type {EntityType}", typeof(TEntity).Name);
            return BMAP.Core.Result.Result<TId>.Failure(BMAP.Core.Result.Error.Internal("CREATE_ERROR", $"Error creating entity: {ex.Message}"));
        }
    }
}

/// <summary>
/// Handler for creating entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
public class CreateEntityHandler<TEntity> : ICommandHandler<CreateEntityCommand<TEntity>, BMAP.Core.Result.Result<int>>
    where TEntity : class
{
    private readonly CreateEntityHandler<TEntity, int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the CreateEntityHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="auditService">The audit service for automatic audit field management.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public CreateEntityHandler(IContextResolver contextResolver, IAuditService auditService, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<CreateEntityHandler<TEntity, int>>();
        _baseHandler = new CreateEntityHandler<TEntity, int>(contextResolver, auditService, logger);
    }

    /// <summary>
    /// Handles the create entity command asynchronously.
    /// </summary>
    /// <param name="request">The create entity command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the created entity's identifier.</returns>
    public async Task<BMAP.Core.Result.Result<int>> HandleAsync(CreateEntityCommand<TEntity> request, CancellationToken cancellationToken = default)
    {
        return await _baseHandler.HandleAsync(request, cancellationToken);
    }
}