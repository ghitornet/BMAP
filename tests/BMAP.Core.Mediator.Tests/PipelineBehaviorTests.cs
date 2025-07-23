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
        var wasCalled = false;
        RequestHandlerDelegate del = () =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await del();

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task RequestHandlerDelegateWithResponse_Should_BeCallable()
    {
        // Arrange
        var expectedResponse = "test response";
        RequestHandlerDelegate<string> del = () => Task.FromResult(expectedResponse);

        // Act
        var result = await del();

        // Assert
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task ConcretePipelineBehavior_Should_ImplementIPipelineBehavior()
    {
        // Arrange
        var behavior = new TestPipelineBehavior();
        var request = new TestRequest();
        var nextCalled = false;
        RequestHandlerDelegate next = () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await behavior.HandleAsync(request, next);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ConcretePipelineBehaviorWithResponse_Should_ImplementIPipelineBehavior()
    {
        // Arrange
        var behavior = new TestPipelineBehaviorWithResponse();
        var request = new TestRequestWithResponse();
        var expectedResponse = "test response";
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.HandleAsync(request, next);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.True(nextCalled);
        Assert.Equal(expectedResponse, result);
    }

    [Fact]
    public async Task PipelineBehavior_Should_HandleExceptions()
    {
        // Arrange
        var behavior = new TestPipelineBehavior();
        var request = new TestRequest();
        var expectedException = new InvalidOperationException("Test exception");
        RequestHandlerDelegate next = () => throw expectedException;

        // Act & Assert
        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(request, next));
        Assert.Equal("Test exception", actualException.Message);
        Assert.True(behavior.WasCalled);
    }

    [Fact]
    public async Task PipelineBehaviorWithResponse_Should_HandleExceptions()
    {
        // Arrange
        var behavior = new TestPipelineBehaviorWithResponse();
        var request = new TestRequestWithResponse();
        var expectedException = new InvalidOperationException("Test exception");
        RequestHandlerDelegate<string> next = () => throw expectedException;

        // Act & Assert
        var actualException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(request, next));
        Assert.Equal("Test exception", actualException.Message);
        Assert.True(behavior.WasCalled);
    }

    [Fact]
    public async Task PipelineBehavior_Should_AcceptCancellationToken()
    {
        // Arrange
        var behavior = new TestPipelineBehavior();
        var request = new TestRequest();
        var cancellationToken = new CancellationToken();
        RequestHandlerDelegate next = () => Task.CompletedTask;

        // Act
        await behavior.HandleAsync(request, next, cancellationToken);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.Equal(cancellationToken, behavior.ReceivedCancellationToken);
    }

    [Fact]
    public async Task PipelineBehaviorWithResponse_Should_AcceptCancellationToken()
    {
        // Arrange
        var behavior = new TestPipelineBehaviorWithResponse();
        var request = new TestRequestWithResponse();
        var cancellationToken = new CancellationToken();
        RequestHandlerDelegate<string> next = () => Task.FromResult("response");

        // Act
        await behavior.HandleAsync(request, next, cancellationToken);

        // Assert
        Assert.True(behavior.WasCalled);
        Assert.Equal(cancellationToken, behavior.ReceivedCancellationToken);
    }

    // Test classes
    private class TestRequest : IRequest
    {
    }

    private class TestRequestWithResponse : IRequest<string>
    {
    }

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