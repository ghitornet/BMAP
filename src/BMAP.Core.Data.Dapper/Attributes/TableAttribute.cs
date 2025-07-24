namespace BMAP.Core.Data.Dapper.Attributes;

/// <summary>
/// Specifies the database table name for an entity.
/// This attribute is used by the Dapper implementation to determine the table name for CRUD operations.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the TableAttribute class.
    /// </summary>
    /// <param name="name">The name of the database table.</param>
    public TableAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the name of the database table.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the schema name for the table.
    /// If not specified, the default schema will be used.
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets the full table name including schema if specified.
    /// </summary>
    public string FullName => string.IsNullOrEmpty(Schema) ? Name : $"{Schema}.{Name}";
}