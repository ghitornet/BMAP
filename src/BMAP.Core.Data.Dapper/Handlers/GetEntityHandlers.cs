using System.Data;
using BMAP.Core.Data.Dapper.Helpers;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.Dapper.Handlers;

/// <summary>
/// Generic handler for retrieving an entity by its identifier using Dapper.
/// This handler automatically generates SELECT SQL based on entity attributes.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class GetEntityByIdHandler<TEntity, TId> : IQueryHandler<GetEntityByIdQuery<TEntity, TId>, BMAP.Core.Result.Result<TEntity>>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<GetEntityByIdHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the GetEntityByIdHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    public GetEntityByIdHandler(IDbConnection connection, ILogger<GetEntityByIdHandler<TEntity, TId>> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the get entity by ID query asynchronously.
    /// </summary>
    /// <param name="request">The get entity by ID query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the entity if found.</returns>
    public async Task<BMAP.Core.Result.Result<TEntity>> HandleAsync(GetEntityByIdQuery<TEntity, TId> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving entity of type {EntityType} with ID {EntityId}", typeof(TEntity).Name, request.Id);

            var sql = SqlGenerator.GenerateSelectById<TEntity>();
            _logger.LogTrace("Generated SQL: {Sql}", sql);

            var entity = await _connection.QuerySingleOrDefaultAsync<TEntity>(sql, new { Id = request.Id });

            if (entity == null)
            {
                _logger.LogWarning("Entity of type {EntityType} with ID {EntityId} not found", typeof(TEntity).Name, request.Id);
                return BMAP.Core.Result.Result<TEntity>.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {typeof(TEntity).Name} with ID {request.Id} was not found"));
            }

            _logger.LogDebug("Successfully retrieved entity of type {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, request.Id);

            return BMAP.Core.Result.Result<TEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entity of type {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, request.Id);
            return BMAP.Core.Result.Result<TEntity>.Failure(BMAP.Core.Result.Error.Internal("RETRIEVE_ERROR", 
                $"Error retrieving entity: {ex.Message}"));
        }
    }
}

/// <summary>
/// Handler for retrieving entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetEntityByIdHandler<TEntity> : IQueryHandler<GetEntityByIdQuery<TEntity>, BMAP.Core.Result.Result<TEntity>>
    where TEntity : class
{
    private readonly GetEntityByIdHandler<TEntity, int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the GetEntityByIdHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public GetEntityByIdHandler(IDbConnection connection, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<GetEntityByIdHandler<TEntity, int>>();
        _baseHandler = new GetEntityByIdHandler<TEntity, int>(connection, logger);
    }

    /// <summary>
    /// Handles the get entity by ID query asynchronously.
    /// </summary>
    /// <param name="request">The get entity by ID query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the entity if found.</returns>
    public async Task<BMAP.Core.Result.Result<TEntity>> HandleAsync(GetEntityByIdQuery<TEntity> request, CancellationToken cancellationToken = default)
    {
        return await _baseHandler.HandleAsync(request, cancellationToken);
    }
}

/// <summary>
/// Generic handler for retrieving all entities using Dapper.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetAllEntitiesHandler<TEntity> : IQueryHandler<GetAllEntitiesQuery<TEntity>, BMAP.Core.Result.Result<IEnumerable<TEntity>>>
    where TEntity : class
{
    private readonly IDbConnection _connection;
    private readonly ILogger<GetAllEntitiesHandler<TEntity>> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllEntitiesHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    public GetAllEntitiesHandler(IDbConnection connection, ILogger<GetAllEntitiesHandler<TEntity>> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the get all entities query asynchronously.
    /// </summary>
    /// <param name="request">The get all entities query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the collection of entities.</returns>
    public async Task<BMAP.Core.Result.Result<IEnumerable<TEntity>>> HandleAsync(GetAllEntitiesQuery<TEntity> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving all entities of type {EntityType}, IncludeDeleted: {IncludeDeleted}", 
                typeof(TEntity).Name, request.IncludeDeleted);

            var sql = SqlGenerator.GenerateSelectAll<TEntity>(request.IncludeDeleted);
            _logger.LogTrace("Generated SQL: {Sql}", sql);

            var entities = await _connection.QueryAsync<TEntity>(sql);

            _logger.LogDebug("Successfully retrieved {Count} entities of type {EntityType}", 
                entities.Count(), typeof(TEntity).Name);

            return BMAP.Core.Result.Result<IEnumerable<TEntity>>.Success(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(TEntity).Name);
            return BMAP.Core.Result.Result<IEnumerable<TEntity>>.Failure(BMAP.Core.Result.Error.Internal("RETRIEVE_ALL_ERROR", 
                $"Error retrieving entities: {ex.Message}"));
        }
    }
}

/// <summary>
/// Generic handler for retrieving entities with pagination using Dapper.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetEntitiesPagedHandler<TEntity> : IQueryHandler<GetEntitiesPagedQuery<TEntity>, BMAP.Core.Result.Result<PagedResult<TEntity>>>
    where TEntity : class
{
    private readonly IDbConnection _connection;
    private readonly ILogger<GetEntitiesPagedHandler<TEntity>> _logger;

    /// <summary>
    /// Initializes a new instance of the GetEntitiesPagedHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    public GetEntitiesPagedHandler(IDbConnection connection, ILogger<GetEntitiesPagedHandler<TEntity>> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the get entities paged query asynchronously.
    /// </summary>
    /// <param name="request">The get entities paged query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the paged collection of entities.</returns>
    public async Task<BMAP.Core.Result.Result<PagedResult<TEntity>>> HandleAsync(GetEntitiesPagedQuery<TEntity> request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving paged entities of type {EntityType}, Page: {PageNumber}, Size: {PageSize}, IncludeDeleted: {IncludeDeleted}", 
                typeof(TEntity).Name, request.PageNumber, request.PageSize, request.IncludeDeleted);

            // Get total count
            var countSql = SqlGenerator.GenerateCount<TEntity>(request.IncludeDeleted);
            var totalCount = await _connection.QuerySingleAsync<int>(countSql);

            // Get paged data
            var selectSql = SqlGenerator.GenerateSelectPaged<TEntity>(request.IncludeDeleted);
            var offset = (request.PageNumber - 1) * request.PageSize;
            
            var entities = await _connection.QueryAsync<TEntity>(selectSql, new 
            { 
                Offset = offset, 
                PageSize = request.PageSize 
            });

            var pagedResult = new PagedResult<TEntity>(entities, totalCount, request.PageNumber, request.PageSize);

            _logger.LogDebug("Successfully retrieved {Count} entities of type {EntityType} (Page {PageNumber}/{TotalPages})", 
                entities.Count(), typeof(TEntity).Name, request.PageNumber, pagedResult.TotalPages);

            return BMAP.Core.Result.Result<PagedResult<TEntity>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged entities of type {EntityType}", typeof(TEntity).Name);
            return BMAP.Core.Result.Result<PagedResult<TEntity>>.Failure(BMAP.Core.Result.Error.Internal("RETRIEVE_PAGED_ERROR", 
                $"Error retrieving paged entities: {ex.Message}"));
        }
    }
}