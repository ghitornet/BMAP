using System.Diagnostics.CodeAnalysis;

namespace BMAP.Core.Result;

/// <summary>
///     Represents the outcome of an operation that can either succeed or fail.
///     This is the base class for all Result types in the BMAP.Core.Result library.
/// </summary>
public abstract class ResultBase
{
    /// <summary>
    ///     Initializes a new instance of the ResultBase class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    protected ResultBase(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("A successful result cannot have an error.");
        
        if (!isSuccess && error == null)
            throw new InvalidOperationException("A failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets the error information if the operation failed.
    /// </summary>
    public Error? Error { get; }

    /// <summary>
    ///     Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed Result with the specified error.</returns>
    public static implicit operator ResultBase(Error error) => Result.Failure(error);
}

/// <summary>
///     Represents the outcome of an operation that can either succeed or fail without returning a value.
/// </summary>
public sealed class Result : ResultBase
{
    /// <summary>
    ///     Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    private Result(bool isSuccess, Error? error) : base(isSuccess, error)
    {
    }

    /// <summary>
    ///     Gets a successful result.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    ///     Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result with the specified error message.</returns>
    public static Result Failure(string message) => new(false, new Error("General.Failure", message));

    /// <summary>
    ///     Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result Failure(string code, string message) => new(false, new Error(code, message));

    /// <summary>
    ///     Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed Result with the specified error.</returns>
    public static implicit operator Result(Error error) => Failure(error);

    /// <summary>
    ///     Creates a Result from a boolean value.
    /// </summary>
    /// <param name="isSuccess">The success status.</param>
    /// <param name="error">The error if the operation failed.</param>
    /// <returns>A Result based on the success status.</returns>
    public static Result Create(bool isSuccess, Error? error = null) =>
        isSuccess ? Success() : Failure(error ?? new Error("General.Failure", "Operation failed"));

    /// <summary>
    ///     Creates a Result from a boolean condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="error">The error if the condition is false.</param>
    /// <returns>A successful Result if the condition is true, otherwise a failed Result.</returns>
    public static Result CreateIf(bool condition, Error error) =>
        condition ? Success() : Failure(error);

    /// <summary>
    ///     Creates a Result from a boolean condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorMessage">The error message if the condition is false.</param>
    /// <returns>A successful Result if the condition is true, otherwise a failed Result.</returns>
    public static Result CreateIf(bool condition, string errorMessage) =>
        condition ? Success() : Failure(errorMessage);

    /// <summary>
    ///     Creates a Result from a boolean condition with custom error code.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="errorCode">The error code if the condition is false.</param>
    /// <param name="errorMessage">The error message if the condition is false.</param>
    /// <returns>A successful Result if the condition is true, otherwise a failed Result.</returns>
    public static Result CreateIf(bool condition, string errorCode, string errorMessage) =>
        condition ? Success() : Failure(errorCode, errorMessage);
}

/// <summary>
///     Represents the outcome of an operation that can either succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
public sealed class Result<TValue> : ResultBase
{
    private readonly TValue? _value;

    /// <summary>
    ///     Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="value">The value if the operation was successful.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error information if the operation failed.</param>
    private Result(TValue? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    ///     Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when trying to access the value of a failed result.</exception>
    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access the value of a failed result. Check IsSuccess before accessing Value.");

    /// <summary>
    ///     Tries to get the value if the operation was successful.
    /// </summary>
    /// <param name="value">The value if the operation was successful.</param>
    /// <returns>True if the operation was successful and the value is available, otherwise false.</returns>
    public bool TryGetValue([NotNullWhen(true)] out TValue? value)
    {
        value = IsSuccess ? _value : default;
        return IsSuccess;
    }

    /// <summary>
    ///     Gets the value if the operation was successful, otherwise returns the default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the operation failed.</param>
    /// <returns>The value if successful, otherwise the default value.</returns>
    public TValue GetValueOrDefault(TValue defaultValue) => IsSuccess ? _value! : defaultValue;

    /// <summary>
    ///     Gets the value if the operation was successful, otherwise returns the default value of the type.
    /// </summary>
    /// <returns>The value if successful, otherwise the default value of TValue.</returns>
    public TValue? GetValueOrDefault() => IsSuccess ? _value : default;

    /// <summary>
    ///     Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    /// <returns>A successful result containing the specified value.</returns>
    public static Result<TValue> Success(TValue value) => new(value, true, null);

    /// <summary>
    ///     Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result<TValue> Failure(Error error) => new(default, false, error);

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result with the specified error message.</returns>
    public static Result<TValue> Failure(string message) => new(default, false, new Error("General.Failure", message));

    /// <summary>
    ///     Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result<TValue> Failure(string code, string message) => new(default, false, new Error(code, message));

    /// <summary>
    ///     Implicitly converts a value to a successful Result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful Result containing the value.</returns>
    public static implicit operator Result<TValue>(TValue value) => Success(value);

    /// <summary>
    ///     Implicitly converts an Error to a failed Result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed Result with the specified error.</returns>
    public static implicit operator Result<TValue>(Error error) => Failure(error);

    /// <summary>
    ///     Creates a Result from a nullable value.
    /// </summary>
    /// <param name="value">The nullable value.</param>
    /// <param name="error">The error if the value is null.</param>
    /// <returns>A successful Result if the value is not null, otherwise a failed Result.</returns>
    public static Result<TValue> Create(TValue? value, Error error) =>
        value is not null ? Success(value) : Failure(error);

    /// <summary>
    ///     Creates a Result from a nullable value with a default error message.
    /// </summary>
    /// <param name="value">The nullable value.</param>
    /// <param name="errorMessage">The error message if the value is null.</param>
    /// <returns>A successful Result if the value is not null, otherwise a failed Result.</returns>
    public static Result<TValue> Create(TValue? value, string errorMessage) =>
        value is not null ? Success(value) : Failure(errorMessage);

    /// <summary>
    ///     Creates a Result from a nullable value with custom error code and message.
    /// </summary>
    /// <param name="value">The nullable value.</param>
    /// <param name="errorCode">The error code if the value is null.</param>
    /// <param name="errorMessage">The error message if the value is null.</param>
    /// <returns>A successful Result if the value is not null, otherwise a failed Result.</returns>
    public static Result<TValue> Create(TValue? value, string errorCode, string errorMessage) =>
        value is not null ? Success(value) : Failure(errorCode, errorMessage);
}