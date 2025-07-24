namespace BMAP.Core.Data.Entities;

/// <summary>
/// Defines an auditable entity that combines creation, modification, and soft deletion tracking.
/// This interface provides a complete audit trail for entities.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IAuditableEntity<TId> : IEntity<TId>, ICreatable, IModifiable, ISoftDeletable
    where TId : IEquatable<TId>
{
}

/// <summary>
/// Defines an auditable entity with an integer identifier.
/// This is a convenience interface for entities with integer identifiers that need full audit tracking.
/// </summary>
public interface IAuditableEntity : IAuditableEntity<int>, IEntity
{
}