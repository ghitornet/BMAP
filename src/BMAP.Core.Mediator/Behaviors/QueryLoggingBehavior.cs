using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Pipeline behavior that provides comprehensive logging for queries in the CQRS pattern.
///     This behavior logs query execution details, timing, caching opportunities, and performance metrics.
/// </summary>
/// <typeparam name="TQuery">The type of query being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the query.</typeparam>
/// <remarks>
///     Initializes a new instance of the QueryLoggingBehavior class.
/// </remarks>
/// <param name="logger">The logger instance for logging query execution details.</param>
public class QueryLoggingBehavior<TQuery, TResponse>(ILogger<QueryLoggingBehavior<TQuery, TResponse>> logger) 
    : IPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly ILogger<QueryLoggingBehavior<TQuery, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(TQuery request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var queryType = typeof(TQuery);
        var responseType = typeof(TResponse);
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Executing query {QueryType} expecting response {ResponseType} at {Timestamp}", 
            queryType.Name, responseType.Name, DateTimeOffset.UtcNow);
        
        _logger.LogDebug("Query {QueryType} parameters: {@Query}", queryType.Name, request);

        try
        {
            var response = await next().ConfigureAwait(false);
            
            stopwatch.Stop();
            _logger.LogInformation("Query {QueryType} executed successfully in {ElapsedMilliseconds}ms", 
                queryType.Name, stopwatch.ElapsedMilliseconds);
                
            // Log caching opportunity for frequently accessed data
            if (stopwatch.ElapsedMilliseconds > 1000) // 1 second threshold for caching consideration
            {
                _logger.LogInformation("Query {QueryType} took {ElapsedMilliseconds}ms - consider caching for performance optimization", 
                    queryType.Name, stopwatch.ElapsedMilliseconds);
            }
            
            // Log performance warning for slow queries
            if (stopwatch.ElapsedMilliseconds > 10000) // 10 seconds threshold
            {
                _logger.LogWarning("Query {QueryType} execution took {ElapsedMilliseconds}ms which exceeds the recommended threshold", 
                    queryType.Name, stopwatch.ElapsedMilliseconds);
            }
            
            // Log response size for large datasets (if response is a collection)
            if (response is System.Collections.ICollection collection)
            {
                _logger.LogDebug("Query {QueryType} returned {ItemCount} items", queryType.Name, collection.Count);
                
                if (collection.Count > 1000)
                {
                    _logger.LogWarning("Query {QueryType} returned {ItemCount} items - consider pagination for large datasets", 
                        queryType.Name, collection.Count);
                }
            }
            
            _logger.LogTrace("Query {QueryType} response: {@Response}", queryType.Name, response);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Query {QueryType} failed after {ElapsedMilliseconds}ms with error: {ErrorMessage}", 
                queryType.Name, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}