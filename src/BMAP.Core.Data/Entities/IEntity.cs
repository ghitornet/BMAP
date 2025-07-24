namespace BMAP.Core.Data.Entities;

/// <summary>
/// Defines an entity with a unique identifier.
/// This interface provides the base contract for all entities in the system.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    TId Id { get; set; }
}

/// <summary>
/// Defines an entity with a unique identifier of type int.
/// This is a convenience interface for entities with integer identifiers.
/// </summary>
public interface IEntity : IEntity<int>
{
}