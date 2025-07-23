using BMAP.Core.Result;
using BMAP.Core.Result.Utilities;

namespace BMAP.Core.Result.Tests.Utilities;

/// <summary>
/// Unit tests for ResultUtilities class.
/// Tests cover exception handling, validation, null checking, and result aggregation utilities.
/// </summary>
public class ResultUtilitiesTests
{
    #region Try Tests

    [Fact]
    public void Try_Action_Success_Should_Return_Success()
    {
        // Arrange
        var executed = false;

        // Act
        var result = ResultUtilities.Try(() => executed = true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(executed);
    }

    [Fact]
    public void Try_Action_Exception_Should_Return_Failure()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = ResultUtilities.Try(() => throw exception);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Exception.Caught", result.Error!.Code);
        Assert.Equal("Test exception", result.Error.Message);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
        Assert.True(result.Error.Metadata.ContainsKey("ExceptionType"));
        Assert.Equal("InvalidOperationException", result.Error.Metadata["ExceptionType"]);
    }

    [Fact]
    public void Try_Action_With_Custom_Error_Mapper_Should_Use_Mapper()
    {
        // Arrange
        var customError = new Error("CUSTOM.EXCEPTION", "Custom mapped error");
        
        // Act
        var result = ResultUtilities.Try(
            () => throw new ArgumentException("Original message"),
            ex => customError);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(customError, result.Error);
    }

    [Fact]
    public void Try_Function_Success_Should_Return_Success_With_Value()
    {
        // Act
        var result = ResultUtilities.Try(() => "Success value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Success value", result.Value);
    }

    [Fact]
    public void Try_Function_Exception_Should_Return_Failure()
    {
        // Act
        var result = ResultUtilities.Try<string>(() => throw new ArgumentNullException("param"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("param", result.Error!.Message);
        Assert.Equal("ArgumentNullException", result.Error.Metadata["ExceptionType"]);
    }

    [Fact]
    public async Task TryAsync_Action_Success_Should_Return_Success()
    {
        // Arrange
        var executed = false;

        // Act
        var result = await ResultUtilities.TryAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(executed);
    }

    [Fact]
    public async Task TryAsync_Action_Exception_Should_Return_Failure()
    {
        // Act
        var result = await ResultUtilities.TryAsync(() => 
            Task.FromException(new TimeoutException("Async timeout")));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Async timeout", result.Error!.Message);
        Assert.Equal("TimeoutException", result.Error.Metadata["ExceptionType"]);
    }

    [Fact]
    public async Task TryAsync_Function_Success_Should_Return_Success_With_Value()
    {
        // Act
        var result = await ResultUtilities.TryAsync(() => Task.FromResult(42));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TryAsync_Function_Exception_Should_Return_Failure()
    {
        // Act
        var result = await ResultUtilities.TryAsync<string>(() => 
            Task.FromException<string>(new HttpRequestException("Network error")));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Network error", result.Error!.Message);
        Assert.Equal("HttpRequestException", result.Error.Metadata["ExceptionType"]);
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_True_Condition_Should_Return_Success()
    {
        // Act
        var result = ResultUtilities.Validate(true, "TestField", "TestValue", "Should be valid");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_False_Condition_Should_Return_Validation_Failure()
    {
        // Act
        var result = ResultUtilities.Validate(false, "Email", "invalid-email", "Must contain @ symbol");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Email", result.Error!.Code);
        Assert.Contains("Email", result.Error.Message);
        Assert.Contains("Must contain @ symbol", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Email", result.Error.Metadata["FieldName"]);
        Assert.Equal("invalid-email", result.Error.Metadata["ActualValue"]);
        Assert.Equal("Must contain @ symbol", result.Error.Metadata["ExpectedCriteria"]);
    }

    [Fact]
    public void Validate_Value_Valid_Should_Return_Success_With_Value()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        var result = ResultUtilities.Validate(email, e => e.Contains("@"), "Email", "Must contain @ symbol");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value);
    }

    [Fact]
    public void Validate_Value_Invalid_Should_Return_Validation_Failure()
    {
        // Arrange
        const int age = 15;

        // Act
        var result = ResultUtilities.Validate(age, a => a >= 18, "Age", "Must be 18 or older");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Age", result.Error!.Code);
        Assert.Contains("Age", result.Error.Message);
        Assert.Contains("Must be 18 or older", result.Error.Message);
        Assert.Equal("Age", result.Error.Metadata["FieldName"]);
        Assert.Equal("15", result.Error.Metadata["ActualValue"]);
    }

    [Fact]
    public void Validate_Null_Value_Should_Handle_Gracefully()
    {
        // Act
        var result = ResultUtilities.Validate(false, "NullField", null, "Cannot be null");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("null", result.Error!.Metadata["ActualValue"]);
    }

    #endregion

    #region NotNull Tests

    [Fact]
    public void NotNull_Reference_Type_With_Value_Should_Return_Success()
    {
        // Arrange
        const string value = "Not null";

        // Act
        var result = ResultUtilities.NotNull(value, "String", "test-id");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void NotNull_Reference_Type_With_Null_Should_Return_NotFound_Failure()
    {
        // Act
        var result = ResultUtilities.NotNull((string?)null, "User", 123);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound.User", result.Error!.Code);
        Assert.Contains("User", result.Error.Message);
        Assert.Contains("123", result.Error.Message);
        Assert.Contains("not found", result.Error.Message);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("User", result.Error.Metadata["ResourceName"]);
        Assert.Equal("123", result.Error.Metadata["Identifier"]);
    }

    [Fact]
    public void NotNull_Nullable_Value_Type_With_Value_Should_Return_Success()
    {
        // Arrange
        int? value = 42;

        // Act
        var result = ResultUtilities.NotNull(value, "Number", "test-id");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void NotNull_Nullable_Value_Type_With_Null_Should_Return_NotFound_Failure()
    {
        // Act
        var result = ResultUtilities.NotNull((DateTime?)null, "Timestamp", "event-123");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound.Timestamp", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Timestamp", result.Error.Metadata["ResourceName"]);
        Assert.Equal("event-123", result.Error.Metadata["Identifier"]);
    }

    [Fact]
    public void NotEmpty_Collection_With_Items_Should_Return_Success()
    {
        // Arrange
        var collection = new[] { 1, 2, 3 };

        // Act
        var result = ResultUtilities.NotEmpty(collection, "Numbers");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(collection, result.Value);
    }

    [Fact]
    public void NotEmpty_Empty_Collection_Should_Return_NotFound_Failure()
    {
        // Arrange
        var emptyCollection = Array.Empty<string>();

        // Act
        var result = ResultUtilities.NotEmpty(emptyCollection, "Items");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("NotFound.Items", result.Error!.Code);
        Assert.Contains("No Items were found", result.Error.Message);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("Items", result.Error.Metadata["ResourceName"]);
        Assert.Equal("String", result.Error.Metadata["CollectionType"]);
    }

    #endregion

    #region AggregateValidations Tests

    [Fact]
    public void AggregateValidations_All_Success_Should_Return_Success()
    {
        // Arrange
        var validations = new[]
        {
            Result.Success(),
            Result.Success(),
            Result.Success()
        };

        // Act
        var result = ResultUtilities.AggregateValidations(validations);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AggregateValidations_Single_Failure_Should_Return_That_Failure()
    {
        // Arrange
        var error = new Error("SINGLE.FAIL", "Single failure");
        var validations = new[]
        {
            Result.Success(),
            Result.Failure(error),
            Result.Success()
        };

        // Act
        var result = ResultUtilities.AggregateValidations(validations);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void AggregateValidations_Multiple_Failures_Should_Return_Aggregated_Error()
    {
        // Arrange
        var error1 = new Error("FAIL.1", "First failure");
        var error2 = new Error("FAIL.2", "Second failure");
        var validations = new[]
        {
            Result.Success(),
            Result.Failure(error1),
            Result.Failure(error2)
        };

        // Act
        var result = ResultUtilities.AggregateValidations(validations);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Multiple", result.Error!.Code);
        Assert.Contains("First failure", result.Error.Message);
        Assert.Contains("Second failure", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(2, result.Error.Metadata["ErrorCount"]);
    }

    [Fact]
    public void AggregateValidations_No_Validations_Should_Return_Success()
    {
        // Act
        var result = ResultUtilities.AggregateValidations();

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion

    #region EnsureAll Tests

    [Fact]
    public void EnsureAll_All_Validations_Pass_Should_Return_Success()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        var result = ResultUtilities.EnsureAll(email,
            (e => !string.IsNullOrWhiteSpace(e), "Email is required"),
            (e => e.Contains("@"), "Email must contain @"),
            (e => e.Length <= 100, "Email must be 100 characters or less")
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value);
    }

    [Fact]
    public void EnsureAll_Some_Validations_Fail_Should_Return_Aggregated_Failure()
    {
        // Arrange
        const string email = "invalid";

        // Act
        var result = ResultUtilities.EnsureAll(email,
            (e => !string.IsNullOrWhiteSpace(e), "Email is required"),
            (e => e.Contains("@"), "Email must contain @"),
            (e => e.Length >= 100, "Email must be at least 100 characters")
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Multiple", result.Error!.Code);
        Assert.Contains("Email must contain @", result.Error.Message);
        Assert.Contains("Email must be at least 100 characters", result.Error.Message);
        Assert.Equal("invalid", result.Error.Metadata["Value"]);
    }

    [Fact]
    public void EnsureAll_No_Validations_Should_Return_Success()
    {
        // Act
        var result = ResultUtilities.EnsureAll("anything");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("anything", result.Value);
    }

    [Fact]
    public void EnsureAll_Null_Value_Should_Handle_Gracefully()
    {
        // Act
        var result = ResultUtilities.EnsureAll((string?)null,
            (s => s != null, "Must not be null")
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("null", result.Error!.Metadata["Value"]);
    }

    #endregion

    #region Complex Scenarios Tests

    [Fact]
    public void Complex_Validation_Scenario_Should_Work()
    {
        // Arrange
        var user = new TestUser("", "invalid-email", -5);

        // Act
        var nameValidation = ResultUtilities.Validate(!string.IsNullOrWhiteSpace(user.Name), "Name", user.Name, "Name is required");
        var emailValidation = ResultUtilities.Validate(user.Email.Contains("@"), "Email", user.Email, "Email must contain @");
        var ageValidation = ResultUtilities.Validate(user.Age >= 0, "Age", user.Age, "Age must be non-negative");

        var aggregatedResult = ResultUtilities.AggregateValidations(nameValidation, emailValidation, ageValidation);

        // Assert
        Assert.True(aggregatedResult.IsFailure);
        Assert.Equal("Validation.Multiple", aggregatedResult.Error!.Code);
        Assert.Contains("Name is required", aggregatedResult.Error.Message);
        Assert.Contains("Email must contain @", aggregatedResult.Error.Message);
        Assert.Contains("Age must be non-negative", aggregatedResult.Error.Message);
    }

    [Fact]
    public void Exception_Handling_With_Custom_Mapper_Should_Work()
    {
        // Arrange
        Error CustomMapper(Exception ex) => ex switch
        {
            ArgumentNullException => Error.Validation("ARG.NULL", "Argument was null"),
            InvalidOperationException => Error.Conflict("INVALID.OP", "Invalid operation"),
            _ => Error.Internal("UNKNOWN", "Unknown error occurred")
        };

        // Act
        var result1 = ResultUtilities.Try(() => throw new ArgumentNullException("param"), CustomMapper);
        var result2 = ResultUtilities.Try(() => throw new InvalidOperationException("invalid"), CustomMapper);
        var result3 = ResultUtilities.Try(() => throw new NotSupportedException("unsupported"), CustomMapper);

        // Assert
        Assert.Equal("ARG.NULL", result1.Error!.Code);
        Assert.Equal(ErrorType.Validation, result1.Error.Type);

        Assert.Equal("INVALID.OP", result2.Error!.Code);
        Assert.Equal(ErrorType.Conflict, result2.Error.Type);

        Assert.Equal("UNKNOWN", result3.Error!.Code);
        Assert.Equal(ErrorType.Internal, result3.Error.Type);
    }

    #endregion

    /// <summary>
    /// Test helper class for complex validation scenarios
    /// </summary>
    private record TestUser(string Name, string Email, int Age);
}