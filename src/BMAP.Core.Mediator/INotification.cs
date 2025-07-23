namespace BMAP.Core.Mediator;

/// <summary>
///     Represents a notification that can be published to multiple handlers.
///     This is used for the publish-subscribe pattern.
/// </summary>
public interface INotification;

/// <summary>
///     Defines a handler for a notification.
///     Multiple handlers can be registered for the same notification type.
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled.</typeparam>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    ///     Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}