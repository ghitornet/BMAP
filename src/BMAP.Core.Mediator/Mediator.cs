using System.Reflection;
using BMAP.Core.Mediator.Exceptions;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator;

/// <summary>
///     Default implementation of the IMediator interface.
///     Provides request/response pattern and publish-subscribe pattern capabilities.
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
        _logger.LogDebug("Sending request of type {RequestType}", requestType.Name);

        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        var handler = _serviceLocator.GetServiceOrDefault(handlerType);
        
        if (handler == null)
        {
            _logger.LogError("No handler found for request type {RequestType}", requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }

        _logger.LogDebug("Found handler of type {HandlerType} for request {RequestType}", 
            handler.GetType().Name, requestType.Name);

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<TRequest>.HandleAsync));
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found on handler for request type {RequestType}", requestType.Name);
            throw new MediatorException($"HandleAsync method not found on handler for request type '{requestType.Name}'.");
        }

        try
        {
            _logger.LogDebug("Invoking handler for request type {RequestType}", requestType.Name);
            var task = (Task)handleMethod.Invoke(handler, [request, cancellationToken])!;
            await task.ConfigureAwait(false);
            _logger.LogDebug("Successfully completed request of type {RequestType}", requestType.Name);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException)
            {
                _logger.LogDebug("Request of type {RequestType} was cancelled", requestType.Name);
                throw ex.InnerException;
            }
            
            _logger.LogError(ex.InnerException, "Error occurred while handling request of type {RequestType}", requestType.Name);
            throw new MediatorException($"Error occurred while handling request of type '{requestType.Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while handling request of type {RequestType}", requestType.Name);
            throw new MediatorException($"Error occurred while handling request of type '{requestType.Name}'.",
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        _logger.LogDebug("Sending request of type {RequestType} expecting response of type {ResponseType}", 
            requestType.Name, typeof(TResponse).Name);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceLocator.GetServiceOrDefault(handlerType);
        
        if (handler == null)
        {
            _logger.LogError("No handler found for request type {RequestType}", requestType.Name);
            throw new HandlerNotFoundException(requestType);
        }

        _logger.LogDebug("Found handler of type {HandlerType} for request {RequestType}", 
            handler.GetType().Name, requestType.Name);

        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync));
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found on handler for request type {RequestType}", requestType.Name);
            throw new MediatorException($"HandleAsync method not found on handler for request type '{requestType.Name}'.");
        }

        try
        {
            _logger.LogDebug("Invoking handler for request type {RequestType}", requestType.Name);
            var task = (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
            var result = await task.ConfigureAwait(false);
            _logger.LogDebug("Successfully completed request of type {RequestType} with response", requestType.Name);
            return result;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException)
            {
                _logger.LogDebug("Request of type {RequestType} was cancelled", requestType.Name);
                throw ex.InnerException;
            }
            
            _logger.LogError(ex.InnerException, "Error occurred while handling request of type {RequestType}", requestType.Name);
            throw new MediatorException($"Error occurred while handling request of type '{requestType.Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while handling request of type {RequestType}", requestType.Name);
            throw new MediatorException($"Error occurred while handling request of type '{requestType.Name}'.", ex);
        }
    }

    /// <inheritdoc />
    public Task PublishAsync<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var notificationType = typeof(TNotification);
        _logger.LogDebug("Publishing notification of type {NotificationType}", notificationType.Name);

        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceLocator.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            _logger.LogDebug("No handlers found for notification type {NotificationType}", notificationType.Name);
            return Task.CompletedTask;
        }

        _logger.LogDebug("Found {HandlerCount} handlers for notification type {NotificationType}", 
            handlers.Count, notificationType.Name);

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<TNotification>.HandleAsync));
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found on notification handler for notification type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"HandleAsync method not found on notification handler for notification type '{notificationType.Name}'.");
        }

        // Execute all handlers in parallel (fire and forget)
        var tasks = handlers.Where(h => h != null).Select(handler =>
        {
            try
            {
                _logger.LogDebug("Invoking notification handler {HandlerType} for notification {NotificationType}", 
                    handler!.GetType().Name, notificationType.Name);
                var task = (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invoking notification handler {HandlerType} for notification {NotificationType}", 
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
                    _logger.LogError(ex, "Error occurred in notification handler for notification type {NotificationType}", 
                        notificationType.Name);
                }
            }
            else
            {
                _logger.LogDebug("All notification handlers completed for notification type {NotificationType}", 
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
        _logger.LogDebug("Publishing and waiting for notification of type {NotificationType}", notificationType.Name);

        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = _serviceLocator.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
        {
            _logger.LogDebug("No handlers found for notification type {NotificationType}", notificationType.Name);
            return;
        }

        _logger.LogDebug("Found {HandlerCount} handlers for notification type {NotificationType}", 
            handlers.Count, notificationType.Name);

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<TNotification>.HandleAsync));
        if (handleMethod == null)
        {
            _logger.LogError("HandleAsync method not found on notification handler for notification type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"HandleAsync method not found on notification handler for notification type '{notificationType.Name}'.");
        }

        try
        {
            var tasks = handlers.Where(h => h != null).Select(handler =>
            {
                _logger.LogDebug("Invoking notification handler {HandlerType} for notification {NotificationType}", 
                    handler!.GetType().Name, notificationType.Name);
                var task = (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
                return task;
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            _logger.LogDebug("All notification handlers completed successfully for notification type {NotificationType}", 
                notificationType.Name);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException)
            {
                _logger.LogDebug("Notification publishing was cancelled for notification type {NotificationType}", 
                    notificationType.Name);
                throw ex.InnerException;
            }
            
            _logger.LogError(ex.InnerException, "Error occurred while publishing notification of type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"Error occurred while publishing notification of type '{notificationType.Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unexpected error occurred while publishing notification of type {NotificationType}", 
                notificationType.Name);
            throw new MediatorException(
                $"Error occurred while publishing notification of type '{notificationType.Name}'.", ex);
        }
    }
}