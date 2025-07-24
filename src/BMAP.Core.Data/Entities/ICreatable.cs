namespace BMAP.Core.Data.Entities;

/// <summary>
/// Defines an entity that tracks creation metadata.
/// This interface provides properties to track when and by whom the entity was created.
/// </summary>
public interface ICreatable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// This can be a user ID, username, or any other identifier that makes sense for your application.
    /// </summary>
    string? CreatedBy { get; set; }
}