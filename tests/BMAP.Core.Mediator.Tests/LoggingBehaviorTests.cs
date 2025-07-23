using BMAP.Core.Mediator.Behaviors;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for logging behaviors.
/// </summary>
public class LoggingBehaviorTests
{
    [Fact]
    public async Task LoggingBehavior_WithResponse_Should_CallNext_AndReturnResponse()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequestWithResponse, string>>();
        var behavior = new LoggingBehavior<TestRequestWithResponse, string>(logger);
        var request = new TestRequestWithResponse();
        var expectedResponse = "Test Response";
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }

        // Act
        var result = await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task LoggingBehavior_WithResponse_Should_PropagateExceptions()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequestWithResponse, string>>();
        var behavior = new LoggingBehavior<TestRequestWithResponse, string>(logger);
        var request = new TestRequestWithResponse();
        var expectedException = new InvalidOperationException("Test exception");

        Task<string> Next() => throw expectedException;

        // Act & Assert
        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(request, Next));
        Assert.Equal("Test exception", actualException.Message);
    }

    [Fact]
    public async Task LoggingBehavior_WithoutResponse_Should_CallNext()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequest>>();
        var behavior = new LoggingBehavior<TestRequest>(logger);
        var request = new TestRequest();
        var nextCalled = false;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task LoggingBehavior_WithoutResponse_Should_PropagateExceptions()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequest>>();
        var behavior = new LoggingBehavior<TestRequest>(logger);
        var request = new TestRequest();
        var expectedException = new InvalidOperationException("Test exception");

        Task Next() => throw expectedException;

        // Act & Assert
        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(request, Next));
        Assert.Equal("Test exception", actualException.Message);
    }

    [Fact]
    public async Task LoggingBehavior_WithResponse_Should_HandleSlowOperations()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequestWithResponse, string>>();
        var behavior = new LoggingBehavior<TestRequestWithResponse, string>(logger);
        var request = new TestRequestWithResponse();
        var expectedResponse = "Slow Response";

        async Task<string> Next()
        {
            await Task.Delay(100); // Simulate slow operation
            return expectedResponse;
        }

        // Act
        var result = await behavior.HandleAsync(request, Next);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task LoggingBehavior_WithoutResponse_Should_HandleSlowOperations()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequest>>();
        var behavior = new LoggingBehavior<TestRequest>(logger);
        var request = new TestRequest();
        var nextCalled = false;

        async Task Next()
        {
            await Task.Delay(100); // Simulate slow operation
            nextCalled = true;
        }

        // Act
        await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task LoggingBehavior_WithResponse_Should_AcceptCancellationToken()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequestWithResponse, string>>();
        var behavior = new LoggingBehavior<TestRequestWithResponse, string>(logger);
        var request = new TestRequestWithResponse();
        var expectedResponse = "Response";
        var cancellationToken = CancellationToken.None;

        Task<string> Next() => Task.FromResult(expectedResponse);

        // Act
        var result = await behavior.HandleAsync(request, Next, cancellationToken);

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task LoggingBehavior_WithoutResponse_Should_AcceptCancellationToken()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequest>>();
        var behavior = new LoggingBehavior<TestRequest>(logger);
        var request = new TestRequest();
        var nextCalled = false;
        var cancellationToken = CancellationToken.None;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await behavior.HandleAsync(request, Next, cancellationToken);

        // Assert
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task LoggingBehavior_WithResponse_Should_HandleCancellation()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequestWithResponse, string>>();
        var behavior = new LoggingBehavior<TestRequestWithResponse, string>(logger);
        var request = new TestRequestWithResponse();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        Task<string> Next()
        {
            cts.Token.ThrowIfCancellationRequested();
            return Task.FromResult("Response");
        }

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => behavior.HandleAsync(request, Next, cts.Token));
    }

    [Fact]
    public async Task LoggingBehavior_WithoutResponse_Should_HandleCancellation()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<LoggingBehavior<TestRequest>>();
        var behavior = new LoggingBehavior<TestRequest>(logger);
        var request = new TestRequest();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        Task Next()
        {
            cts.Token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => behavior.HandleAsync(request, Next, cts.Token));
    }

    // Test classes
    private class TestRequest : IRequest
    {
    }

    private class TestRequestWithResponse : IRequest<string>
    {
    }
}