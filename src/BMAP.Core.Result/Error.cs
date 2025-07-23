namespace BMAP.Core.Result;

/// <summary>
///     Represents an error that occurred during an operation.
///     Provides detailed information about the error including code, message, and additional metadata.
/// </summary>
public sealed class Error : IEquatable<Error>
{
    /// <summary>
    ///     Represents no error (used for successful operations).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>
    ///     Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("General.NullValue", "A null value was provided", ErrorType.Validation);

    /// <summary>
    ///     Initializes a new instance of the Error class.
    /// </summary>
    /// <param name="code">The error code that uniquely identifies the type of error.</param>
    /// <param name="message">A human-readable message describing the error.</param>
    /// <param name="type">The type/category of the error.</param>
    /// <param name="metadata">Additional metadata associated with the error.</param>
    public Error(string code, string message, ErrorType type = ErrorType.General, Dictionary<string, object>? metadata = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    ///     Gets the error code that uniquely identifies the type of error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    ///     Gets the human-readable message describing the error.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the type/category of the error.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    ///     Gets additional metadata associated with the error.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    ///     Gets a value indicating whether this error represents no error (success case).
    /// </summary>
    public bool IsNone => Type == ErrorType.None;

    /// <summary>
    ///     Creates a validation error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A validation error.</returns>
    public static Error Validation(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.Validation, metadata);

    /// <summary>
    ///     Creates a not found error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A not found error.</returns>
    public static Error NotFound(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.NotFound, metadata);

    /// <summary>
    ///     Creates a conflict error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A conflict error.</returns>
    public static Error Conflict(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.Conflict, metadata);

    /// <summary>
    ///     Creates an unauthorized error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>An unauthorized error.</returns>
    public static Error Unauthorized(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.Unauthorized, metadata);

    /// <summary>
    ///     Creates a forbidden error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A forbidden error.</returns>
    public static Error Forbidden(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.Forbidden, metadata);

    /// <summary>
    ///     Creates an internal error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>An internal error.</returns>
    public static Error Internal(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.Internal, metadata);

    /// <summary>
    ///     Creates an external error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>An external error.</returns>
    public static Error External(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.External, metadata);

    /// <summary>
    ///     Creates a custom error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A custom error.</returns>
    public static Error Custom(string code, string message, Dictionary<string, object>? metadata = null) =>
        new(code, message, ErrorType.Custom, metadata);

    /// <summary>
    ///     Adds or updates metadata for this error.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>A new Error instance with the updated metadata.</returns>
    public Error WithMetadata(string key, object value)
    {
        var newMetadata = new Dictionary<string, object>(Metadata) { [key] = value };
        return new Error(Code, Message, Type, newMetadata);
    }

    /// <summary>
    ///     Adds or updates multiple metadata entries for this error.
    /// </summary>
    /// <param name="metadata">The metadata to add or update.</param>
    /// <returns>A new Error instance with the updated metadata.</returns>
    public Error WithMetadata(Dictionary<string, object> metadata)
    {
        var newMetadata = new Dictionary<string, object>(Metadata);
        foreach (var kvp in metadata)
        {
            newMetadata[kvp.Key] = kvp.Value;
        }
        return new Error(Code, Message, Type, newMetadata);
    }

    /// <summary>
    ///     Returns a string representation of the error.
    /// </summary>
    /// <returns>A string containing the error code and message.</returns>
    public override string ToString() => $"[{Code}] {Message}";

    /// <summary>
    ///     Determines whether the specified object is equal to the current error.
    /// </summary>
    /// <param name="obj">The object to compare with the current error.</param>
    /// <returns>True if the specified object is equal to the current error; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Error error && Equals(error);

    /// <summary>
    ///     Determines whether the specified Error is equal to the current error.
    /// </summary>
    /// <param name="other">The Error to compare with the current error.</param>
    /// <returns>True if the specified Error is equal to the current error; otherwise, false.</returns>
    public bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && 
               Message == other.Message && 
               Type == other.Type;
    }

    /// <summary>
    ///     Returns the hash code for this error.
    /// </summary>
    /// <returns>The hash code for this error.</returns>
    public override int GetHashCode() => HashCode.Combine(Code, Message, Type);

    /// <summary>
    ///     Determines whether two Error instances are equal.
    /// </summary>
    /// <param name="left">The first Error to compare.</param>
    /// <param name="right">The second Error to compare.</param>
    /// <returns>True if the Error instances are equal; otherwise, false.</returns>
    public static bool operator ==(Error? left, Error? right) => Equals(left, right);

    /// <summary>
    ///     Determines whether two Error instances are not equal.
    /// </summary>
    /// <param name="left">The first Error to compare.</param>
    /// <param name="right">The second Error to compare.</param>
    /// <returns>True if the Error instances are not equal; otherwise, false.</returns>
    public static bool operator !=(Error? left, Error? right) => !Equals(left, right);

    /// <summary>
    ///     Implicitly converts a string to an Error with a general error type.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>An Error with the specified message and a generated code.</returns>
    public static implicit operator Error(string message) => new("General.Error", message);

    /// <summary>
    ///     Implicitly converts a tuple of code and message to an Error.
    /// </summary>
    /// <param name="error">The tuple containing error code and message.</param>
    /// <returns>An Error with the specified code and message.</returns>
    public static implicit operator Error((string Code, string Message) error) => new(error.Code, error.Message);
}