using System.Reflection;
using System.Text;
using BMAP.Core.Data.Dapper.Attributes;
using BMAP.Core.Data.Entities;

namespace BMAP.Core.Data.Dapper.Helpers;

/// <summary>
/// Provides SQL generation utilities for entity operations.
/// This class uses reflection to analyze entity properties and attributes to generate appropriate SQL statements.
/// </summary>
public static class SqlGenerator
{
    /// <summary>
    /// Generates a SELECT SQL statement for retrieving an entity by its ID.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="whereClause">Optional additional WHERE clause conditions.</param>
    /// <returns>The generated SQL SELECT statement.</returns>
    public static string GenerateSelectById<TEntity>(string? whereClause = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var columns = GetColumnNames(entityType).ToList();
        var primaryKeyColumn = GetPrimaryKeyColumn(entityType);

        var sql = new StringBuilder();
        sql.AppendLine($"SELECT {string.Join(", ", columns)}");
        sql.AppendLine($"FROM {tableName}");
        sql.AppendLine($"WHERE {primaryKeyColumn} = @Id");

        if (!string.IsNullOrEmpty(whereClause))
        {
            sql.AppendLine($"AND ({whereClause})");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Generates a SELECT SQL statement for retrieving all entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="includeDeleted">Whether to include soft-deleted entities.</param>
    /// <param name="whereClause">Optional additional WHERE clause conditions.</param>
    /// <returns>The generated SQL SELECT statement.</returns>
    public static string GenerateSelectAll<TEntity>(bool includeDeleted = false, string? whereClause = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var columns = GetColumnNames(entityType).ToList();

        var sql = new StringBuilder();
        sql.AppendLine($"SELECT {string.Join(", ", columns)}");
        sql.AppendLine($"FROM {tableName}");

        var conditions = new List<string>();

        if (!includeDeleted && typeof(ISoftDeletable).IsAssignableFrom(entityType))
        {
            var isDeletedColumn = GetColumnName(entityType.GetProperty(nameof(ISoftDeletable.IsDeleted))!);
            conditions.Add($"{isDeletedColumn} = 0");
        }

        if (!string.IsNullOrEmpty(whereClause))
        {
            conditions.Add($"({whereClause})");
        }

        if (conditions.Count > 0)
        {
            sql.AppendLine($"WHERE {string.Join(" AND ", conditions)}");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Generates a SELECT SQL statement for paginated results.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="includeDeleted">Whether to include soft-deleted entities.</param>
    /// <param name="whereClause">Optional additional WHERE clause conditions.</param>
    /// <param name="orderBy">The ORDER BY clause (required for pagination).</param>
    /// <returns>The generated SQL SELECT statement with pagination.</returns>
    public static string GenerateSelectPaged<TEntity>(bool includeDeleted = false, string? whereClause = null, string? orderBy = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var columns = GetColumnNames(entityType).ToList();
        var primaryKeyColumn = GetPrimaryKeyColumn(entityType);

        var sql = new StringBuilder();
        sql.AppendLine($"SELECT {string.Join(", ", columns)}");
        sql.AppendLine($"FROM {tableName}");

        var conditions = new List<string>();

        if (!includeDeleted && typeof(ISoftDeletable).IsAssignableFrom(entityType))
        {
            var isDeletedColumn = GetColumnName(entityType.GetProperty(nameof(ISoftDeletable.IsDeleted))!);
            conditions.Add($"{isDeletedColumn} = 0");
        }

        if (!string.IsNullOrEmpty(whereClause))
        {
            conditions.Add($"({whereClause})");
        }

        if (conditions.Count > 0)
        {
            sql.AppendLine($"WHERE {string.Join(" AND ", conditions)}");
        }

        sql.AppendLine($"ORDER BY {orderBy ?? primaryKeyColumn}");
        
        // Use SQLite-compatible LIMIT and OFFSET syntax
        sql.AppendLine("LIMIT @PageSize OFFSET @Offset");

        return sql.ToString();
    }

    /// <summary>
    /// Generates a COUNT SQL statement for total records.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <param name="includeDeleted">Whether to include soft-deleted entities.</param>
    /// <param name="whereClause">Optional additional WHERE clause conditions.</param>
    /// <returns>The generated SQL COUNT statement.</returns>
    public static string GenerateCount<TEntity>(bool includeDeleted = false, string? whereClause = null)
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);

        var sql = new StringBuilder();
        sql.AppendLine("SELECT COUNT(*)");
        sql.AppendLine($"FROM {tableName}");

        var conditions = new List<string>();

        if (!includeDeleted && typeof(ISoftDeletable).IsAssignableFrom(entityType))
        {
            var isDeletedColumn = GetColumnName(entityType.GetProperty(nameof(ISoftDeletable.IsDeleted))!);
            conditions.Add($"{isDeletedColumn} = 0");
        }

        if (!string.IsNullOrEmpty(whereClause))
        {
            conditions.Add($"({whereClause})");
        }

        if (conditions.Count > 0)
        {
            sql.AppendLine($"WHERE {string.Join(" AND ", conditions)}");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Generates an INSERT SQL statement for creating a new entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The generated SQL INSERT statement.</returns>
    public static string GenerateInsert<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var insertColumns = GetInsertColumns(entityType).ToList();
        var parameters = insertColumns.Select(col => $"@{GetPropertyNameFromColumn(entityType, col)}").ToList();

        var sql = new StringBuilder();
        sql.AppendLine($"INSERT INTO {tableName}");
        sql.AppendLine($"({string.Join(", ", insertColumns)})");
        sql.AppendLine($"VALUES ({string.Join(", ", parameters)});");

        // Add identity retrieval if applicable
        if (HasIdentityColumn(entityType))
        {
            // Use SQLite's last_insert_rowid() function for identity retrieval
            sql.AppendLine("SELECT last_insert_rowid();");
        }

        return sql.ToString();
    }

    /// <summary>
    /// Generates an UPDATE SQL statement for modifying an existing entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The generated SQL UPDATE statement.</returns>
    public static string GenerateUpdate<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var updateColumns = GetUpdateColumns(entityType).ToList();
        var primaryKeyColumn = GetPrimaryKeyColumn(entityType);

        var setClause = updateColumns.Select(col =>
            $"{col} = @{GetPropertyNameFromColumn(entityType, col)}").ToList();

        var sql = new StringBuilder();
        sql.AppendLine($"UPDATE {tableName}");
        sql.AppendLine($"SET {string.Join(", ", setClause)}");
        sql.AppendLine($"WHERE {primaryKeyColumn} = @Id");

        return sql.ToString();
    }

    /// <summary>
    /// Generates a DELETE SQL statement for removing an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The generated SQL DELETE statement.</returns>
    public static string GenerateDelete<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var primaryKeyColumn = GetPrimaryKeyColumn(entityType);

        return $"DELETE FROM {tableName} WHERE {primaryKeyColumn} = @Id";
    }

    /// <summary>
    /// Generates an UPDATE SQL statement for soft deleting an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity.</typeparam>
    /// <returns>The generated SQL UPDATE statement for soft delete.</returns>
    public static string GenerateSoftDelete<TEntity>()
        where TEntity : class
    {
        var entityType = typeof(TEntity);
        var tableName = GetTableName(entityType);
        var primaryKeyColumn = GetPrimaryKeyColumn(entityType);

        if (!typeof(ISoftDeletable).IsAssignableFrom(entityType))
        {
            throw new InvalidOperationException($"Entity {entityType.Name} does not implement ISoftDeletable");
        }

        var isDeletedColumn = GetColumnName(entityType.GetProperty(nameof(ISoftDeletable.IsDeleted))!);
        var deletedAtColumn = GetColumnName(entityType.GetProperty(nameof(ISoftDeletable.DeletedAt))!);
        var deletedByColumn = GetColumnName(entityType.GetProperty(nameof(ISoftDeletable.DeletedBy))!);

        var sql = new StringBuilder();
        sql.AppendLine($"UPDATE {tableName}");
        sql.AppendLine($"SET {isDeletedColumn} = 1,");
        sql.AppendLine($"    {deletedAtColumn} = @DeletedAt,");
        sql.AppendLine($"    {deletedByColumn} = @DeletedBy");
        sql.AppendLine($"WHERE {primaryKeyColumn} = @Id");

        return sql.ToString();
    }

    private static string GetTableName(Type entityType)
    {
        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
        return tableAttribute?.FullName ?? entityType.Name;
    }

    private static string GetPrimaryKeyColumn(Type entityType)
    {
        // First look for explicit primary key attribute
        var property = entityType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<ColumnAttribute>()?.IsPrimaryKey == true);

        if (property != null)
        {
            return GetColumnName(property);
        }

        // Fall back to Id property
        var idProperty = entityType.GetProperty("Id");
        if (idProperty != null)
        {
            return GetColumnName(idProperty);
        }

        throw new InvalidOperationException($"No primary key found for entity {entityType.Name}");
    }

    private static string GetColumnName(PropertyInfo property)
    {
        var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
        return columnAttribute?.Name ?? property.Name;
    }

    private static IEnumerable<string> GetColumnNames(Type entityType)
    {
        return entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null)
            .Select(GetColumnName);
    }

    private static IEnumerable<string> GetInsertColumns(Type entityType)
    {
        return entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null)
            .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.IgnoreOnInsert != true)
            .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.IsIdentity != true)
            .Select(GetColumnName);
    }

    private static IEnumerable<string> GetUpdateColumns(Type entityType)
    {
        var primaryKeyProperty = entityType.GetProperties()
            .FirstOrDefault(p => p.GetCustomAttribute<ColumnAttribute>()?.IsPrimaryKey == true);
        
        if (primaryKeyProperty == null)
        {
            primaryKeyProperty = entityType.GetProperty("Id");
        }

        return entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<IgnoreAttribute>() == null)
            .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.IgnoreOnUpdate != true)
            .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.IsPrimaryKey != true)
            .Where(p => p.GetCustomAttribute<ColumnAttribute>()?.IsIdentity != true)
            .Where(p => primaryKeyProperty == null || p.Name != primaryKeyProperty.Name) // Explicitly exclude primary key
            .Select(GetColumnName);
    }

    private static bool HasIdentityColumn(Type entityType)
    {
        return entityType.GetProperties()
            .Any(p => p.GetCustomAttribute<ColumnAttribute>()?.IsIdentity == true);
    }

    private static string GetPropertyNameFromColumn(Type entityType, string columnName)
    {
        var property = entityType.GetProperties()
            .FirstOrDefault(p => GetColumnName(p) == columnName);

        return property?.Name ?? columnName;
    }
}