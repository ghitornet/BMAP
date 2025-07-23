using BMAP.Core.Mediator.Behaviors;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for pipeline behaviors and delegates.
/// </summary>
public class PipelineBehaviorTests
{
    [Fact]
    public void IPipelineBehavior_Should_HaveHandleAsyncMethod()
    {
        // Arrange
        var behaviorType = typeof(IPipelineBehavior<>);

        // Act
        var methods = behaviorType.GetMethods();
        var handleAsyncMethod = methods.FirstOrDefault(m => m.Name == "HandleAsync");

        // Assert
        Assert.NotNull(handleAsyncMethod);
        Assert.Equal(typeof(Task), handleAsyncMethod.ReturnType);
    }

    [Fact]
    public void IPipelineBehaviorWithResponse_Should_HaveHandleAsyncMethod()
    {
        // Arrange
        var behaviorType = typeof(IPipelineBehavior<,>);

        // Act
        var methods = behaviorType.GetMethods();
        var handleAsyncMethod = methods.FirstOrDefault(m => m.Name == "HandleAsync");

        // Assert
        Assert.NotNull(handleAsyncMethod);
        Assert.StartsWith("Task", handleAsyncMethod.ReturnType.Name);
    }

    [Fact]
    public async Task RequestHandlerDelegate_Should_BeCallable()
    {
        // Arrange
        bool wasCalled;

        // Act
        await Del();

        // Assert
        Assert.True(wasCalled);
        return;

        Task Del()
        {
            wasCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task RequestHandlerDelegateWithResponse_Should_BeCallable()
    {
        // Arrange
        var expectedResponse = "test response";

        // Act
        var result = await Del();

        // Assert
        Assert.Equal(expectedResponse, result);
        return;

        Task<string> Del() => Task.FromResult(expectedResponse);
    }

    [Fact]
    public async Task ConcretePipelineBehavior_Should_ImplementIPipelineBehavior()
    {
        // Arrange
        var behavior = new TestPipelineBehavior();
        var request = new TestRequest();
        var nextCalled = false;

        // Act
        await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.True(nextCalled);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ConcretePipelineBehaviorWithResponse_Should_ImplementIPipelineBehavior()
    {
        // Arrange
        var behavior = new TestPipelineBehaviorWithResponse();
        var request = new TestRequestWithResponse();
        var expectedResponse = "test response";
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.True(nextCalled);
        Assert.Equal(expectedResponse, result);
        return;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }
    }

    [Fact]
    public async Task PipelineBehavior_Should_HandleExceptions()
    {
        // Arrange
        var behavior = new TestPipelineBehavior();
        var request = new TestRequest();
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(request, Next));
        Assert.Equal("Test exception", actualException.Message);
        Assert.True(behavior.WasCalled);
        return;

        Task Next() => throw expectedException;
    }

    [Fact]
    public async Task PipelineBehaviorWithResponse_Should_HandleExceptions()
    {
        // Arrange
        var behavior = new TestPipelineBehaviorWithResponse();
        var request = new TestRequestWithResponse();
        var expectedException = new InvalidOperationException("Test exception");

        // Act & Assert
        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(request, Next));
        Assert.Equal("Test exception", actualException.Message);
        Assert.True(behavior.WasCalled);
        return;

        Task<string> Next() => throw expectedException;
    }

    [Fact]
    public async Task PipelineBehavior_Should_AcceptCancellationToken()
    {
        // Arrange
        var behavior = new TestPipelineBehavior();
        var request = new TestRequest();
        var cancellationToken = CancellationToken.None;

        // Act
        await behavior.HandleAsync(request, Next, cancellationToken);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.Equal(cancellationToken, behavior.ReceivedCancellationToken);
        return;

        static Task Next() => Task.CompletedTask;
    }

    [Fact]
    public async Task PipelineBehaviorWithResponse_Should_AcceptCancellationToken()
    {
        // Arrange
        var behavior = new TestPipelineBehaviorWithResponse();
        var request = new TestRequestWithResponse();
        var cancellationToken = CancellationToken.None;

        // Act
        await behavior.HandleAsync(request, Next, cancellationToken);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.Equal(cancellationToken, behavior.ReceivedCancellationToken);
        return;

        static Task<string> Next() => Task.FromResult("response");
    }

    // Test classes
    private class TestRequest : IRequest;

    private class TestRequestWithResponse : IRequest<string>;

    private class TestPipelineBehavior : IPipelineBehavior<TestRequest>
    {
        public bool WasCalled { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public async Task HandleAsync(TestRequest request, RequestHandlerDelegate next,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;
            await next();
        }
    }

    private class TestPipelineBehaviorWithResponse : IPipelineBehavior<TestRequestWithResponse, string>
    {
        public bool WasCalled { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public async Task<string> HandleAsync(TestRequestWithResponse request, RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;
            return await next();
        }
    }
}