using BMAP.Core.Mediator;

namespace BMAP.Core.Data.Queries;

/// <summary>
/// Base interface for all data queries that return a Result.
/// This interface extends IQuery to provide consistent result handling for data operations.
/// </summary>
/// <typeparam name="TResponse">The type of the query response.</typeparam>
public interface IDataQuery<TResponse> : IQuery<BMAP.Core.Result.Result<TResponse>>
{
}

/// <summary>
/// Interface for queries that retrieve a single entity by its identifier.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IGetByIdQuery<TEntity, TId> : IDataQuery<TEntity>
    where TEntity : class
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets or sets the identifier of the entity to retrieve.
    /// </summary>
    TId Id { get; set; }
}

/// <summary>
/// Interface for queries that retrieve a single entity by its identifier (integer).
/// This is a convenience interface for entities with integer identifiers.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public interface IGetByIdQuery<TEntity> : IGetByIdQuery<TEntity, int>
    where TEntity : class
{
}

/// <summary>
/// Interface for queries that retrieve all entities of a specific type.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public interface IGetAllQuery<TEntity> : IDataQuery<IEnumerable<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Gets or sets a value indicating whether to include soft deleted entities.
    /// Default is false, which excludes soft deleted entities.
    /// </summary>
    bool IncludeDeleted { get; set; }
}

/// <summary>
/// Interface for paginated queries.
/// </summary>
/// <typeparam name="TEntity">The type of entity to retrieve.</typeparam>
public interface IPagedQuery<TEntity> : IDataQuery<PagedResult<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    int PageSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include soft deleted entities.
    /// Default is false, which excludes soft deleted entities.
    /// </summary>
    bool IncludeDeleted { get; set; }
}

/// <summary>
/// Represents a paginated result set.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Initializes a new instance of the PagedResult class.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
    }

    /// <summary>
    /// Gets the items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets the current page number.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; }

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; }
}