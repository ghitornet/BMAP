using BMAP.Core.Mediator;

namespace BMAP.Core.Data.Commands;

/// <summary>
/// Base interface for all data commands that return a Result.
/// This interface extends ICommand to provide consistent result handling for data operations.
/// </summary>
public interface IDataCommand : ICommand<BMAP.Core.Result.Result>
{
}

/// <summary>
/// Base interface for all data commands that return a Result with a value.
/// This interface extends ICommand to provide consistent result handling for data operations.
/// </summary>
/// <typeparam name="TResponse">The type of the command response.</typeparam>
public interface IDataCommand<TResponse> : ICommand<BMAP.Core.Result.Result<TResponse>>
{
}

/// <summary>
/// Interface for commands that create a new entity.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface ICreateCommand<TEntity, TId> : IDataCommand<TId>
    where TEntity : class
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the entity to create.
    /// </summary>
    TEntity Entity { get; set; }
}

/// <summary>
/// Interface for commands that create a new entity with integer identifier.
/// This is a convenience interface for entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
public interface ICreateCommand<TEntity> : ICreateCommand<TEntity, int>
    where TEntity : class
{
}

/// <summary>
/// Interface for commands that update an existing entity.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IUpdateCommand<TEntity, TId> : IDataCommand
    where TEntity : class
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the identifier of the entity to update.
    /// </summary>
    TId Id { get; set; }

    /// <summary>
    /// Gets or sets the entity with updated values.
    /// </summary>
    TEntity Entity { get; set; }
}

/// <summary>
/// Interface for commands that update an existing entity with integer identifier.
/// This is a convenience interface for entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
public interface IUpdateCommand<TEntity> : IUpdateCommand<TEntity, int>
    where TEntity : class
{
}

/// <summary>
/// Interface for commands that delete an entity (hard delete).
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IDeleteCommand<TId> : IDataCommand
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the identifier of the entity to delete.
    /// </summary>
    TId Id { get; set; }
}

/// <summary>
/// Interface for commands that delete an entity with integer identifier (hard delete).
/// This is a convenience interface for entities with integer identifiers.
/// </summary>
public interface IDeleteCommand : IDeleteCommand<int>
{
}

/// <summary>
/// Interface for commands that soft delete an entity.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface ISoftDeleteCommand<TId> : IDataCommand
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the identifier of the entity to soft delete.
    /// </summary>
    TId Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user performing the soft delete.
    /// </summary>
    string? DeletedBy { get; set; }
}

/// <summary>
/// Interface for commands that soft delete an entity with integer identifier.
/// This is a convenience interface for entities with integer identifiers.
/// </summary>
public interface ISoftDeleteCommand : ISoftDeleteCommand<int>
{
}