using System.Diagnostics;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    ///     Initializes a new instance of the LoggingBehavior class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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

        _logger.LogInformation("Starting execution of request {RequestName} with data: {@Request}", requestName, request);

        try
        {
            var response = await next();
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully completed request {RequestName} in {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error occurred while handling request {RequestName} after {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
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
    private readonly ILogger<LoggingBehavior<TRequest>> _logger;

    /// <summary>
    ///     Initializes a new instance of the LoggingBehavior class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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

        _logger.LogInformation("Starting execution of request {RequestName} with data: {@Request}", requestName, request);

        try
        {
            await next();
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully completed request {RequestName} in {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error occurred while handling request {RequestName} after {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}