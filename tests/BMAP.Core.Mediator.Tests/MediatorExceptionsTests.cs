using BMAP.Core.Mediator.Exceptions;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for custom mediator exceptions.
/// </summary>
public class MediatorExceptionsTests
{
    [Fact]
    public void HandlerNotFoundException_Should_SetRequestTypeAndMessage()
    {
        // Arrange
        var requestType = typeof(string);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal("No handler found for request type 'String'.", exception.Message);
    }

    [Fact]
    public void HandlerNotFoundException_WithCustomMessage_Should_SetRequestTypeAndCustomMessage()
    {
        // Arrange
        var requestType = typeof(string);
        var customMessage = "Custom error message";

        // Act
        var exception = new HandlerNotFoundException(requestType, customMessage);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public void HandlerNotFoundException_WithInnerException_Should_SetAllProperties()
    {
        // Arrange
        var requestType = typeof(string);
        var customMessage = "Custom error message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new HandlerNotFoundException(requestType, customMessage, innerException);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(customMessage, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void MultipleHandlersFoundException_Should_SetRequestTypeHandlerCountAndMessage()
    {
        // Arrange
        var requestType = typeof(string);
        var handlerCount = 3;

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Equal("Multiple handlers (3) found for request type 'String'. Expected exactly one handler.",
            exception.Message);
    }

    [Fact]
    public void MultipleHandlersFoundException_WithCustomMessage_Should_SetAllPropertiesWithCustomMessage()
    {
        // Arrange
        var requestType = typeof(string);
        var handlerCount = 3;
        var customMessage = "Custom error message";

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount, customMessage);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public void MultipleHandlersFoundException_WithInnerException_Should_SetAllProperties()
    {
        // Arrange
        var requestType = typeof(string);
        var handlerCount = 5;
        var customMessage = "Custom error message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount, customMessage, innerException);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Equal(customMessage, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void MediatorException_Should_SetMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new MediatorException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void MediatorException_WithInnerException_Should_SetMessageAndInnerException()
    {
        // Arrange
        var message = "Test error message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new MediatorException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void MediatorException_Should_BeException()
    {
        // Arrange
        var exception = new MediatorException("Test");

        // Act
        var isException = exception is Exception;

        // Assert
        Assert.True(isException);
    }

    [Fact]
    public void HandlerNotFoundException_Should_BeException()
    {
        // Arrange
        var exception = new HandlerNotFoundException(typeof(string));

        // Act
        var isException = exception is Exception;
        var isMediatorException = exception is MediatorException;

        // Assert
        Assert.True(isException);
        Assert.True(isMediatorException);
    }

    [Fact]
    public void MultipleHandlersFoundException_Should_BeException()
    {
        // Arrange
        var exception = new MultipleHandlersFoundException(typeof(string), 2);

        // Act
        var isException = exception is Exception;
        var isMediatorException = exception is MediatorException;

        // Assert
        Assert.True(isException);
        Assert.True(isMediatorException);
    }
}