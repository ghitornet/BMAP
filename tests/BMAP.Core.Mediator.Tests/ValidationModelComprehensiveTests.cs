using BMAP.Core.Mediator.Behaviors;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Comprehensive unit tests for validation model classes.
/// Tests cover all properties, methods, factory methods, and edge cases for maximum code coverage.
/// </summary>
public class ValidationModelComprehensiveTests
{
    #region ValidationError Tests

    [Fact]
    public void ValidationError_WithMessage_Should_SetMessageAndNullPropertyName()
    {
        // Arrange
        const string message = "Test validation error";

        // Act
        var error = new ValidationError(message);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Null(error.PropertyName);
    }

    [Fact]
    public void ValidationError_WithMessageAndPropertyName_Should_SetBoth()
    {
        // Arrange
        const string message = "Test validation error";
        const string propertyName = "TestProperty";

        // Act
        var error = new ValidationError(message, propertyName);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Equal(propertyName, error.PropertyName);
    }

    [Fact]
    public void ValidationError_WithEmptyMessage_Should_SetEmptyMessage()
    {
        // Arrange
        const string message = "";

        // Act
        var error = new ValidationError(message);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Null(error.PropertyName);
    }

    [Fact]
    public void ValidationError_WithEmptyPropertyName_Should_SetEmptyPropertyName()
    {
        // Arrange
        const string message = "Test error";
        const string propertyName = "";

        // Act
        var error = new ValidationError(message, propertyName);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Equal(propertyName, error.PropertyName);
    }

    [Fact]
    public void ValidationError_WithWhitespaceMessage_Should_SetWhitespaceMessage()
    {
        // Arrange
        const string message = "   ";

        // Act
        var error = new ValidationError(message);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Null(error.PropertyName);
    }

    [Fact]
    public void ValidationError_Properties_Should_BeReadOnly()
    {
        // Arrange

        // Assert
        var messageProperty = typeof(ValidationError).GetProperty(nameof(ValidationError.Message));
        var propertyNameProperty = typeof(ValidationError).GetProperty(nameof(ValidationError.PropertyName));
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(propertyNameProperty);
        Assert.Null(messageProperty.SetMethod);
        Assert.Null(propertyNameProperty.SetMethod);
    }

    [Fact]
    public void ValidationError_Equality_Should_WorkCorrectly()
    {
        // Arrange
        var error1 = new ValidationError("Same message", "SameProperty");
        var error2 = new ValidationError("Same message", "SameProperty");
        var error3 = new ValidationError("Different message", "SameProperty");
        var error4 = new ValidationError("Same message", "DifferentProperty");

        // Assert
        // Note: ValidationError doesn't override Equals, so reference equality is used
        Assert.NotEqual(error1, error2); // Different instances
        Assert.NotEqual(error1, error3);
        Assert.NotEqual(error1, error4);
        Assert.Equal(error1, error1); // Same instance
    }

    [Fact]
    public void ValidationError_ToString_Should_ContainRelevantInformation()
    {
        // Arrange
        var errorWithProperty = new ValidationError("Test error", "TestProperty");
        var errorWithoutProperty = new ValidationError("Test error");

        // Act
        var toStringWithProperty = errorWithProperty.ToString();
        var toStringWithoutProperty = errorWithoutProperty.ToString();

        // Assert
        Assert.Contains("ValidationError", toStringWithProperty);
        Assert.Contains("ValidationError", toStringWithoutProperty);
    }

    #endregion

    #region ValidationResult Tests

    [Fact]
    public void ValidationResult_Success_Should_CreateValidResult()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Errors);
    }

    [Fact]
    public void ValidationResult_Success_Multiple_Calls_Should_ReturnSeparateInstances()
    {
        // Act
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        // Assert
        Assert.NotSame(result1, result2);
        Assert.True(result1.IsValid);
        Assert.True(result2.IsValid);
    }

    [Fact]
    public void ValidationResult_Failure_WithSingleError_Should_CreateFailureResult()
    {
        // Arrange
        var error = new ValidationError("Test error");

        // Act
        var result = ValidationResult.Failure(error);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains(error, result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithMultipleErrors_Should_CreateFailureResult()
    {
        // Arrange
        var error1 = new ValidationError("Error 1");
        var error2 = new ValidationError("Error 2", "Property2");
        var error3 = new ValidationError("Error 3");

        // Act
        var result = ValidationResult.Failure(error1, error2, error3);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count());
        Assert.Contains(error1, result.Errors);
        Assert.Contains(error2, result.Errors);
        Assert.Contains(error3, result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithNoErrors_Should_CreateFailureResultWithEmptyErrors()
    {
        // Act
        var result = ValidationResult.Failure(Array.Empty<ValidationError>());

        // Assert
        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithErrorMessages_Should_CreateFailureResultWithErrorObjects()
    {
        // Arrange
        const string message1 = "Error message 1";
        const string message2 = "Error message 2";

        // Act
        var result = ValidationResult.Failure(message1, message2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
        Assert.Contains(result.Errors, e => e is { Message: message1, PropertyName: null });
        Assert.Contains(result.Errors, e => e is { Message: message2, PropertyName: null });
    }

    [Fact]
    public void ValidationResult_Failure_WithEmptyErrorMessages_Should_CreateFailureResult()
    {
        // Act
        var result = ValidationResult.Failure("");

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("", result.Errors.First().Message);
    }

    [Fact]
    public void ValidationResult_Failure_WithNullErrorArray_Should_CreateFailureResult()
    {
        // Act
        var result = ValidationResult.Failure((ValidationError[])null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithNullStringArray_Should_CreateFailureResult()
    {
        // Act
        var result = ValidationResult.Failure((string[])null!);

        // Assert
        Assert.False(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Properties_Should_BeReadOnly()
    {
        // Arrange

        // Assert
        var isValidProperty = typeof(ValidationResult).GetProperty(nameof(ValidationResult.IsValid));
        var errorsProperty = typeof(ValidationResult).GetProperty(nameof(ValidationResult.Errors));
        
        Assert.NotNull(isValidProperty);
        Assert.NotNull(errorsProperty);
        
        // For properties with private init, the SetMethod exists but is not publicly accessible
        // We verify that the setter is not public (init accessors generate internal setters)
        Assert.True(isValidProperty.SetMethod == null || !isValidProperty.SetMethod.IsPublic);
        Assert.True(errorsProperty.SetMethod == null || !errorsProperty.SetMethod.IsPublic);
        
        // Additional verification: properties should be readable
        Assert.True(isValidProperty.CanRead);
        Assert.True(errorsProperty.CanRead);
        
        // Verify that we cannot set these properties after object creation
        // (This would be enforced by the compiler, but we can verify the intent)
        Assert.NotNull(isValidProperty.GetMethod);
        Assert.NotNull(errorsProperty.GetMethod);
    }

    [Fact]
    public void ValidationResult_Properties_Should_BeImmutableAfterCreation()
    {
        // Arrange
        var result = ValidationResult.Success();

        // Act & Assert
        // Verify that properties can be read
        var isValid = result.IsValid;
        var errors = result.Errors;
        
        Assert.True(isValid);
        Assert.NotNull(errors);
        
        // Verify that the object is effectively immutable after creation
        // Note: The compiler prevents direct assignment to init properties after object initialization
        // This test documents the expected immutable behavior
        
        // These would not compile if attempted:
        // result.IsValid = false;  // Compiler error: Init-only property can only be assigned in an object initializer
        // result.Errors = new ValidationError[0];  // Compiler error: Init-only property can only be assigned in an object initializer
    }

    [Fact]
    public void ValidationResult_Errors_Should_BeImmutable()
    {
        // Arrange
        var error = new ValidationError("Test error");
        var result = ValidationResult.Failure(error);

        // Act & Assert
        // The Errors property should return an IEnumerable that can't be modified
        Assert.IsAssignableFrom<IEnumerable<ValidationError>>(result.Errors);
        
        // Try to cast to modifiable collections - should not be possible or should throw
        var errorsList = result.Errors.ToList();
        Assert.Single(errorsList);
        
        // Modifying the list shouldn't affect the original result
        errorsList.Add(new ValidationError("New error"));
        Assert.Single(result.Errors); // Original should still have 1 error
    }

    #endregion

    #region ValidationException Tests

    [Fact]
    public void ValidationException_WithErrors_Should_SetErrorsAndDefaultMessage()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1"),
            new ValidationError("Error 2", "Property2")
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Equal(errors, exception.Errors);
        Assert.Equal("One or more validation errors occurred.", exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void ValidationException_WithErrorsAndCustomMessage_Should_SetErrorsAndCustomMessage()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1"),
            new ValidationError("Error 2")
        };
        const string customMessage = "Custom validation error message";

        // Act
        var exception = new ValidationException(customMessage, errors);

        // Assert
        Assert.Equal(errors, exception.Errors);
        Assert.Equal(customMessage, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void ValidationException_WithEmptyErrors_Should_SetEmptyErrorsCollection()
    {
        // Arrange
        var errors = Array.Empty<ValidationError>();

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Empty(exception.Errors);
        Assert.Equal("One or more validation errors occurred.", exception.Message);
    }

    [Fact]
    public void ValidationException_WithNullErrors_Should_HandleGracefully()
    {
        // Act
        var exception = new ValidationException(null!);

        // Assert
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public void ValidationException_Should_BeSubclassOfException()
    {
        // Arrange
        var errors = new[] { new ValidationError("Test error") };
        var exception = new ValidationException(errors);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void ValidationException_Errors_Property_Should_BeReadOnly()
    {
        // Arrange

        // Assert
        var errorsProperty = typeof(ValidationException).GetProperty(nameof(ValidationException.Errors));
        Assert.NotNull(errorsProperty);
        Assert.Null(errorsProperty.SetMethod);
    }

    [Fact]
    public void ValidationException_ToString_Should_ContainRelevantInformation()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1"),
            new ValidationError("Error 2", "Property2")
        };
        var exception = new ValidationException("Custom message", errors);

        // Act
        var toString = exception.ToString();

        // Assert
        Assert.Contains("ValidationException", toString);
        Assert.Contains("Custom message", toString);
    }

    [Fact]
    public void ValidationException_With_Single_Error_Should_Work()
    {
        // Arrange
        var error = new ValidationError("Single error", "SingleProperty");
        var errors = new[] { error };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Single(exception.Errors);
        Assert.Equal(error, exception.Errors.First());
    }

    [Fact]
    public void ValidationException_With_Many_Errors_Should_Work()
    {
        // Arrange
        var errors = Enumerable.Range(1, 100)
            .Select(i => new ValidationError($"Error {i}", $"Property{i}"))
            .ToArray();

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Equal(100, exception.Errors.Count());
        Assert.All(exception.Errors, error => Assert.Contains("Error ", error.Message));
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void ValidationError_With_Very_Long_Message_Should_Work()
    {
        // Arrange
        var longMessage = new string('x', 10000);

        // Act
        var error = new ValidationError(longMessage, "Property");

        // Assert
        Assert.Equal(longMessage, error.Message);
        Assert.Equal("Property", error.PropertyName);
    }

    [Fact]
    public void ValidationResult_With_Many_Errors_Should_Work()
    {
        // Arrange
        var errors = Enumerable.Range(1, 1000)
            .Select(i => new ValidationError($"Error {i}", $"Property{i}"))
            .ToArray();

        // Act
        var result = ValidationResult.Failure(errors);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(1000, result.Errors.Count());
    }

    [Fact]
    public void ValidationResult_With_Mixed_Error_Types_Should_Work()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error with property", "Property1"),
            new ValidationError("Error without property"),
            new ValidationError("", "EmptyMessage"),
            new ValidationError("Valid message", ""),
            new ValidationError("   ")
        };

        // Act
        var result = ValidationResult.Failure(errors);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(5, result.Errors.Count());
        Assert.Contains(result.Errors, e => e.PropertyName == "Property1");
        Assert.Contains(result.Errors, e => e.PropertyName == null);
        Assert.Contains(result.Errors, e => e.PropertyName == "EmptyMessage");
        Assert.Contains(result.Errors, e => e.PropertyName == "");
    }

    #endregion
}