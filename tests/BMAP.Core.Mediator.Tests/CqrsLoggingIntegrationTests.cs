using BMAP.Core.Mediator.Extensions;
using BMAP.Core.Mediator.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Tests to verify that CQRS-specific logging is working correctly throughout the mediator pipeline.
/// </summary>
public class CqrsLoggingIntegrationTests
{
    [Fact]
    public async Task Mediator_Should_Log_Command_Execution_With_CQRS_Context()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator();
        // Use convenient extension method that registers both interfaces automatically
        services.AddCommandHandler<TestCqrsLogCommand, TestCqrsLogCommandHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestCqrsLogCommand { Name = "Test Command" };
        
        // Act
        await mediator.SendAsync(command);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending command of type TestCqrsLogCommand", logMessages);
        Assert.Contains("Successfully executed command TestCqrsLogCommand", logMessages);
        Assert.Contains("Found handler of type TestCqrsLogCommandHandler for command TestCqrsLogCommand", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_Log_Command_With_Response_And_CQRS_Context()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator();
        // Use convenient extension method that registers both interfaces automatically
        services.AddCommandHandler<TestCqrsLogCommandWithResponse, int, TestCqrsLogCommandWithResponseHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestCqrsLogCommandWithResponse { ProductName = "Test Product" };
        
        // Act
        var result = await mediator.SendAsync<int>(command);
        
        // Assert
        Assert.True(result > 0);
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending command of type TestCqrsLogCommandWithResponse expecting response of type Int32", logMessages);
        Assert.Contains("Successfully executed command TestCqrsLogCommandWithResponse with response of type Int32", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_Log_Query_Execution_With_CQRS_Context()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator();
        // Use convenient extension method that registers both interfaces automatically
        services.AddQueryHandler<TestCqrsLogQuery, string, TestCqrsLogQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var query = new TestCqrsLogQuery { SearchTerm = "test" };
        
        // Act
        var result = await mediator.SendAsync<string>(query);
        
        // Assert
        Assert.Equal("Result for test", result);
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending query of type TestCqrsLogQuery expecting response of type String", logMessages);
        Assert.Contains("Successfully executed query TestCqrsLogQuery with response of type String", logMessages);
        Assert.Contains("Found handler of type TestCqrsLogQueryHandler for query TestCqrsLogQuery", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_Log_Generic_Request_Without_CQRS_Context()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator();
        services.AddTransient<IRequestHandler<TestCqrsLogGenericRequest>, TestCqrsLogGenericRequestHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var request = new TestCqrsLogGenericRequest { Data = "generic data" };
        
        // Act
        await mediator.SendAsync(request);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending request of type TestCqrsLogGenericRequest", logMessages);
        Assert.Contains("Successfully executed request TestCqrsLogGenericRequest", logMessages);
        // Should not contain "command" or "query" since it's a generic request
        Assert.DoesNotContain("Sending command", logMessages);
        Assert.DoesNotContain("Sending query", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_Log_Event_Publishing_With_Enhanced_Context()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator();
        services.AddTransient<INotificationHandler<TestCqrsLogEvent>, TestCqrsLogEventHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var notification = new TestCqrsLogEvent { EventData = "test event" };
        
        // Act
        await mediator.PublishAndWaitAsync(notification);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Publishing and waiting for event/notification of type TestCqrsLogEvent", logMessages);
        Assert.Contains("Found 1 event handlers for notification type TestCqrsLogEvent", logMessages);
        Assert.Contains("All event handlers completed successfully for notification type TestCqrsLogEvent", logMessages);
    }
    
    [Fact]
    public async Task Mediator_Should_Log_Error_With_CQRS_Context_When_Handler_Not_Found()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediator();
        // Note: Not registering handler for TestCqrsLogMissingCommand
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestCqrsLogMissingCommand { Name = "missing handler" };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HandlerNotFoundException>(() => mediator.SendAsync(command));
        
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending command of type TestCqrsLogMissingCommand", logMessages);
        Assert.Contains("No handler found for command type TestCqrsLogMissingCommand", logMessages);
    }

    // Test classes
    public class TestCqrsLogCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestCqrsLogCommandHandler : ICommandHandler<TestCqrsLogCommand>
    {
        public Task HandleAsync(TestCqrsLogCommand request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCqrsLogCommandWithResponse : ICommand<int>
    {
        public string ProductName { get; set; } = string.Empty;
    }

    public class TestCqrsLogCommandWithResponseHandler : ICommandHandler<TestCqrsLogCommandWithResponse, int>
    {
        private static int _id = 1;
        
        public Task<int> HandleAsync(TestCqrsLogCommandWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_id++);
        }
    }

    public class TestCqrsLogQuery : IQuery<string>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class TestCqrsLogQueryHandler : IQueryHandler<TestCqrsLogQuery, string>
    {
        public Task<string> HandleAsync(TestCqrsLogQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Result for {request.SearchTerm}");
        }
    }

    public class TestCqrsLogGenericRequest : IRequest
    {
        public string Data { get; set; } = string.Empty;
    }

    public class TestCqrsLogGenericRequestHandler : IRequestHandler<TestCqrsLogGenericRequest>
    {
        public Task HandleAsync(TestCqrsLogGenericRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCqrsLogEvent : INotification
    {
        public string EventData { get; set; } = string.Empty;
    }

    public class TestCqrsLogEventHandler : INotificationHandler<TestCqrsLogEvent>
    {
        public Task HandleAsync(TestCqrsLogEvent notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCqrsLogMissingCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }
}