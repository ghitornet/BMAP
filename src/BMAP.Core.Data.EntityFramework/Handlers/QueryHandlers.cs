using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Handlers;

/// <summary>
/// Generic handler for retrieving all entities using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext based on entity configuration.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetAllEntitiesHandler<TEntity> : IQueryHandler<GetAllEntitiesQuery<TEntity>, BMAP.Core.Result.Result<IEnumerable<TEntity>>>
    where TEntity : class
{
    private readonly IContextResolver _contextResolver;
    private readonly ILogger<GetAllEntitiesHandler<TEntity>> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllEntitiesHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="logger">The logger instance.</param>
    public GetAllEntitiesHandler(IContextResolver contextResolver, ILogger<GetAllEntitiesHandler<TEntity>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the get all entities query asynchronously.
    /// </summary>
    /// <param name="request">The get all entities query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the collection of entities or an error.</returns>
    public async Task<BMAP.Core.Result.Result<IEnumerable<TEntity>>> HandleAsync(GetAllEntitiesQuery<TEntity> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving all entities of type {EntityType}, IncludeDeleted: {IncludeDeleted}", 
                typeof(TEntity).Name, request.IncludeDeleted);

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            var query = context.Set<TEntity>().AsQueryable();

            // Apply soft delete filter if supported and not including deleted
            if (!request.IncludeDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !((ISoftDeletable)e).IsDeleted);
            }

            var entities = await query.ToListAsync(cancellationToken);

            _logger.LogDebug("Successfully retrieved {EntityCount} entities of type {EntityType} using context {ContextType}", 
                entities.Count, typeof(TEntity).Name, context.GetType().Name);

            return BMAP.Core.Result.Result<IEnumerable<TEntity>>.Success(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(TEntity).Name);
            return BMAP.Core.Result.Result<IEnumerable<TEntity>>.Failure(BMAP.Core.Result.Error.Internal("RETRIEVE_ALL_ERROR", $"Error retrieving entities: {ex.Message}"));
        }
    }
}

/// <summary>
/// Generic handler for retrieving paginated entities using Entity Framework Core.
/// This handler automatically resolves the appropriate DbContext based on entity configuration.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetEntitiesPagedHandler<TEntity> : IQueryHandler<GetEntitiesPagedQuery<TEntity>, BMAP.Core.Result.Result<PagedResult<TEntity>>>
    where TEntity : class
{
    private readonly IContextResolver _contextResolver;
    private readonly ILogger<GetEntitiesPagedHandler<TEntity>> _logger;

    /// <summary>
    /// Initializes a new instance of the GetEntitiesPagedHandler class.
    /// </summary>
    /// <param name="contextResolver">The context resolver to get the appropriate DbContext.</param>
    /// <param name="logger">The logger instance.</param>
    public GetEntitiesPagedHandler(IContextResolver contextResolver, ILogger<GetEntitiesPagedHandler<TEntity>> logger)
    {
        _contextResolver = contextResolver ?? throw new ArgumentNullException(nameof(contextResolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the get entities paged query asynchronously.
    /// </summary>
    /// <param name="request">The get entities paged query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the paginated entities or an error.</returns>
    public async Task<BMAP.Core.Result.Result<PagedResult<TEntity>>> HandleAsync(GetEntitiesPagedQuery<TEntity> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving paged entities of type {EntityType}, Page: {PageNumber}, Size: {PageSize}, IncludeDeleted: {IncludeDeleted}", 
                typeof(TEntity).Name, request.PageNumber, request.PageSize, request.IncludeDeleted);

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                return BMAP.Core.Result.Result<PagedResult<TEntity>>.Failure(BMAP.Core.Result.Error.Validation("INVALID_PAGE_NUMBER", "Page number must be greater than 0"));
            }

            if (request.PageSize < 1 || request.PageSize > 1000) // Reasonable upper limit
            {
                return BMAP.Core.Result.Result<PagedResult<TEntity>>.Failure(BMAP.Core.Result.Error.Validation("INVALID_PAGE_SIZE", "Page size must be between 1 and 1000"));
            }

            // Resolve the appropriate context for this entity
            var context = _contextResolver.ResolveContext<TEntity>();
            
            _logger.LogTrace("Using context {ContextType} for entity {EntityType}", 
                context.GetType().Name, typeof(TEntity).Name);

            var query = context.Set<TEntity>().AsQueryable();

            // Apply soft delete filter if supported and not including deleted
            if (!request.IncludeDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !((ISoftDeletable)e).IsDeleted);
            }

            // Get total count for pagination metadata
            var totalCount = await query.CountAsync(cancellationToken);

            // Calculate pagination
            var skip = (request.PageNumber - 1) * request.PageSize;
            
            // Apply pagination with default ordering by primary key if no specific ordering is provided
            var orderedQuery = query.OrderBy(e => EF.Property<object>(e, "Id"));
            var entities = await orderedQuery
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Create paginated result
            var pagedResult = new PagedResult<TEntity>(
                entities, 
                totalCount, 
                request.PageNumber, 
                request.PageSize);

            _logger.LogDebug("Successfully retrieved {EntityCount} entities of type {EntityType} (Page {PageNumber}/{TotalPages}) using context {ContextType}", 
                entities.Count, typeof(TEntity).Name, request.PageNumber, pagedResult.TotalPages, context.GetType().Name);

            return BMAP.Core.Result.Result<PagedResult<TEntity>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged entities of type {EntityType}", typeof(TEntity).Name);
            return BMAP.Core.Result.Result<PagedResult<TEntity>>.Failure(BMAP.Core.Result.Error.Internal("RETRIEVE_PAGED_ERROR", $"Error retrieving paged entities: {ex.Message}"));
        }
    }
}