using BMAP.Core.Result;

namespace BMAP.Core.Result.Tests;

/// <summary>
/// Unit tests for the Result class (non-generic version).
/// Tests cover creation, state validation, implicit conversions, and edge cases.
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_Should_Create_Successful_Result()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_With_Error_Should_Create_Failed_Result()
    {
        // Arrange
        var error = new Error("TEST.001", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_With_Message_Should_Create_Failed_Result_With_General_Error()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var result = Result.Failure(message);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal("General.Failure", result.Error.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Failure_With_Code_And_Message_Should_Create_Failed_Result_With_Custom_Error()
    {
        // Arrange
        const string code = "CUSTOM.001";
        const string message = "Custom error message";

        // Act
        var result = Result.Failure(code, message);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(code, result.Error.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Create_With_True_Should_Return_Success()
    {
        // Act
        var result = Result.Create(true);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Create_With_False_Should_Return_Failure_With_Default_Error()
    {
        // Act
        var result = Result.Create(false);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("General.Failure", result.Error.Code);
        Assert.Equal("Operation failed", result.Error.Message);
    }

    [Fact]
    public void Create_With_False_And_Custom_Error_Should_Return_Failure_With_Custom_Error()
    {
        // Arrange
        var customError = new Error("CUSTOM.002", "Custom failure message");

        // Act
        var result = Result.Create(false, customError);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(customError, result.Error);
    }

    [Fact]
    public void CreateIf_With_True_Condition_Should_Return_Success()
    {
        // Arrange
        var error = new Error("TEST.003", "Test error");

        // Act
        var result = Result.CreateIf(true, error);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void CreateIf_With_False_Condition_Should_Return_Failure()
    {
        // Arrange
        var error = new Error("TEST.004", "Test error");

        // Act
        var result = Result.CreateIf(false, error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void CreateIf_With_False_Condition_And_String_Message_Should_Return_Failure()
    {
        // Arrange
        const string errorMessage = "Condition failed";

        // Act
        var result = Result.CreateIf(false, errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("General.Failure", result.Error.Code);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public void CreateIf_With_False_Condition_And_Code_Message_Should_Return_Failure()
    {
        // Arrange
        const string errorCode = "VALIDATION.001";
        const string errorMessage = "Validation failed";

        // Act
        var result = Result.CreateIf(false, errorCode, errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(errorCode, result.Error.Code);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public void Implicit_Conversion_From_Error_Should_Create_Failed_Result()
    {
        // Arrange
        var error = new Error("TEST.005", "Test error");

        // Act
        Result result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsFailure_Should_Be_Opposite_Of_IsSuccess(bool isSuccess)
    {
        // Arrange & Act
        var result = isSuccess ? Result.Success() : Result.Failure("Test error");

        // Assert
        Assert.Equal(isSuccess, result.IsSuccess);
        Assert.Equal(!isSuccess, result.IsFailure);
    }

    [Fact]
    public void Two_Successful_Results_Should_Be_Considered_Equal_Conceptually()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();

        // Assert
        Assert.Equal(result1.IsSuccess, result2.IsSuccess);
        Assert.Equal(result1.IsFailure, result2.IsFailure);
        // Note: Result instances are not reference equal, but they represent the same state
    }

    [Fact]
    public void Multiple_Failures_With_Same_Error_Should_Have_Same_Error()
    {
        // Arrange
        var error = new Error("TEST.006", "Same error");

        // Act
        var result1 = Result.Failure(error);
        var result2 = Result.Failure(error);

        // Assert
        Assert.Equal(result1.Error, result2.Error);
    }

    [Fact]
    public void Result_Should_Be_Immutable()
    {
        // Arrange
        var originalError = new Error("TEST.007", "Original error");
        var result = Result.Failure(originalError);

        // Act - Try to modify (this should not be possible due to readonly properties)
        var retrievedError = result.Error;

        // Assert
        Assert.Equal(originalError, retrievedError);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ResultBase_Implicit_Conversion_Should_Work()
    {
        // Arrange
        var error = new Error("TEST.008", "Base conversion test");

        // Act
        ResultBase resultBase = error;

        // Assert
        Assert.NotNull(resultBase);
        Assert.False(resultBase.IsSuccess);
        Assert.True(resultBase.IsFailure);
        Assert.Equal(error, resultBase.Error);
    }
}