using BMAP.Core.Result;
using System.Diagnostics.CodeAnalysis;

namespace BMAP.Core.Result.Tests;

/// <summary>
/// Unit tests for the Result&lt;T&gt; class (generic version).
/// Tests cover creation, value access, state validation, implicit conversions, and edge cases.
/// </summary>
public class ResultOfTTests
{
    [Fact]
    public void Success_With_Value_Should_Create_Successful_Result()
    {
        // Arrange
        const string value = "Test Value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Success_With_Null_Value_Should_Create_Successful_Result()
    {
        // Act
        var result = Result<string?>.Success(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Failure_With_Error_Should_Create_Failed_Result()
    {
        // Arrange
        var error = new Error("TEST.001", "Test error message");

        // Act
        var result = Result<string>.Failure(error);

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
        var result = Result<int>.Failure(message);

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
        var result = Result<bool>.Failure(code, message);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(code, result.Error.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Value_Property_Should_Return_Value_When_Successful()
    {
        // Arrange
        const int expectedValue = 42;
        var result = Result<int>.Success(expectedValue);

        // Act
        var actualValue = result.Value;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Value_Property_Should_Throw_When_Failed()
    {
        // Arrange
        var result = Result<string>.Failure("Test error");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Cannot access the value of a failed result", exception.Message);
    }

    [Fact]
    public void TryGetValue_Should_Return_True_And_Value_When_Successful()
    {
        // Arrange
        const string expectedValue = "Success Value";
        var result = Result<string>.Success(expectedValue);

        // Act
        var success = result.TryGetValue(out var actualValue);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TryGetValue_Should_Return_False_And_Default_When_Failed()
    {
        // Arrange
        var result = Result<string>.Failure("Test error");

        // Act
        var success = result.TryGetValue(out var actualValue);

        // Assert
        Assert.False(success);
        Assert.Null(actualValue);
    }

    [Fact]
    public void GetValueOrDefault_With_Default_Should_Return_Value_When_Successful()
    {
        // Arrange
        const string expectedValue = "Success Value";
        const string defaultValue = "Default Value";
        var result = Result<string>.Success(expectedValue);

        // Act
        var actualValue = result.GetValueOrDefault(defaultValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void GetValueOrDefault_With_Default_Should_Return_Default_When_Failed()
    {
        // Arrange
        const string defaultValue = "Default Value";
        var result = Result<string>.Failure("Test error");

        // Act
        var actualValue = result.GetValueOrDefault(defaultValue);

        // Assert
        Assert.Equal(defaultValue, actualValue);
    }

    [Fact]
    public void GetValueOrDefault_Without_Default_Should_Return_Value_When_Successful()
    {
        // Arrange
        const int expectedValue = 42;
        var result = Result<int>.Success(expectedValue);

        // Act
        var actualValue = result.GetValueOrDefault();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void GetValueOrDefault_Without_Default_Should_Return_Default_Type_Value_When_Failed()
    {
        // Arrange
        var result = Result<int>.Failure("Test error");

        // Act
        var actualValue = result.GetValueOrDefault();

        // Assert
        Assert.Equal(0, actualValue); // default(int)
    }

    [Fact]
    public void GetValueOrDefault_Without_Default_Should_Return_Null_For_Reference_Types_When_Failed()
    {
        // Arrange
        var result = Result<string>.Failure("Test error");

        // Act
        var actualValue = result.GetValueOrDefault();

        // Assert
        Assert.Null(actualValue);
    }

    [Fact]
    public void Create_With_NonNull_Value_Should_Return_Success()
    {
        // Arrange
        const string value = "Test Value";
        var error = new Error("TEST.002", "Should not be used");

        // Act
        var result = Result<string>.Create(value, error);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Create_With_Null_Value_Should_Return_Failure()
    {
        // Arrange
        string? value = null;
        var error = new Error("TEST.003", "Value was null");

        // Act
        var result = Result<string>.Create(value, error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Create_With_Null_Value_And_String_Message_Should_Return_Failure()
    {
        // Arrange
        string? value = null; // Use string reference type instead
        const string errorMessage = "Value was null";

        // Act
        var result = Result<string>.Create(value, errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public void Create_With_Null_Value_And_Code_Message_Should_Return_Failure()
    {
        // Arrange
        string? value = null; // Use string reference type instead
        const string errorCode = "NULL.VALUE";
        const string errorMessage = "String value was null";

        // Act
        var result = Result<string>.Create(value, errorCode, errorMessage);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(errorCode, result.Error.Code);
        Assert.Equal(errorMessage, result.Error.Message);
    }

    [Fact]
    public void Implicit_Conversion_From_Value_Should_Create_Successful_Result()
    {
        // Arrange
        const string value = "Implicit Value";

        // Act
        Result<string> result = value;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Implicit_Conversion_From_Error_Should_Create_Failed_Result()
    {
        // Arrange
        var error = new Error("TEST.004", "Test error");

        // Act
        Result<string> result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Value_Types_Should_Work_Correctly(int value)
    {
        // Act
        var result = Result<int>.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Complex_Objects_Should_Work_Correctly()
    {
        // Arrange
        var complexObject = new { Name = "Test", Value = 42, Timestamp = DateTime.Now };

        // Act
        var result = Result<object>.Success(complexObject);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(complexObject, result.Value);
    }

    [Fact]
    public void Custom_Types_Should_Work_Correctly()
    {
        // Arrange
        var person = new Person("John Doe", 30);

        // Act
        var result = Result<Person>.Success(person);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(person, result.Value);
        Assert.Equal("John Doe", result.Value.Name);
        Assert.Equal(30, result.Value.Age);
    }

    [Fact]
    public void Nullable_Value_Types_Should_Work_Correctly()
    {
        // Arrange
        int? nullableValue = 42;

        // Act
        var result = Result<int?>.Success(nullableValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(nullableValue, result.Value);
    }

    [Fact]
    public void Nullable_Value_Types_With_Null_Should_Work_Correctly()
    {
        // Arrange
        int? nullableValue = null;

        // Act
        var result = Result<int?>.Success(nullableValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Generic_Type_Constraints_Should_Be_Respected()
    {
        // This test ensures that the generic Result<T> works with various types
        // including value types, reference types, and nullable types

        // Value type
        var intResult = Result<int>.Success(42);
        Assert.True(intResult.IsSuccess);

        // Reference type
        var stringResult = Result<string>.Success("test");
        Assert.True(stringResult.IsSuccess);

        // Nullable value type
        var nullableIntResult = Result<int?>.Success(null);
        Assert.True(nullableIntResult.IsSuccess);

        // Custom class
        var personResult = Result<Person>.Success(new Person("Test", 25));
        Assert.True(personResult.IsSuccess);
    }

    /// <summary>
    /// Test helper class for complex object testing
    /// </summary>
    private record Person(string Name, int Age);
}