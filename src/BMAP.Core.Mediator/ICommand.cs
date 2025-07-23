namespace BMAP.Core.Mediator;

/// <summary>
///     Represents a command in the CQRS pattern.
///     Commands are operations that modify the state of the system and do not return data.
///     Use this interface for commands that don't return a response.
/// </summary>
/// <remarks>
///     Commands in CQRS represent the intent to change the state of the system.
///     They should be named with imperative verbs (e.g., CreateUser, UpdateProduct, DeleteOrder).
///     Commands should not return data; use queries for data retrieval.
/// </remarks>
public interface ICommand : IRequest
{
}

/// <summary>
///     Represents a command in the CQRS pattern that returns a response.
///     Commands typically modify state and may return an identifier or confirmation data.
/// </summary>
/// <typeparam name="TResponse">The type of response that this command will return.</typeparam>
/// <remarks>
///     This interface should be used sparingly. Commands that return data blur the line
///     between commands and queries. Consider using separate command + query operations instead.
///     Common use cases include returning generated identifiers (e.g., newly created entity IDs).
/// </remarks>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}