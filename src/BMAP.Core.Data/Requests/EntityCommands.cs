using BMAP.Core.Data.Commands;

namespace BMAP.Core.Data.Requests;

/// <summary>
/// Generic command for creating a new entity.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class CreateEntityCommand<TEntity, TId> : ICreateCommand<TEntity, TId>
    where TEntity : class
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the CreateEntityCommand class.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    public CreateEntityCommand(TEntity entity)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }

    /// <summary>
    /// Gets or sets the entity to create.
    /// </summary>
    public TEntity Entity { get; set; }
}

/// <summary>
/// Generic command for creating a new entity with integer identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to create.</typeparam>
public class CreateEntityCommand<TEntity> : CreateEntityCommand<TEntity, int>, ICreateCommand<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the CreateEntityCommand class.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    public CreateEntityCommand(TEntity entity) : base(entity)
    {
    }
}

/// <summary>
/// Generic command for updating an existing entity.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class UpdateEntityCommand<TEntity, TId> : IUpdateCommand<TEntity, TId>
    where TEntity : class
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the UpdateEntityCommand class.
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="entity">The entity with updated values.</param>
    public UpdateEntityCommand(TId id, TEntity entity)
    {
        Id = id;
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
    }

    /// <summary>
    /// Gets or sets the identifier of the entity to update.
    /// </summary>
    public TId Id { get; set; }

    /// <summary>
    /// Gets or sets the entity with updated values.
    /// </summary>
    public TEntity Entity { get; set; }
}

/// <summary>
/// Generic command for updating an existing entity with integer identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to update.</typeparam>
public class UpdateEntityCommand<TEntity> : UpdateEntityCommand<TEntity, int>, IUpdateCommand<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the UpdateEntityCommand class.
    /// </summary>
    /// <param name="id">The identifier of the entity to update.</param>
    /// <param name="entity">The entity with updated values.</param>
    public UpdateEntityCommand(int id, TEntity entity) : base(id, entity)
    {
    }
}

/// <summary>
/// Generic command for deleting an entity (hard delete).
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class DeleteEntityCommand<TId> : IDeleteCommand<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the DeleteEntityCommand class.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    public DeleteEntityCommand(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets or sets the identifier of the entity to delete.
    /// </summary>
    public TId Id { get; set; }
}

/// <summary>
/// Generic command for deleting an entity with integer identifier (hard delete).
/// </summary>
public class DeleteEntityCommand : DeleteEntityCommand<int>, IDeleteCommand
{
    /// <summary>
    /// Initializes a new instance of the DeleteEntityCommand class.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    public DeleteEntityCommand(int id) : base(id)
    {
    }
}

/// <summary>
/// Generic command for soft deleting an entity.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class SoftDeleteEntityCommand<TId> : ISoftDeleteCommand<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the SoftDeleteEntityCommand class.
    /// </summary>
    /// <param name="id">The identifier of the entity to soft delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the soft delete.</param>
    public SoftDeleteEntityCommand(TId id, string? deletedBy = null)
    {
        Id = id;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Gets or sets the identifier of the entity to soft delete.
    /// </summary>
    public TId Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user performing the soft delete.
    /// </summary>
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Generic command for soft deleting an entity with integer identifier.
/// </summary>
public class SoftDeleteEntityCommand : SoftDeleteEntityCommand<int>, ISoftDeleteCommand
{
    /// <summary>
    /// Initializes a new instance of the SoftDeleteEntityCommand class.
    /// </summary>
    /// <param name="id">The identifier of the entity to soft delete.</param>
    /// <param name="deletedBy">The identifier of the user performing the soft delete.</param>
    public SoftDeleteEntityCommand(int id, string? deletedBy = null) : base(id, deletedBy)
    {
    }
}