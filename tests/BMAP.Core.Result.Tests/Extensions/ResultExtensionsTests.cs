using BMAP.Core.Result;
using BMAP.Core.Result.Extensions;

namespace BMAP.Core.Result.Tests.Extensions;

/// <summary>
/// Unit tests for Result extension methods.
/// Tests cover Bind, Map, Match, Tap, Ensure, and async operations.
/// </summary>
public class ResultExtensionsTests
{
    #region Bind Tests

    [Fact]
    public void Bind_Success_Should_Execute_Function()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        var bindResult = result.Bind(() =>
        {
            executed = true;
            return Result.Success();
        });

        // Assert
        Assert.True(executed);
        Assert.True(bindResult.IsSuccess);
    }

    [Fact]
    public void Bind_Failure_Should_Not_Execute_Function()
    {
        // Arrange
        var error = new Error("TEST.001", "Initial error");
        var result = Result.Failure(error);
        var executed = false;

        // Act
        var bindResult = result.Bind(() =>
        {
            executed = true;
            return Result.Success();
        });

        // Assert
        Assert.False(executed);
        Assert.True(bindResult.IsFailure);
        Assert.Equal(error, bindResult.Error);
    }

    [Fact]
    public void Bind_Success_To_Failure_Should_Return_Failure()
    {
        // Arrange
        var result = Result.Success();
        var bindError = new Error("BIND.001", "Bind operation failed");

        // Act
        var bindResult = result.Bind(() => Result.Failure(bindError));

        // Assert
        Assert.True(bindResult.IsFailure);
        Assert.Equal(bindError, bindResult.Error);
    }

    [Fact]
    public void Bind_Generic_Result_To_Value_Result_Should_Work()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var bindResult = result.Bind(() => Result<string>.Success("Bound value"));

        // Assert
        Assert.True(bindResult.IsSuccess);
        Assert.Equal("Bound value", bindResult.Value);
    }

    [Fact]
    public void Bind_Value_Result_Success_Should_Execute_Function_With_Value()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var receivedValue = 0;

        // Act
        var bindResult = result.Bind(value =>
        {
            receivedValue = value;
            return Result.Success();
        });

        // Assert
        Assert.Equal(42, receivedValue);
        Assert.True(bindResult.IsSuccess);
    }

    [Fact]
    public void Bind_Value_Result_To_Different_Value_Result_Should_Work()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var bindResult = result.Bind(value => Result<string>.Success($"Value: {value}"));

        // Assert
        Assert.True(bindResult.IsSuccess);
        Assert.Equal("Value: 42", bindResult.Value);
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_Success_Should_Transform_Value()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mappedResult = result.Map(value => $"Number: {value}");

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("Number: 42", mappedResult.Value);
    }

    [Fact]
    public void Map_Failure_Should_Not_Transform_Value()
    {
        // Arrange
        var error = new Error("TEST.002", "Map test error");
        var result = Result<int>.Failure(error);

        // Act
        var mappedResult = result.Map(value => $"Number: {value}");

        // Assert
        Assert.True(mappedResult.IsFailure);
        Assert.Equal(error, mappedResult.Error);
    }

    [Fact]
    public void Map_Non_Generic_Result_Should_Create_Value_Result()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var mappedResult = result.Map(() => "Created value");

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("Created value", mappedResult.Value);
    }

    [Fact]
    public void Map_Non_Generic_Failure_Should_Return_Failure()
    {
        // Arrange
        var error = new Error("TEST.003", "Map non-generic test error");
        var result = Result.Failure(error);

        // Act
        var mappedResult = result.Map(() => "Should not be created");

        // Assert
        Assert.True(mappedResult.IsFailure);
        Assert.Equal(error, mappedResult.Error);
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_Success_Should_Execute_OnSuccess_Function()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var matchResult = result.Match(
            () => "Success",
            error => $"Error: {error.Message}"
        );

        // Assert
        Assert.Equal("Success", matchResult);
    }

    [Fact]
    public void Match_Failure_Should_Execute_OnFailure_Function()
    {
        // Arrange
        var error = new Error("TEST.004", "Match test error");
        var result = Result.Failure(error);

        // Act
        var matchResult = result.Match(
            () => "Success",
            err => $"Error: {err.Message}"
        );

        // Assert
        Assert.Equal("Error: Match test error", matchResult);
    }

    [Fact]
    public void Match_Value_Result_Success_Should_Execute_OnSuccess_With_Value()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var matchResult = result.Match(
            value => $"Value: {value}",
            error => $"Error: {error.Message}"
        );

        // Assert
        Assert.Equal("Value: 42", matchResult);
    }

    [Fact]
    public void Match_Action_Success_Should_Execute_OnSuccess_Action()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        result.Match(
            () => executed = true,
            error => executed = false
        );

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void Match_Action_Failure_Should_Execute_OnFailure_Action()
    {
        // Arrange
        var error = new Error("TEST.005", "Match action test");
        var result = Result.Failure(error);
        Error? receivedError = null;

        // Act
        result.Match(
            () => { },
            err => receivedError = err
        );

        // Assert
        Assert.Equal(error, receivedError);
    }

    #endregion

    #region Tap Tests

    [Fact]
    public void Tap_Success_Should_Execute_Action_And_Return_Original_Result()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        var tappedResult = result.Tap(() => executed = true);

        // Assert
        Assert.True(executed);
        Assert.Same(result, tappedResult);
        Assert.True(tappedResult.IsSuccess);
    }

    [Fact]
    public void Tap_Failure_Should_Not_Execute_Action()
    {
        // Arrange
        var error = new Error("TEST.006", "Tap test error");
        var result = Result.Failure(error);
        var executed = false;

        // Act
        var tappedResult = result.Tap(() => executed = true);

        // Assert
        Assert.False(executed);
        Assert.Same(result, tappedResult);
    }

    [Fact]
    public void Tap_Value_Result_Success_Should_Execute_Action_With_Value()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var receivedValue = 0;

        // Act
        var tappedResult = result.Tap(value => receivedValue = value);

        // Assert
        Assert.Equal(42, receivedValue);
        Assert.Same(result, tappedResult);
    }

    [Fact]
    public void TapError_Success_Should_Not_Execute_Action()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        var tappedResult = result.TapError(error => executed = true);

        // Assert
        Assert.False(executed);
        Assert.Same(result, tappedResult);
    }

    [Fact]
    public void TapError_Failure_Should_Execute_Action_With_Error()
    {
        // Arrange
        var error = new Error("TEST.007", "TapError test");
        var result = Result.Failure(error);
        Error? receivedError = null;

        // Act
        var tappedResult = result.TapError(err => receivedError = err);

        // Assert
        Assert.Equal(error, receivedError);
        Assert.Same(result, tappedResult);
    }

    #endregion

    #region Ensure Tests

    [Fact]
    public void Ensure_Success_With_True_Predicate_Should_Return_Success()
    {
        // Arrange
        var result = Result.Success();
        var error = new Error("ENSURE.001", "Should not be used");

        // Act
        var ensuredResult = result.Ensure(() => true, error);

        // Assert
        Assert.True(ensuredResult.IsSuccess);
    }

    [Fact]
    public void Ensure_Success_With_False_Predicate_Should_Return_Failure()
    {
        // Arrange
        var result = Result.Success();
        var error = new Error("ENSURE.002", "Predicate failed");

        // Act
        var ensuredResult = result.Ensure(() => false, error);

        // Assert
        Assert.True(ensuredResult.IsFailure);
        Assert.Equal(error, ensuredResult.Error);
    }

    [Fact]
    public void Ensure_Failure_Should_Return_Original_Failure()
    {
        // Arrange
        var originalError = new Error("ORIGINAL.001", "Original error");
        var result = Result.Failure(originalError);
        var ensureError = new Error("ENSURE.003", "Should not be used");

        // Act
        var ensuredResult = result.Ensure(() => true, ensureError);

        // Assert
        Assert.True(ensuredResult.IsFailure);
        Assert.Equal(originalError, ensuredResult.Error);
    }

    [Fact]
    public void Ensure_Value_Result_Success_With_True_Predicate_Should_Return_Success()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var error = new Error("ENSURE.004", "Should not be used");

        // Act
        var ensuredResult = result.Ensure(value => value > 0, error);

        // Assert
        Assert.True(ensuredResult.IsSuccess);
        Assert.Equal(42, ensuredResult.Value);
    }

    [Fact]
    public void Ensure_Value_Result_Success_With_False_Predicate_Should_Return_Failure()
    {
        // Arrange
        var result = Result<int>.Success(-1);
        var error = new Error("ENSURE.005", "Value must be positive");

        // Act
        var ensuredResult = result.Ensure(value => value > 0, error);

        // Assert
        Assert.True(ensuredResult.IsFailure);
        Assert.Equal(error, ensuredResult.Error);
    }

    #endregion

    #region Async Tests

    [Fact]
    public async Task BindAsync_Success_Should_Execute_Async_Function()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var bindResult = await result.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        Assert.True(bindResult.IsSuccess);
    }

    [Fact]
    public async Task BindAsync_Failure_Should_Not_Execute_Async_Function()
    {
        // Arrange
        var error = new Error("ASYNC.001", "Async test error");
        var result = Result.Failure(error);

        // Act
        var bindResult = await result.BindAsync(() => Task.FromResult(Result.Success()));

        // Assert
        Assert.True(bindResult.IsFailure);
        Assert.Equal(error, bindResult.Error);
    }

    [Fact]
    public async Task BindAsync_Value_Result_Should_Work()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var bindResult = await result.BindAsync(value => 
            Task.FromResult(Result<string>.Success($"Async: {value}")));

        // Assert
        Assert.True(bindResult.IsSuccess);
        Assert.Equal("Async: 42", bindResult.Value);
    }

    [Fact]
    public async Task MapAsync_Success_Should_Transform_Value_Async()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var mappedResult = await result.MapAsync(value => 
            Task.FromResult($"Async: {value}"));

        // Assert
        Assert.True(mappedResult.IsSuccess);
        Assert.Equal("Async: 42", mappedResult.Value);
    }

    [Fact]
    public async Task TapAsync_Success_Should_Execute_Async_Action()
    {
        // Arrange
        var result = Result.Success();
        var executed = false;

        // Act
        var tappedResult = await result.TapAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(executed);
        Assert.Same(result, tappedResult);
    }

    [Fact]
    public async Task TapAsync_Value_Result_Should_Execute_Async_Action_With_Value()
    {
        // Arrange
        var result = Result<int>.Success(42);
        var receivedValue = 0;

        // Act
        var tappedResult = await result.TapAsync(value =>
        {
            receivedValue = value;
            return Task.CompletedTask;
        });

        // Assert
        Assert.Equal(42, receivedValue);
        Assert.Same(result, tappedResult);
    }

    #endregion

    #region Combine Tests

    [Fact]
    public void Combine_All_Success_Should_Return_Success()
    {
        // Arrange
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        // Act
        var combinedResult = ResultExtensions.Combine(result1, result2, result3);

        // Assert
        Assert.True(combinedResult.IsSuccess);
    }

    [Fact]
    public void Combine_With_Failure_Should_Return_First_Failure()
    {
        // Arrange
        var result1 = Result.Success();
        var error2 = new Error("COMBINE.001", "First failure");
        var result2 = Result.Failure(error2);
        var error3 = new Error("COMBINE.002", "Second failure");
        var result3 = Result.Failure(error3);

        // Act
        var combinedResult = ResultExtensions.Combine(result1, result2, result3);

        // Assert
        Assert.True(combinedResult.IsFailure);
        Assert.Equal(error2, combinedResult.Error);
    }

    [Fact]
    public void Combine_Enumerable_Should_Work()
    {
        // Arrange
        var results = new[]
        {
            Result.Success(),
            Result.Success(),
            Result.Success()
        };

        // Act
        var combinedResult = ResultExtensions.Combine(results);

        // Assert
        Assert.True(combinedResult.IsSuccess);
    }

    [Fact]
    public void Combine_Empty_Enumerable_Should_Return_Success()
    {
        // Arrange
        var results = Array.Empty<Result>();

        // Act
        var combinedResult = ResultExtensions.Combine(results);

        // Assert
        Assert.True(combinedResult.IsSuccess);
    }

    #endregion

    #region Complex Chaining Tests

    [Fact]
    public void Complex_Chaining_Should_Work()
    {
        // Arrange
        var initialResult = Result<int>.Success(10);

        // Act
        var finalResult = initialResult
            .Ensure(x => x > 0, new Error("NEGATIVE", "Value must be positive"))
            .Map(x => x * 2)
            .Bind(x => x > 15 ? Result<string>.Success($"Large: {x}") : Result<string>.Failure("TOO_SMALL", "Value too small"))
            .Tap(value => { /* Could log here */ });

        // Assert
        Assert.True(finalResult.IsSuccess);
        Assert.Equal("Large: 20", finalResult.Value);
    }

    [Fact]
    public void Complex_Chaining_With_Failure_Should_Short_Circuit()
    {
        // Arrange
        var initialResult = Result<int>.Success(-5);
        var mapExecuted = false;
        var bindExecuted = false;

        // Act
        var finalResult = initialResult
            .Ensure(x => x > 0, new Error("NEGATIVE", "Value must be positive"))
            .Map(x => { mapExecuted = true; return x * 2; })
            .Bind(x => { bindExecuted = true; return Result<string>.Success($"Value: {x}"); });

        // Assert
        Assert.True(finalResult.IsFailure);
        Assert.Equal("NEGATIVE", finalResult.Error!.Code);
        Assert.False(mapExecuted);
        Assert.False(bindExecuted);
    }

    #endregion
}