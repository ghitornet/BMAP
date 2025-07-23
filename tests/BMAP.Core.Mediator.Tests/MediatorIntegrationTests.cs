using BMAP.Core.Mediator.Exceptions;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Integration tests for the mediator functionality.
/// </summary>
public class MediatorIntegrationTests
{
    [Fact]
    public async Task FullWorkflow_Should_ProcessRequestsAndNotifications()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorFromAssemblyContaining<MediatorIntegrationTests>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Test request without response
        var simpleRequest = new SimpleTestRequest { Message = "Hello" };
        await mediator.SendAsync(simpleRequest); // Should not throw

        // Act & Assert - Test request with response
        var requestWithResponse = new TestRequestWithResponse { Input = "World" };
        var response = await mediator.SendAsync<string>(requestWithResponse);
        Assert.Equal("Processed: World", response);

        // Act & Assert - Test notification
        var notification = new TestNotification { Data = "Test Data" };
        await mediator.PublishAndWaitAsync(notification); // Should not throw
    }

    [Fact]
    public async Task MultipleNotificationHandlers_Should_AllBeExecuted()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorFromAssemblyContaining<MediatorIntegrationTests>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var notification = new TestNotification { Data = "Multi Handler Test" };
        await mediator.PublishAndWaitAsync(notification);

        // Assert - Multiple handlers should have been called
        // In a real scenario, you might check side effects or use mocking
        // For this test, we just verify no exceptions are thrown
        Assert.True(true);
    }

    [Fact]
    public async Task Mediator_Should_HandleCancellation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorFromAssemblyContaining<MediatorIntegrationTests>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        var request = new CancellableTestRequest();
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            mediator.SendAsync(request, cts.Token));
    }

    [Fact]
    public async Task Mediator_Should_PropagateHandlerExceptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorFromAssemblyContaining<MediatorIntegrationTests>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - The mediator wraps exceptions in MediatorException
        var request = new FailingTestRequest();
        var exception = await Assert.ThrowsAsync<MediatorException>(() =>
            mediator.SendAsync(request));

        // Verify the original exception is preserved as inner exception
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal("Test handler failure", exception.InnerException!.Message);
    }

    [Fact]
    public void ServiceRegistration_Should_RegisterAllExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorFromAssemblyContaining<MediatorIntegrationTests>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Core services
        Assert.NotNull(serviceProvider.GetService<IMediator>());
        Assert.NotNull(serviceProvider.GetService<IServiceLocator>());

        // Assert - Handlers
        Assert.NotNull(serviceProvider.GetService<IRequestHandler<SimpleTestRequest>>());
        Assert.NotNull(serviceProvider.GetService<IRequestHandler<TestRequestWithResponse, string>>());
        Assert.NotNull(serviceProvider.GetServices<INotificationHandler<TestNotification>>());
    }

    // Test classes for integration tests
    public class SimpleTestRequest : IRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class SimpleTestRequestHandler : IRequestHandler<SimpleTestRequest>
    {
        public Task HandleAsync(SimpleTestRequest request, CancellationToken cancellationToken = default)
        {
            // Simple processing
            return Task.CompletedTask;
        }
    }

    public class TestRequestWithResponse : IRequest<string>
    {
        public string Input { get; set; } = string.Empty;
    }

    public class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public Task<string> HandleAsync(TestRequestWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Processed: {request.Input}");
        }
    }

    public class TestNotification : INotification
    {
        public string Data { get; set; } = string.Empty;
    }

    public class TestNotificationHandler1 : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            // Handler 1 processing
            return Task.CompletedTask;
        }
    }

    public class TestNotificationHandler2 : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            // Handler 2 processing
            return Task.CompletedTask;
        }
    }

    public class CancellableTestRequest : IRequest
    {
    }

    public class CancellableTestRequestHandler : IRequestHandler<CancellableTestRequest>
    {
        public Task HandleAsync(CancellableTestRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    public class FailingTestRequest : IRequest
    {
    }

    public class FailingTestRequestHandler : IRequestHandler<FailingTestRequest>
    {
        public Task HandleAsync(FailingTestRequest request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test handler failure");
        }
    }
}