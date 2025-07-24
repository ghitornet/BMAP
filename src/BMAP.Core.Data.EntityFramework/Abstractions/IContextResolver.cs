using Microsoft.EntityFrameworkCore;

namespace BMAP.Core.Data.EntityFramework.Abstractions;

/// <summary>
/// Provides methods to resolve the appropriate DbContext for a given entity type.
/// </summary>
public interface IContextResolver
{
    /// <summary>
    /// Resolves the DbContext for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The DbContext that should be used for the entity type.</returns>
    DbContext ResolveContext<TEntity>() where TEntity : class;

    /// <summary>
    /// Resolves the DbContext for the specified entity type.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <returns>The DbContext that should be used for the entity type.</returns>
    DbContext ResolveContext(Type entityType);

    /// <summary>
    /// Checks if the specified entity type has a registered context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>True if a context is registered for the entity type; otherwise, false.</returns>
    bool HasContext<TEntity>() where TEntity : class;

    /// <summary>
    /// Checks if the specified entity type has a registered context.
    /// </summary>
    /// <param name="entityType">The type of entity.</param>
    /// <returns>True if a context is registered for the entity type; otherwise, false.</returns>
    bool HasContext(Type entityType);
}