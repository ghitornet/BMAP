using BMAP.Core.Result;

namespace BMAP.Core.Result.Tests;

/// <summary>
/// Unit tests for the Error class.
/// Tests cover creation, equality, metadata operations, and static factory methods.
/// </summary>
public class ErrorTests
{
    [Fact]
    public void Constructor_Should_Create_Error_With_Required_Properties()
    {
        // Arrange
        const string code = "TEST.001";
        const string message = "Test error message";
        const ErrorType type = ErrorType.Validation;

        // Act
        var error = new Error(code, message, type);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(type, error.Type);
        Assert.NotNull(error.Metadata);
        Assert.Empty(error.Metadata);
        Assert.False(error.IsNone);
    }

    [Fact]
    public void Constructor_With_Metadata_Should_Create_Error_With_Metadata()
    {
        // Arrange
        const string code = "TEST.002";
        const string message = "Test error with metadata";
        var metadata = new Dictionary<string, object>
        {
            ["UserId"] = 123,
            ["Action"] = "DeleteUser"
        };

        // Act
        var error = new Error(code, message, ErrorType.General, metadata);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.General, error.Type);
        Assert.Equal(2, error.Metadata.Count);
        Assert.Equal(123, error.Metadata["UserId"]);
        Assert.Equal("DeleteUser", error.Metadata["Action"]);
    }

    [Fact]
    public void None_Should_Represent_No_Error()
    {
        // Act
        var error = Error.None;

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
        Assert.Equal(ErrorType.None, error.Type);
        Assert.True(error.IsNone);
    }

    [Fact]
    public void NullValue_Should_Represent_Null_Value_Error()
    {
        // Act
        var error = Error.NullValue;

        // Assert
        Assert.Equal("General.NullValue", error.Code);
        Assert.Equal("A null value was provided", error.Message);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.False(error.IsNone);
    }

    [Fact]
    public void Validation_Should_Create_Validation_Error()
    {
        // Arrange
        const string code = "VALIDATION.001";
        const string message = "Invalid input";

        // Act
        var error = Error.Validation(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    [Fact]
    public void NotFound_Should_Create_NotFound_Error()
    {
        // Arrange
        const string code = "NOTFOUND.001";
        const string message = "Resource not found";

        // Act
        var error = Error.NotFound(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void Conflict_Should_Create_Conflict_Error()
    {
        // Arrange
        const string code = "CONFLICT.001";
        const string message = "Resource already exists";

        // Act
        var error = Error.Conflict(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void Unauthorized_Should_Create_Unauthorized_Error()
    {
        // Arrange
        const string code = "UNAUTHORIZED.001";
        const string message = "Authentication required";

        // Act
        var error = Error.Unauthorized(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void Forbidden_Should_Create_Forbidden_Error()
    {
        // Arrange
        const string code = "FORBIDDEN.001";
        const string message = "Access denied";

        // Act
        var error = Error.Forbidden(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.Forbidden, error.Type);
    }

    [Fact]
    public void Internal_Should_Create_Internal_Error()
    {
        // Arrange
        const string code = "INTERNAL.001";
        const string message = "Internal server error";

        // Act
        var error = Error.Internal(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.Internal, error.Type);
    }

    [Fact]
    public void External_Should_Create_External_Error()
    {
        // Arrange
        const string code = "EXTERNAL.001";
        const string message = "External service unavailable";

        // Act
        var error = Error.External(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.External, error.Type);
    }

    [Fact]
    public void Custom_Should_Create_Custom_Error()
    {
        // Arrange
        const string code = "CUSTOM.001";
        const string message = "Custom application error";

        // Act
        var error = Error.Custom(code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.Custom, error.Type);
    }

    [Fact]
    public void WithMetadata_Single_Should_Add_Metadata()
    {
        // Arrange
        var originalError = new Error("TEST.003", "Original error");

        // Act
        var errorWithMetadata = originalError.WithMetadata("Key1", "Value1");

        // Assert
        Assert.NotSame(originalError, errorWithMetadata);
        Assert.Equal(originalError.Code, errorWithMetadata.Code);
        Assert.Equal(originalError.Message, errorWithMetadata.Message);
        Assert.Equal(originalError.Type, errorWithMetadata.Type);
        Assert.Single(errorWithMetadata.Metadata);
        Assert.Equal("Value1", errorWithMetadata.Metadata["Key1"]);
    }

    [Fact]
    public void WithMetadata_Multiple_Should_Add_Multiple_Metadata()
    {
        // Arrange
        var originalError = new Error("TEST.004", "Original error");
        var newMetadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42,
            ["Key3"] = DateTime.Today
        };

        // Act
        var errorWithMetadata = originalError.WithMetadata(newMetadata);

        // Assert
        Assert.NotSame(originalError, errorWithMetadata);
        Assert.Equal(3, errorWithMetadata.Metadata.Count);
        Assert.Equal("Value1", errorWithMetadata.Metadata["Key1"]);
        Assert.Equal(42, errorWithMetadata.Metadata["Key2"]);
        Assert.Equal(DateTime.Today, errorWithMetadata.Metadata["Key3"]);
    }

    [Fact]
    public void WithMetadata_Should_Update_Existing_Metadata()
    {
        // Arrange
        var originalMetadata = new Dictionary<string, object> { ["Key1"] = "OriginalValue" };
        var originalError = new Error("TEST.005", "Original error", ErrorType.General, originalMetadata);

        // Act
        var errorWithUpdatedMetadata = originalError.WithMetadata("Key1", "UpdatedValue");

        // Assert
        Assert.Equal("UpdatedValue", errorWithUpdatedMetadata.Metadata["Key1"]);
    }

    [Fact]
    public void ToString_Should_Return_Formatted_String()
    {
        // Arrange
        const string code = "TEST.006";
        const string message = "Test error message";
        var error = new Error(code, message);

        // Act
        var result = error.ToString();

        // Assert
        Assert.Equal($"[{code}] {message}", result);
    }

    [Fact]
    public void Equals_Should_Return_True_For_Same_Code_Message_Type()
    {
        // Arrange
        var error1 = new Error("TEST.007", "Same error", ErrorType.Validation);
        var error2 = new Error("TEST.007", "Same error", ErrorType.Validation);

        // Act & Assert
        Assert.True(error1.Equals(error2));
        Assert.True(error1.Equals((object)error2));
        Assert.True(error1 == error2);
        Assert.False(error1 != error2);
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Code()
    {
        // Arrange
        var error1 = new Error("TEST.008", "Same message", ErrorType.Validation);
        var error2 = new Error("TEST.009", "Same message", ErrorType.Validation);

        // Act & Assert
        Assert.False(error1.Equals(error2));
        Assert.False(error1 == error2);
        Assert.True(error1 != error2);
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Message()
    {
        // Arrange
        var error1 = new Error("TEST.010", "Message 1", ErrorType.Validation);
        var error2 = new Error("TEST.010", "Message 2", ErrorType.Validation);

        // Act & Assert
        Assert.False(error1.Equals(error2));
    }

    [Fact]
    public void Equals_Should_Return_False_For_Different_Type()
    {
        // Arrange
        var error1 = new Error("TEST.011", "Same message", ErrorType.Validation);
        var error2 = new Error("TEST.011", "Same message", ErrorType.NotFound);

        // Act & Assert
        Assert.False(error1.Equals(error2));
    }

    [Fact]
    public void Equals_Should_Return_False_For_Null()
    {
        // Arrange
        var error = new Error("TEST.012", "Test message");

        // Act & Assert
        Assert.False(error.Equals(null));
        Assert.False(error.Equals((object?)null));
        Assert.False(error == null);
        Assert.False(null == error);
        Assert.True(error != null);
        Assert.True(null != error);
    }

    [Fact]
    public void GetHashCode_Should_Be_Same_For_Equal_Errors()
    {
        // Arrange
        var error1 = new Error("TEST.013", "Same error", ErrorType.Validation);
        var error2 = new Error("TEST.013", "Same error", ErrorType.Validation);

        // Act & Assert
        Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
    }

    [Fact]
    public void Implicit_Conversion_From_String_Should_Create_Error()
    {
        // Arrange
        const string message = "Implicit error message";

        // Act
        Error error = message;

        // Assert
        Assert.Equal("General.Error", error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.General, error.Type);
    }

    [Fact]
    public void Implicit_Conversion_From_Tuple_Should_Create_Error()
    {
        // Arrange
        const string code = "TUPLE.001";
        const string message = "Tuple error message";

        // Act
        Error error = (code, message);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(ErrorType.General, error.Type);
    }

    [Fact]
    public void Metadata_Should_Be_ReadOnly()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["Key1"] = "Value1" };
        var error = new Error("TEST.014", "Test message", ErrorType.General, metadata);

        // Act
        var retrievedMetadata = error.Metadata;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(retrievedMetadata);
        Assert.Single(retrievedMetadata);
    }

    [Fact]
    public void Factory_Methods_With_Metadata_Should_Include_Metadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["TestKey"] = "TestValue" };

        // Act
        var validationError = Error.Validation("VAL.001", "Validation failed", metadata);
        var notFoundError = Error.NotFound("NF.001", "Not found", metadata);
        var conflictError = Error.Conflict("CONF.001", "Conflict", metadata);

        // Assert
        Assert.True(validationError.Metadata.ContainsKey("TestKey"));
        Assert.True(notFoundError.Metadata.ContainsKey("TestKey"));
        Assert.True(conflictError.Metadata.ContainsKey("TestKey"));
    }

    [Theory]
    [InlineData(ErrorType.None, true)]
    [InlineData(ErrorType.General, false)]
    [InlineData(ErrorType.Validation, false)]
    [InlineData(ErrorType.NotFound, false)]
    public void IsNone_Should_Return_Correct_Value(ErrorType errorType, bool expectedIsNone)
    {
        // Arrange
        var error = new Error("TEST", "Test", errorType);

        // Act & Assert
        Assert.Equal(expectedIsNone, error.IsNone);
    }
}