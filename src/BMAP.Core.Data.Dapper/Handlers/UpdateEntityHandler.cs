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
/// Generic handler for updating entities using Dapper.
/// This handler automatically generates UPDATE SQL based on entity attributes and executes the command.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class UpdateEntityHandler<TEntity, TId> : ICommandHandler<UpdateEntityCommand<TEntity, TId>, BMAP.Core.Result.Result>
    where TEntity : class
    where TId : IEquatable<TId>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<UpdateEntityHandler<TEntity, TId>> _logger;

    /// <summary>
    /// Initializes a new instance of the UpdateEntityHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateEntityHandler(IDbConnection connection, ILogger<UpdateEntityHandler<TEntity, TId>> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
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

            // Set audit fields if the entity supports them
            SetAuditFieldsForUpdate(request.Entity);

            // Ensure the entity has the correct ID
            if (request.Entity is IEntity<TId> entityWithId)
            {
                entityWithId.Id = request.Id;
            }

            var sql = SqlGenerator.GenerateUpdate<TEntity>();
            _logger.LogTrace("Generated SQL: {Sql}", sql);

            // Log the entity properties being used for debugging
            _logger.LogTrace("Entity properties: {@Entity}", request.Entity);

            var rowsAffected = await _connection.ExecuteAsync(sql, request.Entity);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No entity of type {EntityType} found with ID {EntityId} to update. SQL: {Sql}", 
                    typeof(TEntity).Name, request.Id, sql);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {typeof(TEntity).Name} with ID {request.Id} was not found"));
            }

            _logger.LogInformation("Successfully updated entity of type {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, request.Id);

            return BMAP.Core.Result.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity of type {EntityType} with ID {EntityId}", 
                typeof(TEntity).Name, request.Id);
            return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("UPDATE_ERROR", $"Error updating entity: {ex.Message}"));
        }
    }

    private static void SetAuditFieldsForUpdate(TEntity entity)
    {
        if (entity is IModifiable modifiable)
        {
            modifiable.LastModifiedAt = DateTime.UtcNow;
            // LastModifiedBy should be set by the caller based on the current user context
        }
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
    /// <param name="connection">The database connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public UpdateEntityHandler(IDbConnection connection, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<UpdateEntityHandler<TEntity, int>>();
        _baseHandler = new UpdateEntityHandler<TEntity, int>(connection, logger);
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