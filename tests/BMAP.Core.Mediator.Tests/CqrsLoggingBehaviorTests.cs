using BMAP.Core.Mediator.Behaviors;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for CQRS-specific logging behaviors.
/// </summary>
public class CqrsLoggingBehaviorTests
{
    [Fact]
    public async Task Command_Logging_Behavior_Should_Log_Execution_Details()
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
        
        // Register command handler using convenient extension method (registers both interfaces)
        services.AddCommandHandler<TestLoggingCommand, TestLoggingCommandHandler>();
        
        // Register the logging behavior as a pipeline behavior
        services.AddTransient<IPipelineBehavior<TestLoggingCommand>, CommandLoggingBehavior<TestLoggingCommand>>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestLoggingCommand { Name = "Test Command" };
        
        // Act
        await mediator.SendAsync(command);
        
        // Assert
        var logMessages = logOutput.ToString();
        Assert.Contains("Executing command TestLoggingCommand at", logMessages);
        Assert.Contains("Command TestLoggingCommand executed successfully", logMessages);
        Assert.Contains("ms", logMessages); // Should contain timing information
    }
    
    [Fact]
    public async Task Command_With_Response_Logging_Behavior_Should_Log_Execution_And_Response()
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
        
        // Register command handler using convenient extension method (registers both interfaces)
        services.AddCommandHandler<TestLoggingCommandWithResponse, int, TestLoggingCommandWithResponseHandler>();
        
        // Register the logging behavior as a pipeline behavior
        services.AddTransient<IPipelineBehavior<TestLoggingCommandWithResponse, int>, CommandLoggingBehavior<TestLoggingCommandWithResponse, int>>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestLoggingCommandWithResponse { Name = "Test Command With Response" };
        
        // Act
        var result = await mediator.SendAsync<int>(command);
        
        // Assert
        Assert.True(result > 0);
        var logMessages = logOutput.ToString();
        Assert.Contains("Executing command TestLoggingCommandWithResponse expecting response Int32", logMessages);
        Assert.Contains("Command TestLoggingCommandWithResponse executed successfully", logMessages);
        Assert.Contains("with response type Int32", logMessages);
    }
    
    [Fact]
    public async Task Query_Logging_Behavior_Should_Log_Execution_And_Performance_Metrics()
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
        
        // Register query handler using convenient extension method (registers both interfaces)
        services.AddQueryHandler<TestLoggingQuery, string, TestLoggingQueryHandler>();
        
        // Register the logging behavior as a pipeline behavior
        services.AddTransient<IPipelineBehavior<TestLoggingQuery, string>, QueryLoggingBehavior<TestLoggingQuery, string>>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var query = new TestLoggingQuery { SearchTerm = "test" };
        
        // Act
        var result = await mediator.SendAsync<string>(query);
        
        // Assert
        Assert.Equal("Result for test", result);
        var logMessages = logOutput.ToString();
        Assert.Contains("Executing query TestLoggingQuery expecting response String", logMessages);
        Assert.Contains("Query TestLoggingQuery executed successfully", logMessages);
        Assert.Contains("ms", logMessages); // Should contain timing information
    }
    
    [Fact]
    public async Task Query_Logging_Behavior_Should_Warn_About_Large_Collections()
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
        
        // Register query handler using convenient extension method (registers both interfaces)
        services.AddQueryHandler<TestLargeResultQuery, List<string>, TestLargeResultQueryHandler>();
        
        // Register the logging behavior as a pipeline behavior
        services.AddTransient<IPipelineBehavior<TestLargeResultQuery, List<string>>, QueryLoggingBehavior<TestLargeResultQuery, List<string>>>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var query = new TestLargeResultQuery { Count = 1500 }; // Will return more than 1000 items
        
        // Act
        var result = await mediator.SendAsync<List<string>>(query);
        
        // Assert
        Assert.Equal(1500, result.Count);
        var logMessages = logOutput.ToString();
        Assert.Contains("Query TestLargeResultQuery returned 1500 items", logMessages);
        Assert.Contains("consider pagination for large datasets", logMessages);
    }
    
    [Fact]
    public async Task Command_Logging_Should_Warn_About_Slow_Execution()
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
        
        // Register command handler using convenient extension method (registers both interfaces)
        services.AddCommandHandler<TestSlowCommand, TestSlowCommandHandler>();
        
        // Register the logging behavior as a pipeline behavior
        services.AddTransient<IPipelineBehavior<TestSlowCommand>, CommandLoggingBehavior<TestSlowCommand>>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestSlowCommand();
        
        // Act
        await mediator.SendAsync(command);
        
        // Assert
        var logMessages = logOutput.ToString();
        // The slow command simulates 6 seconds, should trigger warning
        Assert.Contains("exceeds the recommended threshold", logMessages);
    }

    // Test classes
    public class TestLoggingCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestLoggingCommandHandler : ICommandHandler<TestLoggingCommand>
    {
        public Task HandleAsync(TestLoggingCommand request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class TestLoggingCommandWithResponse : ICommand<int>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestLoggingCommandWithResponseHandler : ICommandHandler<TestLoggingCommandWithResponse, int>
    {
        private static int _id = 1;
        
        public Task<int> HandleAsync(TestLoggingCommandWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_id++);
        }
    }

    public class TestLoggingQuery : IQuery<string>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class TestLoggingQueryHandler : IQueryHandler<TestLoggingQuery, string>
    {
        public Task<string> HandleAsync(TestLoggingQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Result for {request.SearchTerm}");
        }
    }

    public class TestLargeResultQuery : IQuery<List<string>>
    {
        public int Count { get; set; }
    }

    public class TestLargeResultQueryHandler : IQueryHandler<TestLargeResultQuery, List<string>>
    {
        public Task<List<string>> HandleAsync(TestLargeResultQuery request, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();
            for (int i = 0; i < request.Count; i++)
            {
                result.Add($"Item {i}");
            }
            return Task.FromResult(result);
        }
    }

    public class TestSlowCommand : ICommand
    {
    }

    public class TestSlowCommandHandler : ICommandHandler<TestSlowCommand>
    {
        public async Task HandleAsync(TestSlowCommand request, CancellationToken cancellationToken = default)
        {
            // Simulate slow operation (6 seconds to trigger warning threshold)
            await Task.Delay(6000, cancellationToken);
        }
    }
}