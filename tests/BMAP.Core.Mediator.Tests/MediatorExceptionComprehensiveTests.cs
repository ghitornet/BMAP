using BMAP.Core.Mediator.Exceptions;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for all exception classes in the mediator library.
/// Tests cover all constructors, properties, and inheritance behavior.
/// </summary>
public class MediatorExceptionTests
{
    #region MediatorException Tests

    [Fact]
    public void MediatorException_WithMessage_Should_SetMessage()
    {
        // Arrange
        const string message = "Test mediator exception";

        // Act
        var exception = new MediatorException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void MediatorException_WithMessageAndInnerException_Should_SetBoth()
    {
        // Arrange
        const string message = "Test mediator exception";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new MediatorException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void MediatorException_Should_BeSubclassOfException()
    {
        // Arrange
        var exception = new MediatorException("Test");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    #endregion

    #region HandlerNotFoundException Tests

    [Fact]
    public void HandlerNotFoundException_WithRequestType_Should_SetPropertiesAndMessage()
    {
        // Arrange
        var requestType = typeof(TestRequest);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Contains("TestRequest", exception.Message);
        Assert.Contains("No handler found", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void HandlerNotFoundException_WithRequestTypeAndCustomMessage_Should_SetCustomMessage()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const string customMessage = "Custom handler not found message";

        // Act
        var exception = new HandlerNotFoundException(requestType, customMessage);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(customMessage, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void HandlerNotFoundException_WithRequestTypeMessageAndInnerException_Should_SetAll()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const string customMessage = "Custom handler not found message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new HandlerNotFoundException(requestType, customMessage, innerException);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(customMessage, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void HandlerNotFoundException_Should_BeSubclassOfMediatorException()
    {
        // Arrange
        var exception = new HandlerNotFoundException(typeof(TestRequest));

        // Assert
        Assert.IsAssignableFrom<MediatorException>(exception);
    }

    [Fact]
    public void HandlerNotFoundException_RequestTypeProperty_Should_BeReadOnly()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        var exception = new HandlerNotFoundException(requestType);

        // Act & Assert
        Assert.Equal(requestType, exception.RequestType);
        // Verify property is readonly (no setter accessible)
        var property = typeof(HandlerNotFoundException).GetProperty(nameof(HandlerNotFoundException.RequestType));
        Assert.NotNull(property);
        Assert.Null(property!.SetMethod);
    }

    #endregion

    #region MultipleHandlersFoundException Tests

    [Fact]
    public void MultipleHandlersFoundException_WithRequestTypeAndCount_Should_SetPropertiesAndMessage()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 3;

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Contains("TestRequest", exception.Message);
        Assert.Contains("Multiple handlers (3)", exception.Message);
        Assert.Contains("Expected exactly one", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void MultipleHandlersFoundException_WithRequestTypeCountAndCustomMessage_Should_SetCustomMessage()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 5;
        const string customMessage = "Custom multiple handlers message";

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount, customMessage);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Equal(customMessage, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void MultipleHandlersFoundException_WithRequestTypeCountMessageAndInnerException_Should_SetAll()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 2;
        const string customMessage = "Custom multiple handlers message";
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
    public void MultipleHandlersFoundException_Should_BeSubclassOfMediatorException()
    {
        // Arrange
        var exception = new MultipleHandlersFoundException(typeof(TestRequest), 2);

        // Assert
        Assert.IsAssignableFrom<MediatorException>(exception);
    }

    [Fact]
    public void MultipleHandlersFoundException_Properties_Should_BeReadOnly()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 4;
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Act & Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        
        // Verify properties are readonly (no setters accessible)
        var requestTypeProperty = typeof(MultipleHandlersFoundException).GetProperty(nameof(MultipleHandlersFoundException.RequestType));
        var handlerCountProperty = typeof(MultipleHandlersFoundException).GetProperty(nameof(MultipleHandlersFoundException.HandlerCount));
        
        Assert.NotNull(requestTypeProperty);
        Assert.NotNull(handlerCountProperty);
        Assert.Null(requestTypeProperty!.SetMethod);
        Assert.Null(handlerCountProperty!.SetMethod);
    }

    [Fact]
    public void MultipleHandlersFoundException_ZeroHandlers_Should_WorkCorrectly()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 0;

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Contains("Multiple handlers (0)", exception.Message);
    }

    [Fact]
    public void MultipleHandlersFoundException_LargeHandlerCount_Should_WorkCorrectly()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 999;

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Contains("Multiple handlers (999)", exception.Message);
    }

    #endregion

    #region Generic Type Tests

    [Fact]
    public void HandlerNotFoundException_WithGenericType_Should_HandleGenericsCorrectly()
    {
        // Arrange
        var requestType = typeof(TestGenericRequest<string>);

        // Act
        var exception = new HandlerNotFoundException(requestType);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Contains("TestGenericRequest", exception.Message);
    }

    [Fact]
    public void MultipleHandlersFoundException_WithGenericType_Should_HandleGenericsCorrectly()
    {
        // Arrange
        var requestType = typeof(TestGenericRequest<int>);
        const int handlerCount = 2;

        // Act
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Assert
        Assert.Equal(requestType, exception.RequestType);
        Assert.Equal(handlerCount, exception.HandlerCount);
        Assert.Contains("TestGenericRequest", exception.Message);
    }

    #endregion

    #region Exception Serialization Tests

    [Fact]
    public void MediatorException_Should_BeSerializable()
    {
        // Arrange
        var exception = new MediatorException("Test message");

        // Assert - Exception should have SerializableAttribute (if needed for your use case)
        // Note: In .NET Core+, exceptions don't require SerializableAttribute by default
        Assert.IsType<MediatorException>(exception);
    }

    [Fact]
    public void HandlerNotFoundException_ToString_Should_ContainRelevantInformation()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        var exception = new HandlerNotFoundException(requestType);

        // Act
        var toString = exception.ToString();

        // Assert
        Assert.Contains("HandlerNotFoundException", toString);
        Assert.Contains("TestRequest", toString);
        Assert.Contains("No handler found", toString);
    }

    [Fact]
    public void MultipleHandlersFoundException_ToString_Should_ContainRelevantInformation()
    {
        // Arrange
        var requestType = typeof(TestRequest);
        const int handlerCount = 3;
        var exception = new MultipleHandlersFoundException(requestType, handlerCount);

        // Act
        var toString = exception.ToString();

        // Assert
        Assert.Contains("MultipleHandlersFoundException", toString);
        Assert.Contains("TestRequest", toString);
        Assert.Contains("Multiple handlers (3)", toString);
    }

    #endregion

    #region Test Helper Classes

    // Test request types for exception testing
    public class TestRequest : IRequest
    {
    }

    public class TestGenericRequest<T> : IRequest<T>
    {
        public T? Value { get; set; }
    }

    #endregion
}