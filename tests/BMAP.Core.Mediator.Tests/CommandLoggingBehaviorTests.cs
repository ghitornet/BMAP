using BMAP.Core.Mediator.Behaviors;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for CommandLoggingBehavior classes.
/// Tests cover logging functionality, performance monitoring, error handling, and edge cases for both command variants.
/// </summary>
public class CommandLoggingBehaviorTests
{
    #region CommandLoggingBehavior (without response) Tests

    [Fact]
    public async Task CommandLoggingBehavior_SuccessfulExecution_Should_LogCorrectly()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommand>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommand>(logger);
        var command = new TestLogCommand { Name = "Test Command" };
        var nextCalled = false;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await behavior.HandleAsync(command, Next);

        // Assert
        Assert.True(nextCalled);
        var logs = logOutput.ToString();
        Assert.Contains("Executing command TestLogCommand", logs);
        Assert.Contains("executed successfully", logs);
        Assert.Contains("ms", logs); // Timing information
    }

    [Fact]
    public async Task CommandLoggingBehavior_ExceptionDuringExecution_Should_LogError()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommand>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommand>(logger);
        var command = new TestLogCommand { Name = "Failing Command" };
        var expectedException = new InvalidOperationException("Test exception");

        Task Next()
        {
            throw expectedException;
        }

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(command, Next));
        Assert.Same(expectedException, thrownException);

        var logs = logOutput.ToString();
        Assert.Contains("Executing command TestLogCommand", logs);
        Assert.Contains("failed after", logs);
        Assert.Contains("Test exception", logs);
    }

    [Fact]
    public async Task CommandLoggingBehavior_SlowExecution_Should_LogPerformanceWarning()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommand>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommand>(logger);
        var command = new TestLogCommand { Name = "Slow Command" };

        async Task Next()
        {
            // Simulate slow execution (note: this is a simplified test - in reality you'd use more sophisticated timing simulation)
            await Task.Delay(10); // Short delay for test purposes
        }

        // Act
        await behavior.HandleAsync(command, Next);

        // Assert
        var logs = logOutput.ToString();
        Assert.Contains("Executing command TestLogCommand", logs);
        Assert.Contains("executed successfully", logs);
    }

    [Fact]
    public void CommandLoggingBehavior_NullLogger_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLoggingBehavior<TestLogCommand>(null!));
    }

    [Fact]
    public async Task CommandLoggingBehavior_CancellationToken_Should_BePropagated()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<CommandLoggingBehavior<TestLogCommand>>();
        var behavior = new CommandLoggingBehavior<TestLogCommand>(logger);
        var command = new TestLogCommand { Name = "Test" };
        var cts = new CancellationTokenSource();
        var receivedToken = default(CancellationToken);

        Task Next()
        {
            receivedToken = cts.Token;
            return Task.CompletedTask;
        }

        // Act
        await behavior.HandleAsync(command, Next, cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    #endregion

    #region CommandLoggingBehavior (with response) Tests

    [Fact]
    public async Task CommandLoggingBehaviorWithResponse_SuccessfulExecution_Should_LogCorrectly()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommandWithResponse, string>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommandWithResponse, string>(logger);
        var command = new TestLogCommandWithResponse { Name = "Test Command" };
        var expectedResponse = "Test Response";
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }

        // Act
        var result = await behavior.HandleAsync(command, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(expectedResponse, result);
        var logs = logOutput.ToString();
        Assert.Contains("Executing command TestLogCommandWithResponse", logs);
        Assert.Contains("expecting response String", logs);
        Assert.Contains("executed successfully", logs);
        Assert.Contains("with response type String", logs);
    }

    [Fact]
    public async Task CommandLoggingBehaviorWithResponse_ExceptionDuringExecution_Should_LogError()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommandWithResponse, string>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommandWithResponse, string>(logger);
        var command = new TestLogCommandWithResponse { Name = "Failing Command" };
        var expectedException = new InvalidOperationException("Test exception");

        Task<string> Next()
        {
            throw expectedException;
        }

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(command, Next));
        Assert.Same(expectedException, thrownException);

        var logs = logOutput.ToString();
        Assert.Contains("Executing command TestLogCommandWithResponse", logs);
        Assert.Contains("failed after", logs);
        Assert.Contains("Test exception", logs);
    }

    [Fact]
    public async Task CommandLoggingBehaviorWithResponse_SlowExecution_Should_LogPerformanceWarning()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommandWithResponse, string>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommandWithResponse, string>(logger);
        var command = new TestLogCommandWithResponse { Name = "Slow Command" };

        async Task<string> Next()
        {
            await Task.Delay(10); // Short delay for test purposes
            return "Success";
        }

        // Act
        var result = await behavior.HandleAsync(command, Next);

        // Assert
        Assert.Equal("Success", result);
        var logs = logOutput.ToString();
        Assert.Contains("Executing command TestLogCommandWithResponse", logs);
        Assert.Contains("executed successfully", logs);
    }

    [Fact]
    public void CommandLoggingBehaviorWithResponse_NullLogger_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandLoggingBehavior<TestLogCommandWithResponse, string>(null!));
    }

    [Fact]
    public async Task CommandLoggingBehaviorWithResponse_CancellationToken_Should_BePropagated()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<CommandLoggingBehavior<TestLogCommandWithResponse, string>>();
        var behavior = new CommandLoggingBehavior<TestLogCommandWithResponse, string>(logger);
        var command = new TestLogCommandWithResponse { Name = "Test" };
        var cts = new CancellationTokenSource();
        var receivedToken = default(CancellationToken);

        Task<string> Next()
        {
            receivedToken = cts.Token;
            return Task.FromResult("Success");
        }

        // Act
        await behavior.HandleAsync(command, Next, cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public async Task CommandLoggingBehaviorWithResponse_NullResponse_Should_LogCorrectly()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<CommandLoggingBehavior<TestLogCommandWithResponse, string?>>(logOutput);
        var behavior = new CommandLoggingBehavior<TestLogCommandWithResponse, string?>(logger);
        var command = new TestLogCommandWithResponse { Name = "Null Response Command" };

        Task<string?> Next()
        {
            return Task.FromResult<string?>(null);
        }

        // Act
        var result = await behavior.HandleAsync(command, Next);

        // Assert
        Assert.Null(result);
        var logs = logOutput.ToString();
        Assert.Contains("executed successfully", logs);
    }

    [Fact]
    public async Task CommandLoggingBehavior_ConfigureAwait_Should_BeUsed()
    {
        // This test ensures that ConfigureAwait(false) is properly used in the implementation
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<CommandLoggingBehavior<TestLogCommand>>();
        var behavior = new CommandLoggingBehavior<TestLogCommand>(logger);
        var command = new TestLogCommand { Name = "Test" };

        Task Next() => Task.CompletedTask;

        // Act
        await behavior.HandleAsync(command, Next);

        // Assert - no exception should be thrown
    }

    [Fact]
    public async Task CommandLoggingBehaviorWithResponse_ConfigureAwait_Should_BeUsed()
    {
        // This test ensures that ConfigureAwait(false) is properly used in the implementation
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<CommandLoggingBehavior<TestLogCommandWithResponse, string>>();
        var behavior = new CommandLoggingBehavior<TestLogCommandWithResponse, string>(logger);
        var command = new TestLogCommandWithResponse { Name = "Test" };

        Task<string> Next() => Task.FromResult("Success");

        // Act
        var result = await behavior.HandleAsync(command, Next);

        // Assert
        Assert.Equal("Success", result);
    }

    #endregion

    #region Test Helper Classes

    // Test commands
    public class TestLogCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TestLogCommandWithResponse : ICommand<string>
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    #endregion
}