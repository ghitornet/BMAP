using BMAP.Core.Mediator.Extensions;
using BMAP.Core.Mediator.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Tests to verify that logging is working correctly throughout the mediator pipeline.
/// </summary>
public class LoggingIntegrationTests
{
    [Fact]
    public async Task Mediator_Should_LogRequestExecution_WhenSendingRequest()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        // Add a simple in-memory logger that captures log output
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator(); // Use AddMediator instead of AddMediatorFromAssemblyContaining
        services.AddTransient<IRequestHandler<TestLogRequest>, TestLogRequestHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var request = new TestLogRequest { Message = "Test logging" };
        
        // Act
        await mediator.SendAsync(request);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending request of type TestLogRequest", logMessages);
        Assert.Contains("Found handler of type TestLogRequestHandler", logMessages);
        Assert.Contains("Successfully executed request TestLogRequest", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_LogRequestWithResponse_WhenSendingRequestWithResponse()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator(); // Use AddMediator instead of AddMediatorFromAssemblyContaining
        services.AddTransient<IRequestHandler<TestLogRequestWithResponse, string>, TestLogRequestWithResponseHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var request = new TestLogRequestWithResponse { Message = "Test logging with response" };
        
        // Act
        var result = await mediator.SendAsync<string>(request);
        
        // Assert
        Assert.Equal("Response: Test logging with response", result);
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending request of type TestLogRequestWithResponse expecting response of type String", logMessages);
        Assert.Contains("Successfully executed request TestLogRequestWithResponse with response", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_LogNotificationPublishing_WhenPublishingNotification()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator(); // Use AddMediator instead of AddMediatorFromAssemblyContaining
        services.AddTransient<INotificationHandler<TestLogNotification>, TestLogNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var notification = new TestLogNotification { Message = "Test notification logging" };
        
        // Act
        await mediator.PublishAndWaitAsync(notification);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Publishing and waiting for event/notification of type TestLogNotification", logMessages);
        Assert.Contains("Found 1 event handlers for notification type TestLogNotification", logMessages);
        Assert.Contains("All event handlers completed successfully", logMessages);
    }

    [Fact]
    public async Task Mediator_Should_LogWhenNoHandlersFound_ForNotification()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator(); // Use AddMediator instead of AddMediatorFromAssemblyContaining
        // Note: Not registering any handlers for TestLogNotification
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var notification = new TestLogNotification { Message = "Test notification logging" };
        
        // Act
        await mediator.PublishAndWaitAsync(notification);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Publishing and waiting for event/notification of type TestLogNotification", logMessages);
        Assert.Contains("No event handlers found for notification type TestLogNotification", logMessages);
    }

    [Fact]
    public async Task Mediator_Should_LogMultipleHandlers_WhenMultipleHandlersRegistered()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator(); // Use AddMediator instead of AddMediatorFromAssemblyContaining
        services.AddTransient<INotificationHandler<TestLogNotification>, TestLogNotificationHandler>();
        services.AddTransient<INotificationHandler<TestLogNotification>, AnotherTestLogNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var notification = new TestLogNotification { Message = "Test notification logging" };
        
        // Act
        await mediator.PublishAndWaitAsync(notification);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Publishing and waiting for event/notification of type TestLogNotification", logMessages);
        Assert.Contains("Found 2 event handlers for notification type TestLogNotification", logMessages);
        Assert.Contains("All event handlers completed successfully", logMessages);
    }

    [Fact]
    public async Task Mediator_Should_LogErrorWhenHandlerNotFound_ForRequest()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        // Don't use AddMediatorFromAssemblyContaining to avoid auto-registration
        services.AddMediator(); // Only register the mediator without scanning for handlers
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var request = new TestLogRequestWithoutHandler { Message = "Test logging" };
        
        // Act & Assert
        await Assert.ThrowsAsync<HandlerNotFoundException>(() => mediator.SendAsync(request));
        
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending request of type TestLogRequestWithoutHandler", logMessages);
        Assert.Contains("No handler found for request type TestLogRequestWithoutHandler", logMessages);
    }
    
    [Fact]
    public async Task ServiceLocator_Should_LogServiceResolution()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator(); // Use AddMediator instead of AddMediatorFromAssemblyContaining
        services.AddTransient<IRequestHandler<TestLogRequest>, TestLogRequestHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var request = new TestLogRequest { Message = "Test service locator logging" };
        
        // Act
        await mediator.SendAsync(request);
        
        // Assert
        var logMessages = logOutput.ToString();
        // Should contain ServiceLocator debug logs for service resolution
        Assert.Contains("[Debug] [BMAP.Core.Mediator.ServiceLocator]", logMessages);
        Assert.Contains("Attempting to resolve service of type", logMessages);
    }
    
    // Test classes
    public class TestLogRequestWithoutHandler : IRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestLogRequest : IRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestLogRequestHandler : IRequestHandler<TestLogRequest>
    {
        public Task HandleAsync(TestLogRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestLogRequestWithResponse : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestLogRequestWithResponseHandler : IRequestHandler<TestLogRequestWithResponse, string>
    {
        public Task<string> HandleAsync(TestLogRequestWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Response: {request.Message}");
        }
    }

    public class TestLogNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestLogNotificationHandler : INotificationHandler<TestLogNotification>
    {
        public Task HandleAsync(TestLogNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class AnotherTestLogNotificationHandler : INotificationHandler<TestLogNotification>
    {
        public Task HandleAsync(TestLogNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Simple test logger provider that captures log output to a StringBuilder.
/// </summary>
public class TestLoggerProvider(StringBuilder logOutput) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(logOutput, categoryName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Simple test logger that writes to a StringBuilder.
/// </summary>
public class TestLogger(StringBuilder logOutput, string categoryName) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        logOutput.AppendLine($"[{logLevel}] [{categoryName}] {message}");
    }
}