namespace BMAP.Core.Data.Dapper.Attributes;

/// <summary>
/// Specifies the database column name for a property.
/// This attribute is used by the Dapper implementation to map properties to database columns.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the ColumnAttribute class.
    /// </summary>
    /// <param name="name">The name of the database column.</param>
    public ColumnAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the name of the database column.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is an identity column (auto-incremented).
    /// </summary>
    public bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column should be ignored in insert operations.
    /// This is useful for computed columns or columns with default values.
    /// </summary>
    public bool IgnoreOnInsert { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column should be ignored in update operations.
    /// This is useful for identity columns or audit columns that should not be manually updated.
    /// </summary>
    public bool IgnoreOnUpdate { get; set; }
}