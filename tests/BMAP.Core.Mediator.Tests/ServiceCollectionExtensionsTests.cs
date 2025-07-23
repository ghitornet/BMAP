using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for ServiceCollection extensions.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMediatorFromAssemblyContaining_Should_RegisterMediator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
        Assert.IsType<Mediator>(mediator);
    }

    [Fact]
    public void AddMediatorFromAssemblyContaining_Should_RegisterServiceLocator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var serviceLocator = serviceProvider.GetService<IServiceLocator>();
        Assert.NotNull(serviceLocator);
        Assert.IsType<ServiceLocator>(serviceLocator);
    }

    [Fact]
    public void AddMediatorFromAssemblyContaining_Should_RegisterHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var testRequestHandler = serviceProvider.GetService<IRequestHandler<TestRequest>>();
        var testNotificationHandlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>();

        Assert.NotNull(testRequestHandler);
        Assert.IsType<TestRequestHandler>(testRequestHandler);

        // Should register multiple notification handlers
        var handlerList = testNotificationHandlers.ToList();
        Assert.Equal(2, handlerList.Count);
        Assert.Contains(handlerList, h => h.GetType() == typeof(TestNotificationHandler));
        Assert.Contains(handlerList, h => h.GetType() == typeof(AnotherTestNotificationHandler));
    }

    [Fact]
    public void AddMediatorFromAssemblyContaining_Should_RegisterMultipleNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var notificationHandlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>();

        var handlersList = notificationHandlers.ToList();
        Assert.Equal(2, handlersList.Count);
        Assert.Contains(handlersList, h => h.GetType() == typeof(TestNotificationHandler));
        Assert.Contains(handlersList, h => h.GetType() == typeof(AnotherTestNotificationHandler));
    }

    [Fact]
    public void AddMediatorFromAssemblyContaining_Should_RegisterRequestHandlerWithResponse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromAssemblyContaining<ServiceCollectionExtensionsTests>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var testRequestWithResponseHandler =
            serviceProvider.GetService<IRequestHandler<TestRequestWithResponse, string>>();

        Assert.NotNull(testRequestWithResponseHandler);
        Assert.IsType<TestRequestWithResponseHandler>(testRequestWithResponseHandler);
    }

    [Fact]
    public async Task RegisteredHandlers_Should_WorkWithMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorFromAssemblyContaining<ServiceCollectionExtensionsTests>();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Act
        var request = new TestRequest();
        var requestWithResponse = new TestRequestWithResponse { Value = "test" };
        var notification = new TestNotification();

        await mediator!.SendAsync(request);
        var response = await mediator.SendAsync<string>(requestWithResponse);
        await mediator.PublishAndWaitAsync(notification);

        // Assert
        Assert.Equal("Response: test", response);
        // If we reach here without exceptions, the handlers were called successfully
        Assert.True(true);
    }

    // Test classes for registration
    public class TestRequest : IRequest
    {
    }

    public class TestRequestHandler : IRequestHandler<TestRequest>
    {
        public Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestRequestWithResponse : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public Task<string> HandleAsync(TestRequestWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Response: {request.Value}");
        }
    }

    public class TestNotification : INotification
    {
    }

    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class AnotherTestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}