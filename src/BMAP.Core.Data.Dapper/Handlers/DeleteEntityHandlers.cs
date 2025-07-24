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
/// Generic handler for deleting entities using Dapper.
/// This handler automatically generates DELETE SQL based on entity attributes and executes the command.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class DeleteEntityHandler<TId> : ICommandHandler<DeleteEntityCommand<TId>, BMAP.Core.Result.Result>
    where TId : IEquatable<TId>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DeleteEntityHandler<TId>> _logger;
    private readonly Type _entityType;

    /// <summary>
    /// Initializes a new instance of the DeleteEntityHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="entityType">The type of entity to delete.</param>
    public DeleteEntityHandler(IDbConnection connection, ILogger<DeleteEntityHandler<TId>> logger, Type entityType)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
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
            _logger.LogDebug("Deleting entity of type {EntityType} with ID {EntityId}", _entityType.Name, request.Id);

            var sql = GenerateDeleteSql();
            _logger.LogTrace("Generated SQL: {Sql}", sql);

            var parameters = new { Id = request.Id };
            var rowsAffected = await _connection.ExecuteAsync(sql, parameters, commandTimeout: 30);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No entity of type {EntityType} found with ID {EntityId} to delete", 
                    _entityType.Name, request.Id);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {_entityType.Name} with ID {request.Id} was not found"));
            }

            _logger.LogInformation("Successfully deleted entity of type {EntityType} with ID {EntityId}. Rows affected: {RowsAffected}", 
                _entityType.Name, request.Id, rowsAffected);

            return BMAP.Core.Result.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity of type {EntityType} with ID {EntityId}", 
                _entityType.Name, request.Id);
            return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("DELETE_ERROR", $"Error deleting entity: {ex.Message}"));
        }
    }

    private string GenerateDeleteSql()
    {
        // Use reflection to call the generic method
        var method = typeof(SqlGenerator).GetMethod(nameof(SqlGenerator.GenerateDelete))!
            .MakeGenericMethod(_entityType);
        
        return (string)method.Invoke(null, null)!;
    }
}

/// <summary>
/// Handler for deleting entities with integer identifiers.
/// </summary>
public class DeleteEntityHandler : ICommandHandler<DeleteEntityCommand, BMAP.Core.Result.Result>
{
    private readonly DeleteEntityHandler<int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the DeleteEntityHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="entityType">The type of entity to delete.</param>
    public DeleteEntityHandler(IDbConnection connection, ILoggerFactory loggerFactory, Type entityType)
    {
        var logger = loggerFactory.CreateLogger<DeleteEntityHandler<int>>();
        _baseHandler = new DeleteEntityHandler<int>(connection, logger, entityType);
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
/// Generic handler for soft deleting entities using Dapper.
/// This handler automatically generates UPDATE SQL to mark entities as deleted.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class SoftDeleteEntityHandler<TId> : ICommandHandler<SoftDeleteEntityCommand<TId>, BMAP.Core.Result.Result>
    where TId : IEquatable<TId>
{
    private readonly IDbConnection _connection;
    private readonly ILogger<SoftDeleteEntityHandler<TId>> _logger;
    private readonly Type _entityType;

    /// <summary>
    /// Initializes a new instance of the SoftDeleteEntityHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="entityType">The type of entity to soft delete.</param>
    public SoftDeleteEntityHandler(IDbConnection connection, ILogger<SoftDeleteEntityHandler<TId>> logger, Type entityType)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

        if (!typeof(ISoftDeletable).IsAssignableFrom(_entityType))
        {
            throw new ArgumentException($"Entity type {_entityType.Name} does not implement ISoftDeletable", nameof(entityType));
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
            _logger.LogDebug("Soft deleting entity of type {EntityType} with ID {EntityId}", _entityType.Name, request.Id);

            var sql = GenerateSoftDeleteSql();
            _logger.LogTrace("Generated SQL: {Sql}", sql);

            var parameters = new
            {
                Id = request.Id,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = request.DeletedBy
            };

            var rowsAffected = await _connection.ExecuteAsync(sql, parameters, commandTimeout: 30);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No entity of type {EntityType} found with ID {EntityId} to soft delete", 
                    _entityType.Name, request.Id);
                return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.NotFound("ENTITY_NOT_FOUND", 
                    $"Entity of type {_entityType.Name} with ID {request.Id} was not found"));
            }

            _logger.LogInformation("Successfully soft deleted entity of type {EntityType} with ID {EntityId}. Rows affected: {RowsAffected}", 
                _entityType.Name, request.Id, rowsAffected);

            return BMAP.Core.Result.Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting entity of type {EntityType} with ID {EntityId}", 
                _entityType.Name, request.Id);
            return BMAP.Core.Result.Result.Failure(BMAP.Core.Result.Error.Internal("SOFT_DELETE_ERROR", $"Error soft deleting entity: {ex.Message}"));
        }
    }

    private string GenerateSoftDeleteSql()
    {
        // Use reflection to call the generic method
        var method = typeof(SqlGenerator).GetMethod(nameof(SqlGenerator.GenerateSoftDelete))!
            .MakeGenericMethod(_entityType);
        
        return (string)method.Invoke(null, null)!;
    }
}

/// <summary>
/// Handler for soft deleting entities with integer identifiers.
/// </summary>
public class SoftDeleteEntityHandler : ICommandHandler<SoftDeleteEntityCommand, BMAP.Core.Result.Result>
{
    private readonly SoftDeleteEntityHandler<int> _baseHandler;

    /// <summary>
    /// Initializes a new instance of the SoftDeleteEntityHandler class.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="entityType">The type of entity to soft delete.</param>
    public SoftDeleteEntityHandler(IDbConnection connection, ILoggerFactory loggerFactory, Type entityType)
    {
        var logger = loggerFactory.CreateLogger<SoftDeleteEntityHandler<int>>();
        _baseHandler = new SoftDeleteEntityHandler<int>(connection, logger, entityType);
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