namespace BMAP.Core.Result.Builders;

/// <summary>
///     Provides a fluent builder interface for creating Error instances with a readable and composable API.
///     This builder enables step-by-step construction of complex Error objects with validation and method chaining.
/// </summary>
/// <remarks>
///     The ErrorBuilder follows the Fluent Builder pattern to provide an intuitive way to construct Error objects.
///     It supports method chaining, conditional building, and validation to ensure consistent error creation.
///     
///     Example usage:
///     <code>
///     var error = ErrorBuilder
///         .WithCode("USER_NOT_FOUND")
///         .WithMessage("User with the specified ID was not found")
///         .AsNotFound()
///         .WithMetadata("UserId", 123)
///         .WithMetadata("Timestamp", DateTime.UtcNow)
///         .Build();
///     </code>
/// </remarks>
public sealed class ErrorBuilder
{
    private string? _code;
    private string? _message;
    private ErrorType _type = ErrorType.General;
    private readonly Dictionary<string, object> _metadata = new();

    /// <summary>
    ///     Initializes a new instance of the ErrorBuilder class.
    /// </summary>
    private ErrorBuilder()
    {
    }

    /// <summary>
    ///     Creates a new ErrorBuilder instance.
    /// </summary>
    /// <returns>A new ErrorBuilder instance ready for configuration.</returns>
    public static ErrorBuilder Create() => new();

    /// <summary>
    ///     Creates a new ErrorBuilder instance with the specified error code.
    /// </summary>
    /// <param name="code">The error code that uniquely identifies the type of error.</param>
    /// <returns>An ErrorBuilder instance with the specified code.</returns>
    /// <exception cref="ArgumentException">Thrown when the code is null or whitespace.</exception>
    public static ErrorBuilder WithCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        return new ErrorBuilder { _code = code };
    }

    /// <summary>
    ///     Creates a new ErrorBuilder instance with the specified error message.
    /// </summary>
    /// <param name="message">A human-readable message describing the error.</param>
    /// <returns>An ErrorBuilder instance with the specified message.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or whitespace.</exception>
    public static ErrorBuilder WithMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        return new ErrorBuilder { _message = message };
    }

    /// <summary>
    ///     Creates a new ErrorBuilder instance from an existing Error.
    /// </summary>
    /// <param name="error">The error to copy properties from.</param>
    /// <returns>An ErrorBuilder instance initialized with the properties of the specified error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public static ErrorBuilder FromError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var builder = new ErrorBuilder
        {
            _code = error.Code,
            _message = error.Message,
            _type = error.Type
        };

        foreach (var kvp in error.Metadata)
        {
            builder._metadata[kvp.Key] = kvp.Value;
        }

        return builder;
    }

    /// <summary>
    ///     Sets the error code for the error being built.
    /// </summary>
    /// <param name="code">The error code that uniquely identifies the type of error.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the code is null or whitespace.</exception>
    public ErrorBuilder SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        _code = code;
        return this;
    }

    /// <summary>
    ///     Sets the error message for the error being built.
    /// </summary>
    /// <param name="message">A human-readable message describing the error.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or whitespace.</exception>
    public ErrorBuilder SetMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        _message = message;
        return this;
    }

    /// <summary>
    ///     Sets the error type to General.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsGeneral()
    {
        _type = ErrorType.General;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Validation.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsValidation()
    {
        _type = ErrorType.Validation;
        return this;
    }

    /// <summary>
    ///     Sets the error type to NotFound.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsNotFound()
    {
        _type = ErrorType.NotFound;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Conflict.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsConflict()
    {
        _type = ErrorType.Conflict;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Unauthorized.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsUnauthorized()
    {
        _type = ErrorType.Unauthorized;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Forbidden.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsForbidden()
    {
        _type = ErrorType.Forbidden;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Internal.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsInternal()
    {
        _type = ErrorType.Internal;
        return this;
    }

    /// <summary>
    ///     Sets the error type to External.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsExternal()
    {
        _type = ErrorType.External;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Custom.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder AsCustom()
    {
        _type = ErrorType.Custom;
        return this;
    }

    /// <summary>
    ///     Sets the error type to the specified value.
    /// </summary>
    /// <param name="errorType">The error type to set.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder OfType(ErrorType errorType)
    {
        _type = errorType;
        return this;
    }

    /// <summary>
    ///     Adds a metadata key-value pair to the error.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public ErrorBuilder WithMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or whitespace.", nameof(key));

        ArgumentNullException.ThrowIfNull(value);

        _metadata[key] = value;
        return this;
    }

    /// <summary>
    ///     Adds multiple metadata entries to the error.
    /// </summary>
    /// <param name="metadata">A dictionary containing the metadata to add.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the metadata dictionary is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any key in the metadata dictionary is null or whitespace.</exception>
    public ErrorBuilder WithMetadata(IDictionary<string, object> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        foreach (var kvp in metadata)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
                throw new ArgumentException("Metadata key cannot be null or whitespace.", nameof(metadata));

            ArgumentNullException.ThrowIfNull(kvp.Value, $"Metadata value for key '{kvp.Key}' cannot be null.");

            _metadata[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    ///     Conditionally adds metadata to the error if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="key">The metadata key to add if the condition is true.</param>
    /// <param name="value">The metadata value to add if the condition is true.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace and the condition is true.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the value is null and the condition is true.</exception>
    public ErrorBuilder WithMetadataIf(bool condition, string key, object value)
    {
        if (condition)
        {
            WithMetadata(key, value);
        }

        return this;
    }

    /// <summary>
    ///     Conditionally adds metadata to the error based on a function that evaluates to a condition.
    /// </summary>
    /// <param name="conditionFactory">A function that returns true if the metadata should be added.</param>
    /// <param name="key">The metadata key to add if the condition is true.</param>
    /// <param name="value">The metadata value to add if the condition is true.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the conditionFactory is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace and the condition is true.</exception>
    public ErrorBuilder WithMetadataIf(Func<bool> conditionFactory, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(conditionFactory);

        if (conditionFactory())
        {
            WithMetadata(key, value);
        }

        return this;
    }

    /// <summary>
    ///     Conditionally executes an action on the builder if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="configure">The action to execute on the builder if the condition is true.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configure action is null.</exception>
    public ErrorBuilder If(bool condition, Action<ErrorBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        if (condition)
        {
            configure(this);
        }

        return this;
    }

    /// <summary>
    ///     Conditionally executes an action on the builder based on a function that evaluates to a condition.
    /// </summary>
    /// <param name="conditionFactory">A function that returns true if the action should be executed.</param>
    /// <param name="configure">The action to execute on the builder if the condition is true.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the conditionFactory or configure action is null.</exception>
    public ErrorBuilder If(Func<bool> conditionFactory, Action<ErrorBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(conditionFactory);
        ArgumentNullException.ThrowIfNull(configure);

        if (conditionFactory())
        {
            configure(this);
        }

        return this;
    }

    /// <summary>
    ///     Removes a metadata entry with the specified key.
    /// </summary>
    /// <param name="key">The metadata key to remove.</param>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace.</exception>
    public ErrorBuilder RemoveMetadata(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or whitespace.", nameof(key));

        _metadata.Remove(key);
        return this;
    }

    /// <summary>
    ///     Clears all metadata from the error being built.
    /// </summary>
    /// <returns>The current ErrorBuilder instance for method chaining.</returns>
    public ErrorBuilder ClearMetadata()
    {
        _metadata.Clear();
        return this;
    }

    /// <summary>
    ///     Gets a value indicating whether the builder has a valid configuration to build an Error.
    /// </summary>
    /// <returns>True if the builder can create a valid Error; otherwise, false.</returns>
    public bool CanBuild()
    {
        return !string.IsNullOrWhiteSpace(_code) && !string.IsNullOrWhiteSpace(_message);
    }

    /// <summary>
    ///     Validates the current state of the builder and throws an exception if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public ErrorBuilder Validate()
    {
        if (string.IsNullOrWhiteSpace(_code))
            throw new InvalidOperationException("Error code must be specified before building the error.");

        if (string.IsNullOrWhiteSpace(_message))
            throw new InvalidOperationException("Error message must be specified before building the error.");

        return this;
    }

    /// <summary>
    ///     Builds and returns the configured Error instance.
    /// </summary>
    /// <returns>A new Error instance with the configured properties.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public Error Build()
    {
        Validate();

        return new Error(_code!, _message!, _type, new Dictionary<string, object>(_metadata));
    }

    /// <summary>
    ///     Implicitly converts the ErrorBuilder to an Error by calling Build().
    /// </summary>
    /// <param name="builder">The ErrorBuilder to convert.</param>
    /// <returns>The built Error instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the builder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public static implicit operator Error(ErrorBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }

    /// <summary>
    ///     Returns a string representation of the ErrorBuilder's current state.
    /// </summary>
    /// <returns>A string describing the current configuration of the builder.</returns>
    public override string ToString()
    {
        var code = string.IsNullOrWhiteSpace(_code) ? "<not set>" : _code;
        var message = string.IsNullOrWhiteSpace(_message) ? "<not set>" : _message;
        return $"ErrorBuilder [Code: {code}, Message: {message}, Type: {_type}, Metadata: {_metadata.Count} items]";
    }
}