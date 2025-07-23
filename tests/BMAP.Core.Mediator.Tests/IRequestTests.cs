namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for IRequest and IRequest<TResponse> interfaces.
/// </summary>
public class RequestTests
{
    [Fact]
    public void IRequest_Should_BeMarkerInterface()
    {
        // Arrange
        var requestType = typeof(IRequest);

        // Act
        var hasMembers = requestType.GetMethods().Length > 0 || requestType.GetProperties().Length > 0;

        // Assert
        Assert.False(hasMembers);
    }

    [Fact]
    public void IRequestWithResponse_Should_InheritFromIRequest()
    {
        // Arrange
        var requestWithResponseType = typeof(IRequest<>);

        // Act
        var implementsIRequest = typeof(IRequest).IsAssignableFrom(requestWithResponseType);

        // Assert
        Assert.True(implementsIRequest);
    }

    [Fact]
    public void ConcreteRequest_Should_ImplementIRequest()
    {
        // Arrange
        var concreteRequest = new TestRequest();

        // Act
        var isRequest = concreteRequest is IRequest;

        // Assert
        Assert.True(isRequest);
    }

    [Fact]
    public void ConcreteRequestWithResponse_Should_ImplementIRequestWithResponse()
    {
        // Arrange
        var concreteRequest = new TestRequestWithResponse();

        // Act
        var isRequest = concreteRequest is IRequest<string>;
        var isBaseRequest = concreteRequest is IRequest;

        // Assert
        Assert.True(isRequest);
        Assert.True(isBaseRequest);
    }

    // Test classes
    private class TestRequest : IRequest
    {
    }

    private class TestRequestWithResponse : IRequest<string>
    {
    }
}