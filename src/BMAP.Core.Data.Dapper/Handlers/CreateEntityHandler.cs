using System.Data;
using BMAP.Core.Data.Commands;
using BMAP.Core.Data.Dapper.Helpers;
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.Dapper.Handlers;

/// <summary>
/// Generic handler for creating entities using Dapper.
/// This handler automatically generates INSERT SQL based on entity attributes and executes the command.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class CreateEntityHandler<TEntity, TId> : ICommandHandler<CreateEntityCommand<TEntity, TId>, BMAP.Core.Result.Result<TId>>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<CreateEntityHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the CreateEntityHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateEntityHandler(IDbConnection connection, ILogger<CreateEntityHandler<TEntity, TId>> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
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

            // Set audit fields if the entity supports them
            SetAuditFieldsForCreate(request.Entity);

            var sql = SqlGenerator.GenerateInsert<TEntity>();
            _logger.LogTrace("Generated SQL: {Sql}", sql);

            // Execute the insert and get the ID
            var result = await _connection.QuerySingleOrDefaultAsync<TId>(sql, request.Entity);
            
            if (result == null || result.Equals(default(TId)))
            {
                // For entities without identity columns, try to get the ID from the entity itself
                if (request.Entity is IEntity<TId> entityWithId)
                {
                    result = entityWithId.Id;
                }
                else
                {
                    return BMAP.Core.Result.Result<TId>.Failure(BMAP.Core.Result.Error.Internal("CREATE_FAILED", "Failed to retrieve the created entity ID"));
                }
            }

            _logger.LogInformation("Successfully created entity of type {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, result);

            return BMAP.Core.Result.Result<TId>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entity of type {EntityType}", typeof(TEntity).Name);
            return BMAP.Core.Result.Result<TId>.Failure(BMAP.Core.Result.Error.Internal("CREATE_ERROR", $"Error creating entity: {ex.Message}"));
        }
    }

    private static void SetAuditFieldsForCreate(TEntity entity)
    {
        var now = DateTime.UtcNow;

        if (entity is ICreatable creatable)
        {
            creatable.CreatedAt = now;
            // CreatedBy should be set by the caller based on the current user context
        }

        if (entity is IModifiable modifiable)
        {
            modifiable.LastModifiedAt = now;
            // LastModifiedBy should be set by the caller based on the current user context
        }

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsDeleted = false;
            softDeletable.DeletedAt = null;
            softDeletable.DeletedBy = null;
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
    /// <param name="connection">The database connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public CreateEntityHandler(IDbConnection connection, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<CreateEntityHandler<TEntity, int>>();
        _baseHandler = new CreateEntityHandler<TEntity, int>(connection, logger);
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