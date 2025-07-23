namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for IRequestHandler interfaces.
/// </summary>
public class RequestHandlerTests
{
    [Fact]
    public void IRequestHandler_Should_HaveHandleAsyncMethod()
    {
        // Arrange
        var handlerType = typeof(IRequestHandler<>);

        // Act
        var methods = handlerType.GetMethods();
        var handleAsyncMethod = methods.FirstOrDefault(m => m.Name == "HandleAsync");

        // Assert
        Assert.NotNull(handleAsyncMethod);
        Assert.Equal(typeof(Task), handleAsyncMethod.ReturnType);
    }

    [Fact]
    public void IRequestHandlerWithResponse_Should_HaveHandleAsyncMethod()
    {
        // Arrange
        var handlerType = typeof(IRequestHandler<,>);

        // Act
        var methods = handlerType.GetMethods();
        var handleAsyncMethod = methods.FirstOrDefault(m => m.Name == "HandleAsync");

        // Assert
        Assert.NotNull(handleAsyncMethod);
        Assert.StartsWith("Task", handleAsyncMethod.ReturnType.Name);
    }

    [Fact]
    public void ConcreteRequestHandler_Should_ImplementIRequestHandler()
    {
        // Arrange
        var concreteHandler = new TestRequestHandler();

        // Act
        var isHandler = concreteHandler is IRequestHandler<TestRequest>;

        // Assert
        Assert.True(isHandler);
    }

    [Fact]
    public void ConcreteRequestHandlerWithResponse_Should_ImplementIRequestHandlerWithResponse()
    {
        // Arrange
        var concreteHandler = new TestRequestWithResponseHandler();

        // Act
        var isHandler = concreteHandler is IRequestHandler<TestRequestWithResponse, string>;

        // Assert
        Assert.True(isHandler);
    }

    [Fact]
    public async Task RequestHandler_HandleAsync_Should_BeCallable()
    {
        // Arrange
        var handler = new TestRequestHandler();
        var request = new TestRequest();

        // Act & Assert
        await handler.HandleAsync(request);
        // If we reach here without exception, the test passes
        Assert.True(true);
    }

    [Fact]
    public async Task RequestHandlerWithResponse_HandleAsync_Should_BeCallableAndReturnResponse()
    {
        // Arrange
        var handler = new TestRequestWithResponseHandler();
        var request = new TestRequestWithResponse { Value = "test" };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.Equal("Response: test", result);
    }

    [Fact]
    public async Task RequestHandler_HandleAsync_Should_AcceptCancellationToken()
    {
        // Arrange
        var handler = new TestRequestHandler();
        var request = new TestRequest();
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await handler.HandleAsync(request, cancellationToken);
        // If we reach here without exception, the test passes
        Assert.True(true);
    }

    [Fact]
    public async Task RequestHandlerWithResponse_HandleAsync_Should_AcceptCancellationToken()
    {
        // Arrange
        var handler = new TestRequestWithResponseHandler();
        var request = new TestRequestWithResponse { Value = "test" };
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.HandleAsync(request, cancellationToken);

        // Assert
        Assert.Equal("Response: test", result);
    }

    [Fact]
    public async Task RequestHandler_HandleAsync_Should_HandleCancellation()
    {
        // Arrange
        var handler = new CancellableRequestHandler();
        var request = new TestRequest();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => handler.HandleAsync(request, cts.Token));
    }

    // Test classes
    private class TestRequest : IRequest
    {
    }

    private class TestRequestHandler : IRequestHandler<TestRequest>
    {
        public Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class TestRequestWithResponse : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public Task<string> HandleAsync(TestRequestWithResponse request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"Response: {request.Value}");
        }
    }

    private class CancellableRequestHandler : IRequestHandler<TestRequest>
    {
        public Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}