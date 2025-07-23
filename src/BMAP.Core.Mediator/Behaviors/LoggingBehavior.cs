using System.Diagnostics;

namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Pipeline behavior that logs the execution time of requests.
///     This behavior measures how long each request takes to process.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     Handles the request and logs execution time.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the response.</returns>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Console.WriteLine($"[MEDIATOR] Handling {requestName}");
            var response = await next();
            stopwatch.Stop();
            Console.WriteLine($"[MEDIATOR] Handled {requestName} in {stopwatch.ElapsedMilliseconds}ms");
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine(
                $"[MEDIATOR] Error handling {requestName} after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
///     Pipeline behavior that logs the execution time of requests without response.
///     This behavior measures how long each request takes to process.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
public class LoggingBehavior<TRequest> : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    ///     Handles the request and logs execution time.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Console.WriteLine($"[MEDIATOR] Handling {requestName}");
            await next();
            stopwatch.Stop();
            Console.WriteLine($"[MEDIATOR] Handled {requestName} in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine(
                $"[MEDIATOR] Error handling {requestName} after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw;
        }
    }
}