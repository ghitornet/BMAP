using BMAP.Core.Mediator.Behaviors;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for validation behaviors and related classes.
/// </summary>
public class ValidationBehaviorTests
{
    [Fact]
    public void ValidationResult_Success_Should_CreateSuccessfulResult()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithErrors_Should_CreateFailureResult()
    {
        // Arrange
        var error1 = new ValidationError("Error 1");
        var error2 = new ValidationError("Error 2");

        // Act
        var result = ValidationResult.Failure(error1, error2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
        Assert.Contains(error1, result.Errors);
        Assert.Contains(error2, result.Errors);
    }

    [Fact]
    public void ValidationResult_Failure_WithMessages_Should_CreateFailureResultWithErrorMessages()
    {
        // Arrange
        var message1 = "Error message 1";
        var message2 = "Error message 2";

        // Act
        var result = ValidationResult.Failure(message1, message2);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
        Assert.Contains(result.Errors, e => e.Message == message1);
        Assert.Contains(result.Errors, e => e.Message == message2);
    }

    [Fact]
    public void ValidationError_Constructor_Should_SetMessageAndPropertyName()
    {
        // Arrange
        var message = "Test error message";
        var propertyName = "TestProperty";

        // Act
        var error = new ValidationError(message, propertyName);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Equal(propertyName, error.PropertyName);
    }

    [Fact]
    public void ValidationError_Constructor_WithoutPropertyName_Should_SetMessageOnly()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var error = new ValidationError(message);

        // Assert
        Assert.Equal(message, error.Message);
        Assert.Null(error.PropertyName);
    }

    [Fact]
    public void ValidationException_Constructor_Should_SetErrorsAndDefaultMessage()
    {
        // Arrange
        var errors = new[]
        {
            new ValidationError("Error 1"),
            new ValidationError("Error 2")
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.Equal(errors, exception.Errors);
        Assert.Equal("One or more validation errors occurred.", exception.Message);
    }

    [Fact]
    public void ValidationException_Constructor_WithCustomMessage_Should_SetErrorsAndCustomMessage()
    {
        // Arrange
        var customMessage = "Custom validation error message";
        var errors = new[]
        {
            new ValidationError("Error 1"),
            new ValidationError("Error 2")
        };

        // Act
        var exception = new ValidationException(customMessage, errors);

        // Assert
        Assert.Equal(errors, exception.Errors);
        Assert.Equal(customMessage, exception.Message);
    }

    [Fact]
    public async Task ValidationBehavior_WithResponse_Should_CallNext_WhenNoValidatorsRegistered()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequestWithResponse>>();
        var logger = MockLoggerHelper.CreateNullLogger<ValidationBehavior<TestRequestWithResponse, string>>();
        var behavior = new ValidationBehavior<TestRequestWithResponse, string>(validators, logger);
        var request = new TestRequestWithResponse();
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Response");
        }

        // Act
        var result = await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Response", result);
    }

    [Fact]
    public async Task ValidationBehavior_WithResponse_Should_CallNext_WhenValidationSucceeds()
    {
        // Arrange
        var validator = new TestValidatorWithResponse(ValidationResult.Success());
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<ValidationBehavior<TestRequestWithResponse, string>>();
        var behavior = new ValidationBehavior<TestRequestWithResponse, string>(validators, logger);
        var request = new TestRequestWithResponse();
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Response");
        }

        // Act
        var result = await behavior.HandleAsync(request, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Response", result);
        Assert.True(validator.WasCalled);
    }

    [Fact]
    public async Task ValidationBehavior_WithResponse_Should_ThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var error = new ValidationError("Validation error");
        var validator = new TestValidatorWithResponse(ValidationResult.Failure(error));
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<ValidationBehavior<TestRequestWithResponse, string>>();
        var behavior = new ValidationBehavior<TestRequestWithResponse, string>(validators, logger);
        var request = new TestRequestWithResponse();
        var nextCalled = false;
        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Response");
        }

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(request, Next));
        Assert.False(nextCalled);
        Assert.True(validator.WasCalled);
    }

    [Fact]
    public async Task ValidationBehavior_WithoutResponse_Should_CallNext_WhenNoValidatorsRegistered()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var logger = MockLoggerHelper.CreateNullLogger<ValidationBehavior<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest>(validators, logger);
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
    public async Task ValidationBehavior_WithoutResponse_Should_CallNext_WhenValidationSucceeds()
    {
        // Arrange
        var validator = new TestValidator(ValidationResult.Success());
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<ValidationBehavior<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest>(validators, logger);
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
        Assert.True(validator.WasCalled);
    }

    [Fact]
    public async Task ValidationBehavior_WithoutResponse_Should_ThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var error = new ValidationError("Validation error");
        var validator = new TestValidator(ValidationResult.Failure(error));
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<ValidationBehavior<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest>(validators, logger);
        var request = new TestRequest();
        var nextCalled = false;
        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(request, Next));
        Assert.False(nextCalled);
        Assert.True(validator.WasCalled);
    }

    // Test classes
    private class TestRequest : IRequest
    {
    }

    private class TestRequestWithResponse : IRequest<string>
    {
    }

    private class TestValidator(ValidationResult result) : IValidator<TestRequest>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    private class TestValidatorWithResponse(ValidationResult result) : IValidator<TestRequestWithResponse>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestRequestWithResponse request,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }
}