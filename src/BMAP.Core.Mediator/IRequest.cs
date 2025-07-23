namespace BMAP.Core.Mediator;

/// <summary>
///     Represents a request that can be processed by the mediator.
///     This is a marker interface for requests that don't return a response.
/// </summary>
public interface IRequest;

/// <summary>
///     Represents a request that can be processed by the mediator and returns a response.
/// </summary>
/// <typeparam name="TResponse">The type of response that this request will return.</typeparam>
public interface IRequest<out TResponse> : IRequest;