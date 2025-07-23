namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for INotification and INotificationHandler interfaces.
/// </summary>
public class NotificationTests
{
    [Fact]
    public void INotification_Should_BeMarkerInterface()
    {
        // Arrange
        var notificationType = typeof(INotification);

        // Act
        var hasMembers = notificationType.GetMethods().Length > 0 || notificationType.GetProperties().Length > 0;

        // Assert
        Assert.False(hasMembers);
    }

    [Fact]
    public void INotificationHandler_Should_HaveHandleAsyncMethod()
    {
        // Arrange
        var handlerType = typeof(INotificationHandler<>);

        // Act
        var methods = handlerType.GetMethods();
        var handleAsyncMethod = methods.FirstOrDefault(m => m.Name == "HandleAsync");

        // Assert
        Assert.NotNull(handleAsyncMethod);
        Assert.Equal(typeof(Task), handleAsyncMethod.ReturnType);
    }

    [Fact]
    public void ConcreteNotification_Should_ImplementINotification()
    {
        // Arrange
        var concreteNotification = new TestNotification();

        // Act
        var isNotification = concreteNotification is INotification;

        // Assert
        Assert.True(isNotification);
    }

    [Fact]
    public void ConcreteNotificationHandler_Should_ImplementINotificationHandler()
    {
        // Arrange
        var concreteHandler = new TestNotificationHandler();

        // Act
        var isHandler = concreteHandler is INotificationHandler<TestNotification>;

        // Assert
        Assert.True(isHandler);
    }

    [Fact]
    public async Task NotificationHandler_HandleAsync_Should_BeCallable()
    {
        // Arrange
        var handler = new TestNotificationHandler();
        var notification = new TestNotification();

        // Act & Assert
        await handler.HandleAsync(notification);
        // If we reach here without exception, the test passes
        Assert.True(true);
    }

    // Test classes
    private class TestNotification : INotification;

    private class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}