using BMAP.Core.Result;

namespace BMAP.Core.Result.Tests;

/// <summary>
/// Unit tests for the ErrorType enumeration.
/// Tests cover all enum values and their intended usage.
/// </summary>
public class ErrorTypeTests
{
    [Fact]
    public void ErrorType_Should_Have_Expected_Values()
    {
        // Assert - Verify all expected enum values exist
        Assert.True(Enum.IsDefined(ErrorType.None));
        Assert.True(Enum.IsDefined(ErrorType.General));
        Assert.True(Enum.IsDefined(ErrorType.Validation));
        Assert.True(Enum.IsDefined(ErrorType.NotFound));
        Assert.True(Enum.IsDefined(ErrorType.Conflict));
        Assert.True(Enum.IsDefined(ErrorType.Unauthorized));
        Assert.True(Enum.IsDefined(ErrorType.Forbidden));
        Assert.True(Enum.IsDefined(ErrorType.Internal));
        Assert.True(Enum.IsDefined(ErrorType.External));
        Assert.True(Enum.IsDefined(ErrorType.Custom));
    }

    [Fact]
    public void ErrorType_Values_Should_Have_Expected_Integer_Values()
    {
        // Assert - Verify the underlying integer values
        Assert.Equal(0, (int)ErrorType.None);
        Assert.Equal(1, (int)ErrorType.General);
        Assert.Equal(2, (int)ErrorType.Validation);
        Assert.Equal(3, (int)ErrorType.NotFound);
        Assert.Equal(4, (int)ErrorType.Conflict);
        Assert.Equal(5, (int)ErrorType.Unauthorized);
        Assert.Equal(6, (int)ErrorType.Forbidden);
        Assert.Equal(7, (int)ErrorType.Internal);
        Assert.Equal(8, (int)ErrorType.External);
        Assert.Equal(9, (int)ErrorType.Custom);
    }

    [Theory]
    [InlineData(ErrorType.None)]
    [InlineData(ErrorType.General)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Internal)]
    [InlineData(ErrorType.External)]
    [InlineData(ErrorType.Custom)]
    public void ErrorType_Should_Convert_To_String(ErrorType errorType)
    {
        // Act
        var stringValue = errorType.ToString();

        // Assert
        Assert.NotNull(stringValue);
        Assert.NotEmpty(stringValue);
        Assert.Equal(errorType, Enum.Parse<ErrorType>(stringValue));
    }

    [Fact]
    public void ErrorType_Should_Support_Comparison()
    {
        // Assert - Verify enum comparison works as expected
        Assert.True(ErrorType.None < ErrorType.General);
        Assert.True(ErrorType.Validation > ErrorType.General);
        Assert.True(ErrorType.Custom > ErrorType.External);
        
        Assert.True(ErrorType.None.CompareTo(ErrorType.General) < 0);
        Assert.True(ErrorType.Custom.CompareTo(ErrorType.None) > 0);
        Assert.Equal(0, ErrorType.Validation.CompareTo(ErrorType.Validation));
    }

    [Fact]
    public void ErrorType_Should_Have_Stable_Hash_Codes()
    {
        // Arrange & Act
        var noneHash1 = ErrorType.None.GetHashCode();
        var noneHash2 = ErrorType.None.GetHashCode();
        var validationHash = ErrorType.Validation.GetHashCode();

        // Assert
        Assert.Equal(noneHash1, noneHash2);
        Assert.NotEqual(noneHash1, validationHash);
    }

    [Fact]
    public void ErrorType_Should_Work_In_Switch_Expressions()
    {
        // Assert
        Assert.Equal("No error", GetMessage(ErrorType.None));
        Assert.Equal("Validation error", GetMessage(ErrorType.Validation));
        Assert.Equal("Not found error", GetMessage(ErrorType.NotFound));
        Assert.Equal("Internal error", GetMessage(ErrorType.Internal));
        return;

        // Arrange & Act
        static string GetMessage(ErrorType type) =>
            type switch
            {
                ErrorType.None => "No error",
                ErrorType.General => "General error",
                ErrorType.Validation => "Validation error",
                ErrorType.NotFound => "Not found error",
                ErrorType.Conflict => "Conflict error",
                ErrorType.Unauthorized => "Unauthorized error",
                ErrorType.Forbidden => "Forbidden error",
                ErrorType.Internal => "Internal error",
                ErrorType.External => "External error",
                ErrorType.Custom => "Custom error",
                _ => "Unknown error"
            };
    }

    [Fact]
    public void ErrorType_Should_Work_With_Error_Factory_Methods()
    {
        // Act
        var validationError = Error.Validation("VAL.001", "Validation failed");
        var notFoundError = Error.NotFound("NF.001", "Resource not found");
        var conflictError = Error.Conflict("CONF.001", "Resource conflict");
        var unauthorizedError = Error.Unauthorized("AUTH.001", "Authentication required");
        var forbiddenError = Error.Forbidden("FORB.001", "Access denied");
        var internalError = Error.Internal("INT.001", "Internal error");
        var externalError = Error.External("EXT.001", "External service error");
        var customError = Error.Custom("CUST.001", "Custom error");

        // Assert
        Assert.Equal(ErrorType.Validation, validationError.Type);
        Assert.Equal(ErrorType.NotFound, notFoundError.Type);
        Assert.Equal(ErrorType.Conflict, conflictError.Type);
        Assert.Equal(ErrorType.Unauthorized, unauthorizedError.Type);
        Assert.Equal(ErrorType.Forbidden, forbiddenError.Type);
        Assert.Equal(ErrorType.Internal, internalError.Type);
        Assert.Equal(ErrorType.External, externalError.Type);
        Assert.Equal(ErrorType.Custom, customError.Type);
    }

    [Fact]
    public void ErrorType_Should_Be_Serializable()
    {
        // This test ensures that ErrorType can be serialized/deserialized
        // which is important for APIs and persistence scenarios

        // Arrange
        var originalType = ErrorType.Validation;

        // Act - Simulate serialization/deserialization by converting to string and back
        var serialized = originalType.ToString();
        var deserialized = Enum.Parse<ErrorType>(serialized);

        // Assert
        Assert.Equal(originalType, deserialized);
    }

    [Fact]
    public void ErrorType_Count_Should_Match_Expected()
    {
        // Act
        var allValues = Enum.GetValues<ErrorType>();

        // Assert - Verify we have exactly the expected number of error types
        Assert.Equal(10, allValues.Length);
    }

    [Fact]
    public void ErrorType_Names_Should_Be_Descriptive()
    {
        // Act
        var names = Enum.GetNames<ErrorType>();

        // Assert - Verify all names are meaningful and follow conventions
        Assert.Contains("None", names);
        Assert.Contains("General", names);
        Assert.Contains("Validation", names);
        Assert.Contains("NotFound", names);
        Assert.Contains("Conflict", names);
        Assert.Contains("Unauthorized", names);
        Assert.Contains("Forbidden", names);
        Assert.Contains("Internal", names);
        Assert.Contains("External", names);
        Assert.Contains("Custom", names);
        
        // All names should follow PascalCase convention
        foreach (var name in names)
        {
            Assert.NotNull(name);
            Assert.NotEmpty(name);
            Assert.True(char.IsUpper(name[0]), "Name should start with uppercase letter");
        }
    }

    [Theory]
    [InlineData(0, ErrorType.None)]
    [InlineData(1, ErrorType.General)]
    [InlineData(2, ErrorType.Validation)]
    [InlineData(3, ErrorType.NotFound)]
    [InlineData(4, ErrorType.Conflict)]
    [InlineData(5, ErrorType.Unauthorized)]
    [InlineData(6, ErrorType.Forbidden)]
    [InlineData(7, ErrorType.Internal)]
    [InlineData(8, ErrorType.External)]
    [InlineData(9, ErrorType.Custom)]
    public void ErrorType_Should_Cast_From_Integer(int intValue, ErrorType expectedType)
    {
        // Act
        var errorType = (ErrorType)intValue;

        // Assert
        Assert.Equal(expectedType, errorType);
    }

    [Fact]
    public void ErrorType_Should_Handle_Invalid_Cast_Gracefully()
    {
        // Act & Assert - This should not throw, but will create an undefined enum value
        var invalidType = (ErrorType)999;
        
        // The enum value exists but is not defined
        Assert.False(Enum.IsDefined(invalidType));
        
        // But it should still have a numeric value
        Assert.Equal(999, (int)invalidType);
    }
}