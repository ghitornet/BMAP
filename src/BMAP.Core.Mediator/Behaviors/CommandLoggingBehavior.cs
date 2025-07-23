using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Pipeline behavior that provides comprehensive logging for commands in the CQRS pattern.
///     This behavior logs command execution details, timing, and performance metrics.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
/// <remarks>
///     Initializes a new instance of the CommandLoggingBehavior class.
/// </remarks>
/// <param name="logger">The logger instance for logging command execution details.</param>
public class CommandLoggingBehavior<TCommand>(ILogger<CommandLoggingBehavior<TCommand>> logger) 
    : IPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    private readonly ILogger<CommandLoggingBehavior<TCommand>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task HandleAsync(TCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken = default)
    {
        var commandType = typeof(TCommand);
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Executing command {CommandType} at {Timestamp}", 
            commandType.Name, DateTimeOffset.UtcNow);
        
        _logger.LogDebug("Command {CommandType} data: {@Command}", commandType.Name, request);

        try
        {
            await next().ConfigureAwait(false);
            
            stopwatch.Stop();
            _logger.LogInformation("Command {CommandType} executed successfully in {ElapsedMilliseconds}ms", 
                commandType.Name, stopwatch.ElapsedMilliseconds);
                
            // Log performance warning for slow commands
            if (stopwatch.ElapsedMilliseconds > 5000) // 5 seconds threshold
            {
                _logger.LogWarning("Command {CommandType} execution took {ElapsedMilliseconds}ms which exceeds the recommended threshold", 
                    commandType.Name, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Command {CommandType} failed after {ElapsedMilliseconds}ms with error: {ErrorMessage}", 
                commandType.Name, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}

/// <summary>
///     Pipeline behavior that provides comprehensive logging for commands with response in the CQRS pattern.
///     This behavior logs command execution details, timing, and performance metrics.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the command.</typeparam>
/// <remarks>
///     Initializes a new instance of the CommandLoggingBehavior class.
/// </remarks>
/// <param name="logger">The logger instance for logging command execution details.</param>
public class CommandLoggingBehavior<TCommand, TResponse>(ILogger<CommandLoggingBehavior<TCommand, TResponse>> logger) 
    : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly ILogger<CommandLoggingBehavior<TCommand, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(TCommand request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var commandType = typeof(TCommand);
        var responseType = typeof(TResponse);
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Executing command {CommandType} expecting response {ResponseType} at {Timestamp}", 
            commandType.Name, responseType.Name, DateTimeOffset.UtcNow);
        
        _logger.LogDebug("Command {CommandType} data: {@Command}", commandType.Name, request);

        try
        {
            var response = await next().ConfigureAwait(false);
            
            stopwatch.Stop();
            _logger.LogInformation("Command {CommandType} executed successfully in {ElapsedMilliseconds}ms with response type {ResponseType}", 
                commandType.Name, stopwatch.ElapsedMilliseconds, responseType.Name);
                
            _logger.LogDebug("Command {CommandType} response: {@Response}", commandType.Name, response);
                
            // Log performance warning for slow commands
            if (stopwatch.ElapsedMilliseconds > 5000) // 5 seconds threshold
            {
                _logger.LogWarning("Command {CommandType} execution took {ElapsedMilliseconds}ms which exceeds the recommended threshold", 
                    commandType.Name, stopwatch.ElapsedMilliseconds);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Command {CommandType} failed after {ElapsedMilliseconds}ms with error: {ErrorMessage}", 
                commandType.Name, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}