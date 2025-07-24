namespace BMAP.Core.Data.EntityFramework.Attributes;

/// <summary>
/// Specifies which DbContext(s) an entity belongs to.
/// This attribute is used to automatically configure entities in their appropriate contexts.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ContextAttribute : Attribute
{
    /// <summary>
    /// Gets the names of the DbContext types that this entity belongs to.
    /// </summary>
    public string[] ContextNames { get; }

    /// <summary>
    /// Initializes a new instance of the ContextAttribute with a single context name.
    /// </summary>
    /// <param name="contextName">The name of the DbContext type.</param>
    public ContextAttribute(string contextName)
    {
        ContextNames = new[] { contextName ?? throw new ArgumentNullException(nameof(contextName)) };
    }

    /// <summary>
    /// Initializes a new instance of the ContextAttribute with multiple context names.
    /// </summary>
    /// <param name="contextNames">The names of the DbContext types.</param>
    public ContextAttribute(params string[] contextNames)
    {
        if (contextNames == null || contextNames.Length == 0)
            throw new ArgumentException("At least one context name must be provided", nameof(contextNames));

        ContextNames = contextNames;
    }

    /// <summary>
    /// Checks if this entity belongs to the specified context type.
    /// </summary>
    /// <param name="contextType">The type of the DbContext to check.</param>
    /// <returns>True if the entity belongs to the specified context; otherwise, false.</returns>
    public bool BelongsToContext(Type contextType)
    {
        ArgumentNullException.ThrowIfNull(contextType);
        return ContextNames.Contains(contextType.Name);
    }

    /// <summary>
    /// Checks if this entity belongs to the specified context name.
    /// </summary>
    /// <param name="contextName">The name of the DbContext to check.</param>
    /// <returns>True if the entity belongs to the specified context; otherwise, false.</returns>
    public bool BelongsToContext(string contextName)
    {
        if (string.IsNullOrWhiteSpace(contextName))
            return false;

        return ContextNames.Contains(contextName);
    }
}