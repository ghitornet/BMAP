namespace BMAP.Core.Data.Entities;

/// <summary>
/// Defines an entity that tracks modification metadata.
/// This interface provides properties to track when and by whom the entity was last modified.
/// </summary>
public interface IModifiable
{
    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// This value should be updated every time the entity is changed.
    /// </summary>
    DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// This can be a user ID, username, or any other identifier that makes sense for your application.
    /// </summary>
    string? LastModifiedBy { get; set; }
}