namespace BMAP.Core.Data.Entities;

/// <summary>
/// Defines an entity that supports soft deletion.
/// Soft deletion means the entity is marked as deleted but not physically removed from the database.
/// This allows for data recovery and maintains referential integrity.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets a value indicating whether the entity is deleted.
    /// When true, the entity should be treated as deleted and excluded from normal queries.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was deleted.
    /// This value should be set when IsDeleted is set to true.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who deleted the entity.
    /// This can be a user ID, username, or any other identifier that makes sense for your application.
    /// </summary>
    string? DeletedBy { get; set; }
}