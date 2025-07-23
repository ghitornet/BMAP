namespace BMAP.Core.Mediator;

/// <summary>
///     Defines a handler for a query in the CQRS pattern.
///     Query handlers encapsulate the logic for retrieving data without modifying system state.
/// </summary>
/// <typeparam name="TQuery">The type of query being handled.</typeparam>
/// <typeparam name="TResponse">The type of data returned by the query.</typeparam>
/// <remarks>
///     Query handlers should:
///     - Be read-only and side-effect free
///     - Access optimized read stores (could be different from write stores)
///     - Handle data projection and transformation
///     - Be cacheable and safe to retry
///     - Focus on a single query type for better separation of concerns
///     - Not modify any system state or trigger side effects
/// </remarks>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}