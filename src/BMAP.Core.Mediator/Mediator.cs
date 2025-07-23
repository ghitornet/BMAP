using System.Reflection;
using BMAP.Core.Mediator.Exceptions;

namespace BMAP.Core.Mediator;

/// <summary>
///     Default implementation of the IMediator interface.
///     Provides request/response pattern and publish-subscribe pattern capabilities.
/// </summary>
/// <remarks>
///     Initializes a new instance of the Mediator class.
/// </remarks>
/// <param name="serviceLocator">The service locator used for resolving handlers.</param>
/// <exception cref="ArgumentNullException">Thrown when serviceLocator is null.</exception>
public class Mediator(IServiceLocator serviceLocator) : IMediator
{
    private readonly IServiceLocator _serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));

    /// <inheritdoc />
    public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerType = typeof(IRequestHandler<>).MakeGenericType(typeof(TRequest));
        var handler = _serviceLocator.GetServiceOrDefault(handlerType) ?? throw new HandlerNotFoundException(typeof(TRequest));
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<TRequest>.HandleAsync)) ?? throw new MediatorException(
                $"HandleAsync method not found on handler for request type '{typeof(TRequest).Name}'.");
        try
        {
            var task = (Task)handleMethod.Invoke(handler, [request, cancellationToken])!;
            await task.ConfigureAwait(false);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException) throw ex.InnerException;
            throw new MediatorException($"Error occurred while handling request of type '{typeof(TRequest).Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new MediatorException($"Error occurred while handling request of type '{typeof(TRequest).Name}'.",
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceLocator.GetServiceOrDefault(handlerType) ?? throw new HandlerNotFoundException(requestType);
        var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync)) ?? throw new MediatorException(
                $"HandleAsync method not found on handler for request type '{requestType.Name}'.");
        try
        {
            var task = (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
            return await task.ConfigureAwait(false);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException) throw ex.InnerException;
            throw new MediatorException($"Error occurred while handling request of type '{requestType.Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new MediatorException($"Error occurred while handling request of type '{requestType.Name}'.", ex);
        }
    }

    /// <inheritdoc />
    public Task PublishAsync<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlerType = typeof(INotificationHandler<>).MakeGenericType(typeof(TNotification));
        var handlers = _serviceLocator.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
            // No handlers found - this is acceptable for notifications
            return Task.CompletedTask;

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<TNotification>.HandleAsync)) ?? throw new MediatorException(
                $"HandleAsync method not found on notification handler for notification type '{typeof(TNotification).Name}'.");

        // Execute all handlers in parallel (fire and forget)
        var tasks = handlers.Select(handler =>
        {
            try
            {
                var task = (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
                return task;
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to avoid stopping other handlers
                // In a real implementation, you might want to use a logger here
                return Task.FromException(ex);
            }
        });

        // Start all tasks but don't wait for completion
        _ = Task.WhenAll(tasks).ContinueWith(t =>
        {
            if (t.IsFaulted && t.Exception != null)
            {
                // Handle exceptions from notification handlers
                // In a real implementation, you might want to log these
                var exceptions = t.Exception.InnerExceptions;
                foreach (var ex in exceptions)
                {
                    // Log exception
                }
            }
        }, TaskContinuationOptions.OnlyOnFaulted);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task PublishAndWaitAsync<TNotification>(TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        var handlerType = typeof(INotificationHandler<>).MakeGenericType(typeof(TNotification));
        var handlers = _serviceLocator.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
            // No handlers found - this is acceptable for notifications
            return;

        var handleMethod = handlerType.GetMethod(nameof(INotificationHandler<TNotification>.HandleAsync)) ?? throw new MediatorException(
                $"HandleAsync method not found on notification handler for notification type '{typeof(TNotification).Name}'.");
        try
        {
            var tasks = handlers.Select(handler =>
            {
                var task = (Task)handleMethod.Invoke(handler, [notification, cancellationToken])!;
                return task;
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            if (ex.InnerException is OperationCanceledException) throw ex.InnerException;
            throw new MediatorException(
                $"Error occurred while publishing notification of type '{typeof(TNotification).Name}'.",
                ex.InnerException);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new MediatorException(
                $"Error occurred while publishing notification of type '{typeof(TNotification).Name}'.", ex);
        }
    }
}