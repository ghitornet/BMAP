using BMAP.Core.Result;
using BMAP.Core.Result.Builders;

namespace BMAP.Core.Result.Tests.Builders;

/// <summary>
/// Unit tests for the ErrorBuilder class.
/// Tests cover fluent building, validation, error types, metadata handling, and edge cases.
/// </summary>
public class ErrorBuilderTests
{
    #region Factory Method Tests

    [Fact]
    public void Create_Should_Return_New_ErrorBuilder_Instance()
    {
        // Act
        var builder = ErrorBuilder.Create();

        // Assert
        Assert.NotNull(builder);
        Assert.False(builder.CanBuild());
    }

    [Fact]
    public void WithCode_Should_Return_ErrorBuilder_With_Code_Set()
    {
        // Arrange
        const string code = "TEST.001";

        // Act
        var builder = ErrorBuilder.WithCode(code);

        // Assert
        Assert.NotNull(builder);
        Assert.Contains(code, builder.ToString());
    }

    [Fact]
    public void WithCode_WithNullOrWhitespace_Should_Throw_ArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ErrorBuilder.WithCode(null!));
        Assert.Throws<ArgumentException>(() => ErrorBuilder.WithCode(""));
        Assert.Throws<ArgumentException>(() => ErrorBuilder.WithCode("   "));
    }

    [Fact]
    public void WithMessage_Should_Return_ErrorBuilder_With_Message_Set()
    {
        // Arrange
        const string message = "Test error message";

        // Act
        var builder = ErrorBuilder.WithMessage(message);

        // Assert
        Assert.NotNull(builder);
        Assert.Contains(message, builder.ToString());
    }

    [Fact]
    public void WithMessage_WithNullOrWhitespace_Should_Throw_ArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ErrorBuilder.WithMessage(null!));
        Assert.Throws<ArgumentException>(() => ErrorBuilder.WithMessage(""));
        Assert.Throws<ArgumentException>(() => ErrorBuilder.WithMessage("   "));
    }

    [Fact]
    public void FromError_Should_Create_Builder_With_All_Properties_Copied()
    {
        // Arrange
        var originalError = new Error("TEST.002", "Original message", ErrorType.Validation, 
            new Dictionary<string, object> { ["Key1"] = "Value1", ["Key2"] = 42 });

        // Act
        var builder = ErrorBuilder.FromError(originalError);
        var builtError = builder.Build();

        // Assert
        Assert.Equal(originalError.Code, builtError.Code);
        Assert.Equal(originalError.Message, builtError.Message);
        Assert.Equal(originalError.Type, builtError.Type);
        Assert.Equal(originalError.Metadata.Count, builtError.Metadata.Count);
        Assert.Equal("Value1", builtError.Metadata["Key1"]);
        Assert.Equal(42, builtError.Metadata["Key2"]);
    }

    [Fact]
    public void FromError_WithNullError_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ErrorBuilder.FromError(null!));
    }

    #endregion

    #region Property Setting Tests

    [Fact]
    public void SetCode_Should_Update_Code_Property()
    {
        // Arrange
        const string initialCode = "INITIAL.001";
        const string newCode = "UPDATED.002";
        var builder = ErrorBuilder.WithCode(initialCode);

        // Act
        builder.SetCode(newCode);
        var error = builder.SetMessage("Test message").Build();

        // Assert
        Assert.Equal(newCode, error.Code);
    }

    [Fact]
    public void SetCode_WithNullOrWhitespace_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.SetCode(null!));
        Assert.Throws<ArgumentException>(() => builder.SetCode(""));
        Assert.Throws<ArgumentException>(() => builder.SetCode("   "));
    }

    [Fact]
    public void SetMessage_Should_Update_Message_Property()
    {
        // Arrange
        const string initialMessage = "Initial message";
        const string newMessage = "Updated message";
        var builder = ErrorBuilder.WithMessage(initialMessage);

        // Act
        builder.SetMessage(newMessage);
        var error = builder.SetCode("TEST.003").Build();

        // Assert
        Assert.Equal(newMessage, error.Message);
    }

    [Fact]
    public void SetMessage_WithNullOrWhitespace_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.SetMessage(null!));
        Assert.Throws<ArgumentException>(() => builder.SetMessage(""));
        Assert.Throws<ArgumentException>(() => builder.SetMessage("   "));
    }

    #endregion

    #region Error Type Tests

    [Fact]
    public void AsGeneral_Should_Set_Error_Type_To_General()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.004")
            .SetMessage("Test message")
            .AsGeneral()
            .Build();

        // Assert
        Assert.Equal(ErrorType.General, error.Type);
    }

    [Fact]
    public void AsValidation_Should_Set_Error_Type_To_Validation()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.005")
            .SetMessage("Test message")
            .AsValidation()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    [Fact]
    public void AsNotFound_Should_Set_Error_Type_To_NotFound()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.006")
            .SetMessage("Test message")
            .AsNotFound()
            .Build();

        // Assert
        Assert.Equal(ErrorType.NotFound, error.Type);
    }

    [Fact]
    public void AsConflict_Should_Set_Error_Type_To_Conflict()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.007")
            .SetMessage("Test message")
            .AsConflict()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Conflict, error.Type);
    }

    [Fact]
    public void AsUnauthorized_Should_Set_Error_Type_To_Unauthorized()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.008")
            .SetMessage("Test message")
            .AsUnauthorized()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Unauthorized, error.Type);
    }

    [Fact]
    public void AsForbidden_Should_Set_Error_Type_To_Forbidden()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.009")
            .SetMessage("Test message")
            .AsForbidden()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Forbidden, error.Type);
    }

    [Fact]
    public void AsInternal_Should_Set_Error_Type_To_Internal()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.010")
            .SetMessage("Test message")
            .AsInternal()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Internal, error.Type);
    }

    [Fact]
    public void AsExternal_Should_Set_Error_Type_To_External()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.011")
            .SetMessage("Test message")
            .AsExternal()
            .Build();

        // Assert
        Assert.Equal(ErrorType.External, error.Type);
    }

    [Fact]
    public void AsCustom_Should_Set_Error_Type_To_Custom()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.012")
            .SetMessage("Test message")
            .AsCustom()
            .Build();

        // Assert
        Assert.Equal(ErrorType.Custom, error.Type);
    }

    [Fact]
    public void OfType_Should_Set_Error_Type_To_Specified_Type()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.013")
            .SetMessage("Test message")
            .OfType(ErrorType.External)
            .Build();

        // Assert
        Assert.Equal(ErrorType.External, error.Type);
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void WithMetadata_Single_Should_Add_Metadata_Entry()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.014")
            .SetMessage("Test message")
            .WithMetadata("UserId", 123)
            .Build();

        // Assert
        Assert.Single(error.Metadata);
        Assert.Equal(123, error.Metadata["UserId"]);
    }

    [Fact]
    public void WithMetadata_Single_WithNullKey_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithMetadata(null!, "value"));
        Assert.Throws<ArgumentException>(() => builder.WithMetadata("", "value"));
        Assert.Throws<ArgumentException>(() => builder.WithMetadata("   ", "value"));
    }

    [Fact]
    public void WithMetadata_Single_WithNullValue_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithMetadata("key", null!));
    }

    [Fact]
    public void WithMetadata_Multiple_Should_Add_All_Metadata_Entries()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42,
            ["Key3"] = DateTime.Today
        };

        // Act
        var error = ErrorBuilder
            .WithCode("TEST.015")
            .SetMessage("Test message")
            .WithMetadata(metadata)
            // .WithMetadata(new TestMetadataProvider())
            .Build();

        // Assert
        Assert.Equal(3, error.Metadata.Count);
        Assert.Equal("Value1", error.Metadata["Key1"]);
        Assert.Equal(42, error.Metadata["Key2"]);
        Assert.Equal(DateTime.Today, error.Metadata["Key3"]);
    }

    [Fact]
    public void WithMetadata_Multiple_WithNullDictionary_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithMetadata((IDictionary<string, object>)null!));
    }

    [Fact]
    public void WithMetadata_Multiple_WithNullKey_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();
        
        // We need to create a mock implementation that simulates null keys
        // since Dictionary<string, object> won't allow null keys to be added
        var mockDictionary = new TestDictionaryWithNullKey();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithMetadata(mockDictionary));
    }

    [Fact]
    public void WithMetadata_Multiple_WithNullValue_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();
        
        // We need to create a mock implementation that simulates null values
        var mockDictionary = new TestDictionaryWithNullValue();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithMetadata(mockDictionary));
    }

    #region Test Helper Classes for Metadata Tests

    /// <summary>
    /// Test helper class that implements IDictionary but allows null keys for testing purposes
    /// </summary>
    private class TestDictionaryWithNullKey : IDictionary<string, object>
    {
        public object this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<string> Keys => new string?[] { null };
        public ICollection<object> Values => new object[] { "value" };
        public int Count => 1;
        public bool IsReadOnly => false;
        
        public void Add(string key, object value) => throw new NotImplementedException();
        public void Add(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public bool ContainsKey(string key) => throw new NotImplementedException();
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(string key) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public bool TryGetValue(string key, out object value) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object>(null!, "value");
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// Test helper class that implements IDictionary but allows null values for testing purposes
    /// </summary>
    private class TestDictionaryWithNullValue : IDictionary<string, object>
    {
        public object this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ICollection<string> Keys => new string[] { "key" };
        public ICollection<object> Values => new object?[] { null! };
        public int Count => 1;
        public bool IsReadOnly => false;
        
        public void Add(string key, object value) => throw new NotImplementedException();
        public void Add(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public bool ContainsKey(string key) => throw new NotImplementedException();
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(string key) => throw new NotImplementedException();
        public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException();
        public bool TryGetValue(string key, out object value) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            yield return new KeyValuePair<string, object>("key", null!);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    #endregion

    [Fact]
    public void WithMetadata_Should_Override_Existing_Keys()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.016")
            .SetMessage("Test message")
            .WithMetadata("Key1", "OriginalValue")
            .WithMetadata("Key1", "UpdatedValue")
            .Build();

        // Assert
        Assert.Single(error.Metadata);
        Assert.Equal("UpdatedValue", error.Metadata["Key1"]);
    }

    [Fact]
    public void RemoveMetadata_Should_Remove_Specified_Key()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.017")
            .SetMessage("Test message")
            .WithMetadata("Key1", "Value1")
            .WithMetadata("Key2", "Value2")
            .RemoveMetadata("Key1")
            .Build();

        // Assert
        Assert.Single(error.Metadata);
        Assert.False(error.Metadata.ContainsKey("Key1"));
        Assert.True(error.Metadata.ContainsKey("Key2"));
    }

    [Fact]
    public void RemoveMetadata_WithNullOrWhitespaceKey_Should_Throw_ArgumentException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.RemoveMetadata(null!));
        Assert.Throws<ArgumentException>(() => builder.RemoveMetadata(""));
        Assert.Throws<ArgumentException>(() => builder.RemoveMetadata("   "));
    }

    [Fact]
    public void ClearMetadata_Should_Remove_All_Metadata()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.018")
            .SetMessage("Test message")
            .WithMetadata("Key1", "Value1")
            .WithMetadata("Key2", "Value2")
            .ClearMetadata()
            .Build();

        // Assert
        Assert.Empty(error.Metadata);
    }

    #endregion

    #region Conditional Building Tests

    [Fact]
    public void WithMetadataIf_WithTrueCondition_Should_Add_Metadata()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.019")
            .SetMessage("Test message")
            .WithMetadataIf(true, "ConditionalKey", "ConditionalValue")
            .Build();

        // Assert
        Assert.Single(error.Metadata);
        Assert.Equal("ConditionalValue", error.Metadata["ConditionalKey"]);
    }

    [Fact]
    public void WithMetadataIf_WithFalseCondition_Should_Not_Add_Metadata()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.020")
            .SetMessage("Test message")
            .WithMetadataIf(false, "ConditionalKey", "ConditionalValue")
            .Build();

        // Assert
        Assert.Empty(error.Metadata);
    }

    [Fact]
    public void WithMetadataIf_WithConditionFactory_Should_Evaluate_Function()
    {
        // Arrange
        var shouldAdd = true;

        // Act
        var error = ErrorBuilder
            .WithCode("TEST.021")
            .SetMessage("Test message")
            .WithMetadataIf(() => shouldAdd, "FunctionKey", "FunctionValue")
            .Build();

        // Assert
        Assert.Single(error.Metadata);
        Assert.Equal("FunctionValue", error.Metadata["FunctionKey"]);
    }

    [Fact]
    public void WithMetadataIf_WithNullConditionFactory_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.WithMetadataIf((Func<bool>)null!, "key", "value"));
    }

    [Fact]
    public void If_WithTrueCondition_Should_Execute_Action()
    {
        // Arrange
        var executed = false;

        // Act
        var error = ErrorBuilder
            .WithCode("TEST.022")
            .SetMessage("Test message")
            .If(true, builder => executed = true)
            .Build();

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void If_WithFalseCondition_Should_Not_Execute_Action()
    {
        // Arrange
        var executed = false;

        // Act
        var error = ErrorBuilder
            .WithCode("TEST.023")
            .SetMessage("Test message")
            .If(false, builder => executed = true)
            .Build();

        // Assert
        Assert.False(executed);
    }

    [Fact]
    public void If_WithConditionFactory_Should_Evaluate_Function()
    {
        // Arrange
        var shouldExecute = true;
        var executed = false;

        // Act
        var error = ErrorBuilder
            .WithCode("TEST.024")
            .SetMessage("Test message")
            .If(() => shouldExecute, builder => executed = true)
            .Build();

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void If_WithNullConditionFactory_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.If((Func<bool>)null!, b => { }));
    }

    [Fact]
    public void If_WithNullAction_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.If(true, (Action<ErrorBuilder>)null!));
        Assert.Throws<ArgumentNullException>(() => builder.If(() => true, (Action<ErrorBuilder>)null!));
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void CanBuild_WithCodeAndMessage_Should_Return_True()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithCode("TEST.025")
            .SetMessage("Test message");

        // Act & Assert
        Assert.True(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithoutCode_Should_Return_False()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithMessage("Test message");

        // Act & Assert
        Assert.False(builder.CanBuild());
    }

    [Fact]
    public void CanBuild_WithoutMessage_Should_Return_False()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithCode("TEST.026");

        // Act & Assert
        Assert.False(builder.CanBuild());
    }

    [Fact]
    public void Validate_WithValidConfiguration_Should_Return_Builder()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithCode("TEST.027")
            .SetMessage("Test message");

        // Act & Assert
        Assert.Same(builder, builder.Validate());
    }

    [Fact]
    public void Validate_WithoutCode_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithMessage("Test message");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Validate());
    }

    [Fact]
    public void Validate_WithoutMessage_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithCode("TEST.028");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Validate());
    }

    [Fact]
    public void Build_WithValidConfiguration_Should_Return_Error()
    {
        // Act
        var error = ErrorBuilder
            .WithCode("TEST.029")
            .SetMessage("Test message")
            .AsValidation()
            .WithMetadata("TestKey", "TestValue")
            .Build();

        // Assert
        Assert.Equal("TEST.029", error.Code);
        Assert.Equal("Test message", error.Message);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Single(error.Metadata);
        Assert.Equal("TestValue", error.Metadata["TestKey"]);
    }

    [Fact]
    public void Build_WithInvalidConfiguration_Should_Throw_InvalidOperationException()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_Should_Call_Build()
    {
        // Act
        Error error = ErrorBuilder
            .WithCode("TEST.030")
            .SetMessage("Test implicit conversion");

        // Assert
        Assert.Equal("TEST.030", error.Code);
        Assert.Equal("Test implicit conversion", error.Message);
    }

    [Fact]
    public void ImplicitConversion_WithNullBuilder_Should_Throw_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            Error error = (ErrorBuilder)null!;
        });
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_Should_Return_Descriptive_String()
    {
        // Arrange
        var builder = ErrorBuilder
            .WithCode("TEST.031")
            .SetMessage("Test message")
            .AsValidation()
            .WithMetadata("Key1", "Value1");

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("TEST.031", result);
        Assert.Contains("Test message", result);
        Assert.Contains("Validation", result);
        Assert.Contains("1 items", result);
    }

    [Fact]
    public void ToString_WithIncompleteBuilder_Should_Show_NotSet()
    {
        // Arrange
        var builder = ErrorBuilder.Create();

        // Act
        var result = builder.ToString();

        // Assert
        Assert.Contains("<not set>", result);
    }

    #endregion

    #region Fluent Chaining Tests

    [Fact]
    public void FluentChaining_Should_Allow_Complex_Configuration()
    {
        // Act
        var error = ErrorBuilder
            .Create()
            .SetCode("COMPLEX.001")
            .SetMessage("Complex error scenario")
            .AsValidation()
            .WithMetadata("UserId", 123)
            .WithMetadata("Action", "UpdateProfile")
            .WithMetadata("Timestamp", DateTime.Now)
            .If(true, builder => builder.WithMetadata("Conditional", "AddedConditionally"))
            .WithMetadataIf(false, "SkippedKey", "SkippedValue")
            .Build();

        // Assert
        Assert.Equal("COMPLEX.001", error.Code);
        Assert.Equal("Complex error scenario", error.Message);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(4, error.Metadata.Count);
        Assert.Equal(123, error.Metadata["UserId"]);
        Assert.Equal("UpdateProfile", error.Metadata["Action"]);
        Assert.True(error.Metadata.ContainsKey("Timestamp"));
        Assert.Equal("AddedConditionally", error.Metadata["Conditional"]);
        Assert.False(error.Metadata.ContainsKey("SkippedKey"));
    }

    #endregion
}