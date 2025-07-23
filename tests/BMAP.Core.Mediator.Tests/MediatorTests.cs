using BMAP.Core.Mediator.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for Mediator implementation.
/// </summary>
public class MediatorTests
{
    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_WhenServiceLocatorIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Mediator(null!));
        Assert.Equal("serviceLocator", exception.ParamName);
    }

    [Fact]
    public async Task SendAsync_Should_ThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync((TestRequest)null!));
    }

    [Fact]
    public async Task SendAsync_Should_ThrowHandlerNotFoundException_WhenNoHandlerIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var request = new TestRequest();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HandlerNotFoundException>(() => mediator.SendAsync(request));
        Assert.Equal("No handler found for request type 'TestRequest'.", exception.Message);
    }

    [Fact]
    public async Task SendAsync_Should_CallHandler_WhenHandlerIsRegistered()
    {
        // Arrange
        TestRequestHandler.Reset(); // Reset static state
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<TestRequest>, TestRequestHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var request = new TestRequest();

        // Act
        await mediator.SendAsync(request);

        // Assert
        Assert.True(TestRequestHandler.WasCalled);
        Assert.Same(request, TestRequestHandler.ReceivedRequest);
    }

    [Fact]
    public async Task SendAsync_WithResponse_Should_ThrowArgumentNullException_WhenRequestIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.SendAsync((IRequest<string>)null!));
    }

    [Fact]
    public async Task SendAsync_WithResponse_Should_ThrowHandlerNotFoundException_WhenNoHandlerIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var request = new TestRequestWithResponse();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HandlerNotFoundException>(() => mediator.SendAsync<string>(request));
        Assert.Equal("No handler found for request type 'TestRequestWithResponse'.", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WithResponse_Should_CallHandlerAndReturnResponse_WhenHandlerIsRegistered()
    {
        // Arrange
        TestRequestWithResponseHandler.Reset(); // Reset static state
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<TestRequestWithResponse, string>, TestRequestWithResponseHandler>();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var request = new TestRequestWithResponse { Value = "test" };

        // Act
        var result = await mediator.SendAsync<string>(request);

        // Assert
        Assert.Equal("Response: test", result);
        Assert.True(TestRequestWithResponseHandler.WasCalled);
        Assert.Same(request, TestRequestWithResponseHandler.ReceivedRequest);
    }

    [Fact]
    public async Task PublishAsync_Should_ThrowArgumentNullException_WhenNotificationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.PublishAsync((TestNotification)null!));
    }

    [Fact]
    public async Task PublishAsync_Should_NotThrow_WhenNoHandlersAreRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var notification = new TestNotification();

        // Act & Assert - Should not throw
        await mediator.PublishAsync(notification);
        // If we reach here, the test passes
        Assert.True(true);
    }

    [Fact]
    public async Task PublishAsync_Should_CallAllHandlers_WhenMultipleHandlersAreRegistered()
    {
        // Arrange
        TestNotificationHandler1.Reset();
        TestNotificationHandler2.Reset();
        var services = new ServiceCollection();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler2>();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var notification = new TestNotification();

        // Act
        await mediator.PublishAsync(notification);

        // Small delay to ensure async operations complete
        await Task.Delay(100);

        // Assert
        Assert.True(TestNotificationHandler1.WasCalled);
        Assert.True(TestNotificationHandler2.WasCalled);
    }

    [Fact]
    public async Task PublishAndWaitAsync_Should_ThrowArgumentNullException_WhenNotificationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.PublishAndWaitAsync((TestNotification)null!));
    }

    [Fact]
    public async Task PublishAndWaitAsync_Should_NotThrow_WhenNoHandlersAreRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var notification = new TestNotification();

        // Act & Assert - Should not throw
        await mediator.PublishAndWaitAsync(notification);
        // If we reach here, the test passes
        Assert.True(true);
    }

    [Fact]
    public async Task PublishAndWaitAsync_Should_CallAllHandlersAndWait_WhenMultipleHandlersAreRegistered()
    {
        // Arrange
        TestNotificationHandler1.Reset();
        TestNotificationHandler2.Reset();
        var services = new ServiceCollection();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddSingleton<INotificationHandler<TestNotification>, TestNotificationHandler2>();
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = new ServiceLocator(serviceProvider);
        var mediator = new Mediator(serviceLocator);
        var notification = new TestNotification();

        // Act
        await mediator.PublishAndWaitAsync(notification);

        // Assert
        Assert.True(TestNotificationHandler1.WasCalled);
        Assert.True(TestNotificationHandler2.WasCalled);
    }

    // Test classes
    private class TestRequest : IRequest
    {
    }

    private class TestRequestHandler : IRequestHandler<TestRequest>
    {
        public static bool WasCalled { get; private set; }
        public static TestRequest? ReceivedRequest { get; private set; }

        public Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedRequest = request;
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            WasCalled = false;
            ReceivedRequest = null;
        }
    }

    private class TestRequestWithResponse : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public static bool WasCalled { get; private set; }
        public static TestRequestWithResponse? ReceivedRequest { get; private set; }

        public Task<string> HandleAsync(TestRequestWithResponse request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedRequest = request;
            return Task.FromResult($"Response: {request.Value}");
        }

        public static void Reset()
        {
            WasCalled = false;
            ReceivedRequest = null;
        }
    }

    private class TestNotification : INotification
    {
        public string Message { get; set; } = "Test notification";
    }

    private class TestNotificationHandler1 : INotificationHandler<TestNotification>
    {
        public static bool WasCalled { get; private set; }

        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            WasCalled = false;
        }
    }

    private class TestNotificationHandler2 : INotificationHandler<TestNotification>
    {
        public static bool WasCalled { get; private set; }

        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }

        public static void Reset()
        {
            WasCalled = false;
        }
    }
}