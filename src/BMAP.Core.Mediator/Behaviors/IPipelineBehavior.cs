namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Defines a pipeline behavior for requests without response.
///     Pipeline behaviors are executed in order before the actual request handler.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
public interface IPipelineBehavior<in TRequest> where TRequest : IRequest
{
    /// <summary>
    ///     Handles the request and can modify it before passing to the next behavior or handler.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken = default);
}

/// <summary>
///     Defines a pipeline behavior for requests with response.
///     Pipeline behaviors are executed in order before the actual request handler.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     Handles the request and can modify it or the response.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the response.</returns>
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}

/// <summary>
///     Represents a delegate for the next handler in the request pipeline.
/// </summary>
/// <returns>A task that represents the asynchronous operation.</returns>
public delegate Task RequestHandlerDelegate();

/// <summary>
///     Represents a delegate for the next handler in the request pipeline with response.
/// </summary>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <returns>A task that represents the asynchronous operation and contains the response.</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();