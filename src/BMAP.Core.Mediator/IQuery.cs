namespace BMAP.Core.Mediator;

/// <summary>
///     Represents a query in the CQRS pattern.
///     Queries are read-only operations that retrieve data without modifying the system state.
/// </summary>
/// <typeparam name="TResponse">The type of data that this query will return.</typeparam>
/// <remarks>
///     Queries in CQRS are responsible for data retrieval and should never modify system state.
///     They should be named with nouns or question-like phrases (e.g., GetUser, FindProducts, UserExists).
///     Queries should be side-effect free and can be cached, logged, or replicated safely.
/// </remarks>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}