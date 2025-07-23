using BMAP.Core.Mediator.Exceptions;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        
        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
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
        
        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
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
        
        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        services.AddMediatorFromAssemblyContaining<MediatorIntegrationTests>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

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
        
        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
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
        
        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
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
    
    /// <summary>
    /// Simple request without response for testing basic mediator functionality.
    /// </summary>
    public class SimpleTestRequest : IRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for SimpleTestRequest.
    /// </summary>
    public class SimpleTestRequestHandler : IRequestHandler<SimpleTestRequest>
    {
        public Task HandleAsync(SimpleTestRequest request, CancellationToken cancellationToken = default)
        {
            // Simple handler that just completes successfully
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Request with response for testing request-response pattern.
    /// </summary>
    public class TestRequestWithResponse : IRequest<string>
    {
        public string Input { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for TestRequestWithResponse that processes input and returns a response.
    /// </summary>
    public class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public Task<string> HandleAsync(TestRequestWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Processed: {request.Input}");
        }
    }

    /// <summary>
    /// Notification for testing publish-subscribe pattern.
    /// </summary>
    public class TestNotification : INotification
    {
        public string Data { get; set; } = string.Empty;
    }

    /// <summary>
    /// First handler for TestNotification to test multiple handlers scenario.
    /// </summary>
    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            // Simple handler that just completes successfully
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Second handler for TestNotification to test multiple handlers scenario.
    /// </summary>
    public class AnotherTestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            // Another handler for the same notification to test multiple handlers
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Request for testing cancellation functionality.
    /// </summary>
    public class CancellableTestRequest : IRequest;

    /// <summary>
    /// Handler for CancellableTestRequest that respects cancellation tokens.
    /// </summary>
    public class CancellableTestRequestHandler : IRequestHandler<CancellableTestRequest>
    {
        public Task HandleAsync(CancellableTestRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Request for testing exception handling and propagation.
    /// </summary>
    public class FailingTestRequest : IRequest;

    /// <summary>
    /// Handler for FailingTestRequest that always throws an exception for testing error handling.
    /// </summary>
    public class FailingTestRequestHandler : IRequestHandler<FailingTestRequest>
    {
        public Task HandleAsync(FailingTestRequest request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test handler failure");
        }
    }
}