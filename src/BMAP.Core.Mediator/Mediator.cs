using System.Reflection;
using BMAP.Core.Mediator.Behaviors;
using BMAP.Core.Mediator.Exceptions;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator;

/// <summary>
///     Default implementation of the IMediator interface.
///     Provides request/response pattern, publish-subscribe pattern, CQRS capabilities, and pipeline behavior support.
/// </summary>
/// <remarks>
///     Initializes a new instance of the Mediator class.
/// </remarks>
/// <param name="serviceLocator">The service locator used for resolving handlers.</param>
/// <param name="logger">The logger instance.</param>
/// <exception cref="ArgumentNullException">Thrown when serviceLocator or logger is null.</exception>
public class Mediator(IServiceLocator serviceLocator, ILogger<Mediator> logger) : IMediator
{
    private readonly IServiceLocator _serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
    private readonly ILogger<Mediator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = typeof(TRequest);
        var cqrsType = GetCqrsTypeDescription(requestType);
        
        _logger.LogDebug("Sending {CqrsType} of type {RequestType}", cqrsType, requestType.Name);

        try
        {
            _logger.LogDebug("Building pipeline for {CqrsType} {RequestType}", cqrsType, requestType.Name);
            
            // Build and execute pipeline with behaviors
            await ExecutePipeline(request, requestType, cqrsType, cancellationToken).ConfigureAwait(false);
            
            _logger.LogInformation("Successfully executed {CqrsType} {RequestType}", cqrsType, requestType.Name);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("{CqrsType} {RequestType} was cancelled", cqrsType, requestType.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing {CqrsType} {RequestType}", cqrsType, requestType.Name);
            if (ex is MediatorException or HandlerNotFoundException or Behaviors.ValidationException)
                throw;
            
            throw new MediatorException($"Error occurred while executing {cqrsType} '{requestType.Name}'.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var cqrsType = GetCqrsTypeDescription(requestType);
        
        _logger.LogDebug("Sending {CqrsType} of type {RequestType} expecting response of type {ResponseType}", 
            cqrsType, requestType.Name, typeof(TResponse).Name);

        try
        {
            _logger.LogDebug("Building pipeline for {CqrsType} {RequestType}", cqrsType, requestType.Name);
            
            // Build and execute pipeline with behaviors
            var result = await ExecutePipelineWithResponse<TResponse>(request, requestType, cqrsType, cancellationToken).ConfigureAwait(false);
            
            _logger.LogInformation("Successfully executed {CqrsType} {RequestType} with response of type {ResponseType}", 
                cqrsType, requestType.Name, typeof(TResponse).Name);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("{CqrsType} {RequestType} was cancelled", cqrsType, requestType.Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing {CqrsType} {RequestType}", cqrsType, requestType.Name);
            if (ex is MediatorException or HandlerNotFoundException or Behaviors.ValidationException)
                throw;
            
            throw new MediatorException($"Error occurred while executing {cqrsType} '{requestType.Name}'.", ex);
        }
    }

    /// <summary>
    ///     Executes the pipeline for requests without response.
    /// </summary>
    private async Task ExecutePipeline<TRequest>(TRequest request, Type requestType, string cqrsType, CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(requestType);
        var behaviors = _serviceLocator.GetServices(behaviorType).ToArray();

        _logger.LogDebug("Found {BehaviorCount} pipeline behaviors for {CqrsType} {RequestType}", 
            behaviors.Length, cqrsType, requestType.Name);

        // Get the actual request handler
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        var handler = _serviceLocator.GetServiceOrDefault(handlerType);
        
        if (handler == null)
        {
            _logger.LogError("No handler found for {CqrsType} type {RequestType}", cqrsType, requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }

        _logger.LogDebug("Found handler of type {HandlerType} for {CqrsType} {RequestType}", 
            handler.GetType().Name, cqrsType, requestType.Name);

        // Build the pipeline from the end (handler) to the beginning (first behavior)
        RequestHandlerDelegate pipeline = async () =>
        {
            var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<TRequest>.HandleAsync));
            if (handleMethod == null)
            {
                throw new MediatorException($"HandleAsync method not found on handler for {cqrsType} type '{requestType.Name}'.");
            }

            try
            {
                var task = (Task)handleMethod.Invoke(handler, [request, cancellationToken])!;
                await task.ConfigureAwait(false);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };

        // Wrap the handler with behaviors (in reverse order so they execute in registration order)
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var nextPipeline = pipeline;
            
            // Use reflection to call the behavior's HandleAsync method
            var behaviorHandleMethod = behavior?.GetType().GetMethod("HandleAsync");
            if (behaviorHandleMethod != null)
            {
                pipeline = () =>
                {
                    try
                    {
                        var task = (Task)behaviorHandleMethod.Invoke(behavior, [request, nextPipeline, cancellationToken])!;
                        return task;
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                };
            }
        }

        await pipeline().ConfigureAwait(false);
    }

    /// <summary>
    ///     Executes the pipeline for requests with response.
    /// </summary>
    private async Task<TResponse> ExecutePipelineWithResponse<TResponse>(object request, Type requestType, string cqrsType, CancellationToken cancellationToken)
    {
        // Get pipeline behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _serviceLocator.GetServices(behaviorType).ToArray();

        _logger.LogDebug("Found {BehaviorCount} pipeline behaviors for {CqrsType} {RequestType}", 
            behaviors.Length, cqrsType, requestType.Name);

        // Get the actual request handler
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceLocator.GetServiceOrDefault(handlerType);
        
        if (handler == null)
        {
            _logger.LogError("No handler found for {CqrsType} type {RequestType}", cqrsType, requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }

        _logger.LogDebug("Found handler of type {HandlerType} for {CqrsType} {RequestType}", 
            handler.GetType().Name, cqrsType, requestType.Name);

        // Build the pipeline from the end (handler) to the beginning (first behavior)
        RequestHandlerDelegate<TResponse> pipeline = async () =>
        {
            var handleMethod = handlerType.GetMethod("HandleAsync");
            if (handleMethod == null)
            {
                throw new MediatorException($"HandleAsync method not found on handler for {cqrsType} type '{requestType.Name}'.");
            }

            try
            {
                var task = (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
                return await task.ConfigureAwait(false);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };

        // Wrap the handler with behaviors (in reverse order so they execute in registration order)
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var nextPipeline = pipeline;
            
            // Use reflection to call the behavior's HandleAsync method
            var behaviorHandleMethod = behavior?.GetType().GetMethod("HandleAsync");
            if (behaviorHandleMethod != null)
            {
                pipeline = () =>
                {
                    try
                    {
                        var task = (Task<TResponse>)behaviorHandleMethod.Invoke(behavior, [request, nextPipeline, cancellationToken])!;
                        return task;
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                };
            }
        }

        return await pipeline().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task PublishAsync<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = typeof(TNotification);
        _logger.LogDebug("Publishing event/notification of type {NotificationType}", notificationType.Name);

        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceLocator.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            _logger.LogDebug("No event handlers found for notification type {NotificationType}", notificationType.Name);
            return Task.CompletedTask;
        }

        _logger.LogDebug("Found {HandlerCount} event handlers for notification type {NotificationType}", 
            handlers.Count, notificationType.Name);

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<TNotification>.HandleAsync));
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found on event handler for notification type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"HandleAsync method not found on event handler for notification type '{notificationType.Name}'.");
        }

        // Execute all handlers in parallel (fire and forget)
        var tasks = handlers.Where(h => h != null).Select(handler =>
        {
            try
            {
                _logger.LogDebug("Invoking event handler {HandlerType} for notification {NotificationType}", 
                    handler!.GetType().Name, notificationType.Name);
                var task = (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking event handler {HandlerType} for notification {NotificationType}", 
                    handler!.GetType().Name, notificationType.Name);
                return Task.FromException(ex);
            }
        });

        // Start all tasks but don't wait for completion
        _ = Task.WhenAll(tasks).ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                var exceptions = t.Exception.InnerExceptions;
                foreach (var ex in exceptions)
                {
                    _logger.LogError(ex, "Error occurred in event handler for notification type {NotificationType}", 
                        notificationType.Name);
                }
            }
            else
            {
                _logger.LogDebug("All event handlers completed for notification type {NotificationType}", 
                    notificationType.Name);
            }
        }, TaskContinuationOptions.ExecuteSynchronously);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task PublishAndWaitAsync<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = typeof(TNotification);
        _logger.LogDebug("Publishing and waiting for event/notification of type {NotificationType}", notificationType.Name);

        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceLocator.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            _logger.LogDebug("No event handlers found for notification type {NotificationType}", notificationType.Name);
            return;
        }

        _logger.LogDebug("Found {HandlerCount} event handlers for notification type {NotificationType}", 
            handlers.Count, notificationType.Name);

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<TNotification>.HandleAsync));
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found on event handler for notification type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"HandleAsync method not found on event handler for notification type '{notificationType.Name}'.");
        }

        try
        {
            var tasks = handlers.Where(h => h != null).Select(handler =>
            {
                _logger.LogDebug("Executing event handler {HandlerType} for notification {NotificationType}", 
                    handler!.GetType().Name, notificationType.Name);
                var task = (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
                return task;
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogInformation("All event handlers completed successfully for notification type {NotificationType}", 
                notificationType.Name);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException)
            {
                _logger.LogDebug("Event publishing was cancelled for notification type {NotificationType}", 
                    notificationType.Name);
                throw ex.InnerException;
            }
            
            _logger.LogError(ex.InnerException, "Error occurred while publishing event of type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"Error occurred while publishing event of type '{notificationType.Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while publishing event of type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"Error occurred while publishing event of type '{notificationType.Name}'.", ex);
        }
    }

    /// <summary>
    ///     Determines the CQRS type description for logging purposes.
    /// </summary>
    /// <param name="requestType">The type of the request.</param>
    /// <returns>A string describing the CQRS type (Command, Query, or Request).</returns>
    private static string GetCqrsTypeDescription(Type requestType)
    {
        if (typeof(ICommand).IsAssignableFrom(requestType))
        {
            return "command";
        }
        
        if (requestType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>)))
        {
            return "command";
        }
        
        if (requestType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>)))
        {
            return "query";
        }
        
        return "request";
    }
}