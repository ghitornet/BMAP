using BMAP.Core.Data.Queries;

namespace BMAP.Core.Data.Requests;

/// <summary>
/// Generic query for retrieving an entity by its identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public class GetEntityByIdQuery<TEntity, TId> : IGetByIdQuery<TEntity, TId>
    where TEntity : class
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Initializes a new instance of the GetEntityByIdQuery class.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    public GetEntityByIdQuery(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Gets or sets the identifier of the entity to retrieve.
    /// </summary>
    public TId Id { get; set; }
}

/// <summary>
/// Generic query for retrieving an entity by its integer identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetEntityByIdQuery<TEntity> : GetEntityByIdQuery<TEntity, int>, IGetByIdQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the GetEntityByIdQuery class.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    public GetEntityByIdQuery(int id) : base(id)
    {
    }
}

/// <summary>
/// Generic query for retrieving all entities of a specific type.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetAllEntitiesQuery<TEntity> : IGetAllQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the GetAllEntitiesQuery class.
    /// </summary>
    /// <param name="includeDeleted">A value indicating whether to include soft deleted entities.</param>
    public GetAllEntitiesQuery(bool includeDeleted = false)
    {
        IncludeDeleted = includeDeleted;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to include soft deleted entities.
    /// Default is false, which excludes soft deleted entities.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}

/// <summary>
/// Generic query for retrieving entities with pagination.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public class GetEntitiesPagedQuery<TEntity> : IPagedQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Initializes a new instance of the GetEntitiesPagedQuery class.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="includeDeleted">A value indicating whether to include soft deleted entities.</param>
    public GetEntitiesPagedQuery(int pageNumber = 1, int pageSize = 10, bool includeDeleted = false)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 ? pageSize : 10;
        IncludeDeleted = includeDeleted;
    }

    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include soft deleted entities.
    /// Default is false, which excludes soft deleted entities.
    /// </summary>
    public bool IncludeDeleted { get; set; }
}