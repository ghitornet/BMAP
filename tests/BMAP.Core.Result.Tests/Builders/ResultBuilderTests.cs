using BMAP.Core.Result;
using BMAP.Core.Result.Builders;

namespace BMAP.Core.Result.Tests.Builders;

/// <summary>
/// Unit tests for the ResultBuilder class (non-generic version).
/// Tests cover fluent building, validation, error handling, and edge cases.
/// </summary>
public class ResultBuilderTests
{
    #region Factory Method Tests

    [Fact]
    public void Success_Should_Return_Builder_Configured_For_Success()
    {
        // Act
        var builder = ResultBuilder.Success();
        var result = builder.Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_Should_Return_Builder_Configured_For_Failure()
    {
        // Act
        var builder = ResultBuilder.Failure();

        // Assert
        Assert.False(builder.CanBuild()); // Cannot build without error
    }

    [Fact]
    public void Failure_WithError_Should_Return_Builder_With_Error()
    {
        // Arrange
        var error = new Error("TEST.001", "Test error");

        // Act
        var builder = ResultBuilder.Failure(error);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultBuilder.Failure((Error)null!));
    }

    [Fact]
    public void Failure_WithMessage_Should_Return_Builder_With_Error_Message()
    {
        // Arrange
        const string message = "Test failure message";

        // Act
        var builder = ResultBuilder.Failure(message);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("General.Failure", result.Error!.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Failure_WithNullOrWhitespaceMessage_Should_Throw_ArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure((string)null!));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure(""));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure("   "));
    }

    [Fact]
    public void Failure_WithCodeAndMessage_Should_Return_Builder_With_Custom_Error()
    {
        // Arrange
        const string code = "CUSTOM.001";
        const string message = "Custom error message";

        // Act
        var builder = ResultBuilder.Failure(code, message);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Failure_WithNullOrWhitespaceCodeOrMessage_Should_Throw_ArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure(null!, "message"));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure("", "message"));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure("   ", "message"));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure("code", null!));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure("code", ""));
        Assert.Throws<ArgumentException>(() => ResultBuilder.Failure("code", "   "));
    }

    [Fact]
    public void FromResult_Should_Copy_Result_Properties()
    {
        // Arrange
        var originalResult = Result.Failure("ORIGINAL.001", "Original error");

        // Act
        var builder = ResultBuilder.FromResult(originalResult);
        var result = builder.Build();

        // Assert
        Assert.Equal(originalResult.IsSuccess, result.IsSuccess);
        Assert.Equal(originalResult.Error, result.Error);
    }

    [Fact]
    public void FromResult_WithNullResult_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultBuilder.FromResult(null!));
    }

    [Fact]
    public void Create_WithTrueCondition_Should_Return_Success_Builder()
    {
        // Act
        var builder = ResultBuilder.Create(true);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithFalseCondition_Should_Return_Failure_Builder()
    {
        // Act
        var builder = ResultBuilder.Create(false);

        // Assert
        Assert.False(builder.CanBuild()); // Cannot build without error
    }

    [Fact]
    public void Create_WithFalseConditionAndError_Should_Return_Failure_Builder_With_Error()
    {
        // Arrange
        var error = new Error("TEST.002", "Conditional error");

        // Act
        var builder = ResultBuilder.Create(false, error);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Create_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultBuilder.Create(false, null!));
    }

    #endregion

    #region Error Setting Tests

    [Fact]
    public void WithError_Should_Set_Error_And_Configure_For_Failure()
    {
        // Arrange
        var error = new Error("TEST.003", "Test error");

        // Act
        var result = ResultBuilder
            .Success()
            .WithError(error)
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void WithError_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithError((Error)null!));
    }

    [Fact]
    public void WithError_WithCodeAndMessage_Should_Set_Error_And_Configure_For_Failure()
    {
        // Arrange
        const string code = "TEST.004";
        const string message = "Test error message";

        // Act
        var result = ResultBuilder
            .Success()
            .WithError(code, message)
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void WithError_WithNullOrWhitespaceCodeOrMessage_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithError(null!, "message"));
        Assert.Throws<ArgumentException>(() => builder.WithError("", "message"));
        Assert.Throws<ArgumentException>(() => builder.WithError("   ", "message"));
        Assert.Throws<ArgumentException>(() => builder.WithError("code", null!));
        Assert.Throws<ArgumentException>(() => builder.WithError("code", ""));
        Assert.Throws<ArgumentException>(() => builder.WithError("code", "   "));
    }

    #endregion

    #region Error Type Tests

    [Fact]
    public void AsValidation_Should_Set_Error_Type_To_Validation()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.005", "Test message")
            .AsValidation()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public void AsValidation_WithoutError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.AsValidation());
    }

    [Fact]
    public void AsNotFound_Should_Set_Error_Type_To_NotFound()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.006", "Test message")
            .AsNotFound()
            .Build();

        // Assert
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public void AsConflict_Should_Set_Error_Type_To_Conflict()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.007", "Test message")
            .AsConflict()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    [Fact]
    public void AsUnauthorized_Should_Set_Error_Type_To_Unauthorized()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.008", "Test message")
            .AsUnauthorized()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }

    [Fact]
    public void AsForbidden_Should_Set_Error_Type_To_Forbidden()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.009", "Test message")
            .AsForbidden()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public void AsInternal_Should_Set_Error_Type_To_Internal()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.010", "Test message")
            .AsInternal()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
    }

    [Fact]
    public void AsExternal_Should_Set_Error_Type_To_External()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.011", "Test message")
            .AsExternal()
            .Build();

        // Assert
        Assert.Equal(ErrorType.External, result.Error!.Type);
    }

    [Fact]
    public void AsCustom_Should_Set_Error_Type_To_Custom()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.012", "Test message")
            .AsCustom()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Custom, result.Error!.Type);
    }

    #endregion

    #region Error Metadata Tests

    [Fact]
    public void WithErrorMetadata_Single_Should_Add_Metadata_To_Error()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.013", "Test message")
            .WithErrorMetadata("UserId", 123)
            .Build();

        // Assert
        Assert.Single(result.Error!.Metadata);
        Assert.Equal(123, result.Error.Metadata["UserId"]);
    }

    [Fact]
    public void WithErrorMetadata_Single_WithoutError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithErrorMetadata("key", "value"));
    }

    [Fact]
    public void WithErrorMetadata_Single_WithNullKey_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ResultBuilder.Failure("TEST.014", "Test message");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithErrorMetadata(null!, "value"));
        Assert.Throws<ArgumentException>(() => builder.WithErrorMetadata("", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithErrorMetadata("   ", "value"));
    }

    [Fact]
    public void WithErrorMetadata_Single_WithNullValue_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ResultBuilder.Failure("TEST.015", "Test message");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithErrorMetadata("key", null!));
    }

    [Fact]
    public void WithErrorMetadata_Multiple_Should_Add_All_Metadata_To_Error()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42
        };

        // Act
        var result = ResultBuilder
            .Failure("TEST.016", "Test message")
            .WithErrorMetadata(metadata)
            .Build();

        // Assert
        Assert.Equal(2, result.Error!.Metadata.Count);
        Assert.Equal("Value1", result.Error.Metadata["Key1"]);
        Assert.Equal(42, result.Error.Metadata["Key2"]);
    }

    [Fact]
    public void WithErrorMetadata_Multiple_WithoutError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder.Success();
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithErrorMetadata(metadata));
    }

    [Fact]
    public void WithErrorMetadata_Multiple_WithNullDictionary_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ResultBuilder.Failure("TEST.017", "Test message");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithErrorMetadata((IDictionary<string, object>)null!));
    }

    #endregion

    #region Conditional Building Tests

    [Fact]
    public void If_WithTrueCondition_Should_Execute_Action()
    {
        // Arrange
        var executed = false;

        // Act
        var result = ResultBuilder
            .Success()
            .If(true, builder => executed = true)
            .Build();

        // Assert
        Assert.True(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void If_WithFalseCondition_Should_Not_Execute_Action()
    {
        // Arrange
        var executed = false;

        // Act
        var result = ResultBuilder
            .Success()
            .If(false, builder => executed = true)
            .Build();

        // Assert
        Assert.False(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void If_WithConditionFactory_Should_Evaluate_Function()
    {
        // Arrange
        var shouldExecute = true;
        var executed = false;

        // Act
        var result = ResultBuilder
            .Success()
            .If(() => shouldExecute, builder => executed = true)
            .Build();

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void If_WithNullConditionFactory_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.If((Func<bool>)null!, b => { }));
    }

    [Fact]
    public void If_WithNullAction_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.If(true, (Action<ResultBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => builder.If(() => true, (Action<ResultBuilder>)null!));
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void CanBuild_WithSuccessBuilder_Should_Return_True()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.True(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithFailureAndError_Should_Return_True()
    {
        // Arrange
        var builder = ResultBuilder.Failure("TEST.018", "Test message");

        // Act & Assert
        Assert.True(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithFailureButNoError_Should_Return_False()
    {
        // Arrange
        var builder = ResultBuilder.Failure();

        // Act & Assert
        Assert.False(builder.CanBuild());
    }

    [Fact]
    public void Validate_WithValidConfiguration_Should_Return_Builder()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act & Assert
        Assert.Same(builder, builder.Validate());
    }

    [Fact]
    public void Validate_WithFailureButNoError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder.Failure();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Validate());
    }

    [Fact]
    public void Build_WithValidConfiguration_Should_Return_Result()
    {
        // Act
        var result = ResultBuilder
            .Failure("TEST.019", "Test message")
            .AsValidation()
            .WithErrorMetadata("TestKey", "TestValue")
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("TEST.019", result.Error!.Code);
        Assert.Equal("Test message", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Single(result.Error.Metadata);
        Assert.Equal("TestValue", result.Error.Metadata["TestKey"]);
    }

    [Fact]
    public void Build_WithInvalidConfiguration_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder.Failure();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_Should_Call_Build()
    {
        // Act
        Result result = ResultBuilder.Success();

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ImplicitConversion_WithNullBuilder_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result result = (ResultBuilder)null!;
        });
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithSuccessBuilder_Should_Return_Descriptive_String()
    {
        // Arrange
        var builder = ResultBuilder.Success();

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("Success", result);
        Assert.Contains("ResultBuilder", result);
    }

    [Fact]
    public void ToString_WithFailureBuilder_Should_Return_Descriptive_String()
    {
        // Arrange
        var builder = ResultBuilder.Failure("TEST.020", "Test message");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("Failure", result);
        Assert.Contains("TEST.020", result);
        Assert.Contains("ResultBuilder", result);
    }

    #endregion

    #region Fluent Chaining Tests

    [Fact]
    public void FluentChaining_Should_Allow_Complex_Configuration()
    {
        // Act
        var result = ResultBuilder
            .Failure()
            .WithError("COMPLEX.001", "Complex error scenario")
            .AsValidation()
            .WithErrorMetadata("UserId", 123)
            .WithErrorMetadata("Action", "UpdateProfile")
            .If(true, builder => builder.WithErrorMetadata("Conditional", "AddedConditionally"))
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("COMPLEX.001", result.Error!.Code);
        Assert.Equal("Complex error scenario", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(3, result.Error.Metadata.Count);
        Assert.Equal(123, result.Error.Metadata["UserId"]);
        Assert.Equal("UpdateProfile", result.Error.Metadata["Action"]);
        Assert.Equal("AddedConditionally", result.Error.Metadata["Conditional"]);
    }

    #endregion
}

/// <summary>
/// Unit tests for the ResultBuilder&lt;T&gt; class (generic version).
/// Tests cover fluent building, validation, error handling, value management, and edge cases.
/// </summary>
public class ResultBuilderGenericTests
{
    #region Factory Method Tests

    [Fact]
    public void Success_Should_Return_Builder_Configured_For_Success_With_Value()
    {
        // Arrange
        const string value = "Test Value";

        // Act
        var builder = ResultBuilder<string>.Success(value);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Success_WithNullValue_Should_Return_Builder_With_Null_Value()
    {
        // Act
        var builder = ResultBuilder<string>.Success(null!);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Failure_Should_Return_Builder_Configured_For_Failure()
    {
        // Act
        var builder = ResultBuilder<string>.Failure();

        // Assert
        Assert.False(builder.CanBuild()); // Cannot build without error
    }

    [Fact]
    public void Failure_WithError_Should_Return_Builder_With_Error()
    {
        // Arrange
        var error = new Error("TEST.001", "Test error");

        // Act
        var builder = ResultBuilder<string>.Failure(error);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultBuilder<string>.Failure((Error)null!));
    }

    [Fact]
    public void Failure_WithMessage_Should_Return_Builder_With_Error_Message()
    {
        // Arrange
        const string message = "Test failure message";

        // Act
        var builder = ResultBuilder<string>.Failure(message);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("General.Failure", result.Error!.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void Failure_WithNullOrWhitespaceMessage_Should_Throw_ArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ResultBuilder<string>.Failure((string)null!));
        Assert.Throws<ArgumentException>(() => ResultBuilder<string>.Failure(""));
        Assert.Throws<ArgumentException>(() => ResultBuilder<string>.Failure("   "));
    }

    [Fact]
    public void Failure_WithCodeAndMessage_Should_Return_Builder_With_Custom_Error()
    {
        // Arrange
        const string code = "CUSTOM.001";
        const string message = "Custom error message";

        // Act
        var builder = ResultBuilder<string>.Failure(code, message);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal(message, result.Error.Message);
    }

    [Fact]
    public void FromResult_Should_Copy_Result_Properties()
    {
        // Arrange
        var originalResult = Result<string>.Success("Test Value");

        // Act
        var builder = ResultBuilder<string>.FromResult(originalResult);
        var result = builder.Build();

        // Assert
        Assert.Equal(originalResult.IsSuccess, result.IsSuccess);
        Assert.Equal(originalResult.Value, result.Value);
    }

    [Fact]
    public void FromResult_WithNullResult_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultBuilder<string>.FromResult(null!));
    }

    [Fact]
    public void Create_WithTrueCondition_Should_Return_Success_Builder()
    {
        // Arrange
        const string value = "Success Value";

        // Act
        var builder = ResultBuilder<string>.Create(true, value);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void Create_WithFalseCondition_Should_Return_Failure_Builder()
    {
        // Act
        var builder = ResultBuilder<string>.Create(false, "unused");

        // Assert
        Assert.False(builder.CanBuild()); // Cannot build without error
    }

    [Fact]
    public void Create_WithFalseConditionAndError_Should_Return_Failure_Builder_With_Error()
    {
        // Arrange
        var error = new Error("TEST.002", "Conditional error");

        // Act
        var builder = ResultBuilder<string>.Create(false, "unused", error);
        var result = builder.Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Create_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResultBuilder<string>.Create(false, "unused", null!));
    }

    #endregion

    #region Value Setting Tests

    [Fact]
    public void WithValue_Should_Set_Value_And_Configure_For_Success()
    {
        // Arrange
        const string value = "New Value";

        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.003", "Error")
            .WithValue(value)
            .Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void WithValue_WithNull_Should_Set_Null_Value()
    {
        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.004", "Error")
            .WithValue(null!)
            .Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    #endregion

    #region Error Setting Tests

    [Fact]
    public void WithError_Should_Set_Error_And_Configure_For_Failure()
    {
        // Arrange
        var error = new Error("TEST.005", "Test error");

        // Act
        var result = ResultBuilder<string>
            .Success("value")
            .WithError(error)
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void WithError_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ResultBuilder<string>.Success("value");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithError((Error)null!));
    }

    [Fact]
    public void WithError_WithCodeAndMessage_Should_Set_Error_And_Configure_For_Failure()
    {
        // Arrange
        const string code = "TEST.006";
        const string message = "Test error message";

        // Act
        var result = ResultBuilder<string>
            .Success("value")
            .WithError(code, message)
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error!.Code);
        Assert.Equal(message, result.Error.Message);
    }

    #endregion

    #region Error Type Tests

    [Fact]
    public void AsValidation_Should_Set_Error_Type_To_Validation()
    {
        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.007", "Test message")
            .AsValidation()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public void AsValidation_WithoutError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder<string>.Success("value");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.AsValidation());
    }

    [Fact]
    public void AsNotFound_Should_Set_Error_Type_To_NotFound()
    {
        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.008", "Test message")
            .AsNotFound()
            .Build();

        // Assert
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    // Similar tests for other error types...

    #endregion

    #region Error Metadata Tests

    [Fact]
    public void WithErrorMetadata_Single_Should_Add_Metadata_To_Error()
    {
        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.009", "Test message")
            .WithErrorMetadata("UserId", 123)
            .Build();

        // Assert
        Assert.Single(result.Error!.Metadata);
        Assert.Equal(123, result.Error.Metadata["UserId"]);
    }

    [Fact]
    public void WithErrorMetadata_Single_WithoutError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder<string>.Success("value");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithErrorMetadata("key", "value"));
    }

    [Fact]
    public void WithErrorMetadata_Multiple_Should_Add_All_Metadata_To_Error()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42
        };

        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.010", "Test message")
            .WithErrorMetadata(metadata)
            .Build();

        // Assert
        Assert.Equal(2, result.Error!.Metadata.Count);
        Assert.Equal("Value1", result.Error.Metadata["Key1"]);
        Assert.Equal(42, result.Error.Metadata["Key2"]);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void CanBuild_WithSuccessBuilderAndValue_Should_Return_True()
    {
        // Arrange
        var builder = ResultBuilder<string>.Success("value");

        // Act & Assert
        Assert.True(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithSuccessBuilderAndNullValue_Should_Return_True_For_Nullable()
    {
        // Arrange
        var builder = ResultBuilder<string?>.Success(null);

        // Act & Assert
        Assert.True(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithFailureAndError_Should_Return_True()
    {
        // Arrange
        var builder = ResultBuilder<string>.Failure("TEST.011", "Test message");

        // Act & Assert
        Assert.True(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithFailureButNoError_Should_Return_False()
    {
        // Arrange
        var builder = ResultBuilder<string>.Failure();

        // Act & Assert
        Assert.False(builder.CanBuild());
    }

    [Fact]
    public void Validate_WithValidConfiguration_Should_Return_Builder()
    {
        // Arrange
        var builder = ResultBuilder<string>.Success("value");

        // Act & Assert
        Assert.Same(builder, builder.Validate());
    }

    [Fact]
    public void Validate_WithFailureButNoError_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder<string>.Failure();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Validate());
    }

    [Fact]
    public void Build_WithValidConfiguration_Should_Return_Result()
    {
        // Act
        var result = ResultBuilder<string>
            .Failure("TEST.012", "Test message")
            .AsValidation()
            .WithErrorMetadata("TestKey", "TestValue")
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("TEST.012", result.Error!.Code);
        Assert.Equal("Test message", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Single(result.Error.Metadata);
        Assert.Equal("TestValue", result.Error.Metadata["TestKey"]);
    }

    [Fact]
    public void Build_WithInvalidConfiguration_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ResultBuilder<string>.Failure();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_Should_Call_Build()
    {
        // Act
        Result<string> result = ResultBuilder<string>.Success("value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("value", result.Value);
    }

    [Fact]
    public void ImplicitConversion_WithNullBuilder_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Result<string> result = (ResultBuilder<string>)null!;
        });
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WithSuccessBuilder_Should_Return_Descriptive_String()
    {
        // Arrange
        var builder = ResultBuilder<string>.Success("value");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("Success", result);
        Assert.Contains("String", result);
        Assert.Contains("value", result);
        Assert.Contains("ResultBuilder", result);
    }

    [Fact]
    public void ToString_WithFailureBuilder_Should_Return_Descriptive_String()
    {
        // Arrange
        var builder = ResultBuilder<string>.Failure("TEST.013", "Test message");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("Failure", result);
        Assert.Contains("String", result);
        Assert.Contains("TEST.013", result);
        Assert.Contains("ResultBuilder", result);
    }

    #endregion

    #region Fluent Chaining Tests

    [Fact]
    public void FluentChaining_Should_Allow_Complex_Configuration()
    {
        // Act
        var result = ResultBuilder<User>
            .Failure()
            .WithError("COMPLEX.001", "Complex error scenario")
            .AsValidation()
            .WithErrorMetadata("UserId", 123)
            .WithErrorMetadata("Action", "UpdateProfile")
            .If(true, builder => builder.WithErrorMetadata("Conditional", "AddedConditionally"))
            .Build();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("COMPLEX.001", result.Error!.Code);
        Assert.Equal("Complex error scenario", result.Error.Message);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(3, result.Error.Metadata.Count);
        Assert.Equal(123, result.Error.Metadata["UserId"]);
        Assert.Equal("UpdateProfile", result.Error.Metadata["Action"]);
        Assert.Equal("AddedConditionally", result.Error.Metadata["Conditional"]);
    }

    [Fact]
    public void FluentChaining_Success_To_Failure_To_Success_Should_Work()
    {
        // Act
        var result = ResultBuilder<string>
            .Success("initial")
            .WithError("ERROR.001", "Something went wrong")
            .WithValue("final")
            .Build();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("final", result.Value);
    }

    #endregion

    #region Test Helper Classes

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}