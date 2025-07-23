namespace BMAP.Core.Result.Builders;

/// <summary>
///     Provides a fluent builder interface for creating Result instances with a readable and composable API.
///     This builder enables step-by-step construction of Result objects with validation and method chaining.
/// </summary>
/// <remarks>
///     The ResultBuilder follows the Fluent Builder pattern to provide an intuitive way to construct Result objects.
///     It supports method chaining, conditional building, and validation to ensure consistent result creation.
///     
///     Example usage:
///     <code>
///     var result = ResultBuilder
///         .Success()
///         .Build();
///     
///     var failureResult = ResultBuilder
///         .Failure()
///         .WithError("VALIDATION_FAILED", "Input validation failed")
///         .AsValidation()
///         .WithErrorMetadata("Field", "Email")
///         .Build();
///     </code>
/// </remarks>
public sealed class ResultBuilder
{
    private bool _isSuccess = true;
    private Error? _error;

    /// <summary>
    ///     Initializes a new instance of the ResultBuilder class.
    /// </summary>
    private ResultBuilder()
    {
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance configured for success.
    /// </summary>
    /// <returns>A ResultBuilder instance configured for a successful result.</returns>
    public static ResultBuilder Success()
    {
        return new ResultBuilder { _isSuccess = true };
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance configured for failure.
    /// </summary>
    /// <returns>A ResultBuilder instance configured for a failed result.</returns>
    public static ResultBuilder Failure()
    {
        return new ResultBuilder { _isSuccess = false };
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance configured for failure with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A ResultBuilder instance configured for a failed result with the specified error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public static ResultBuilder Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ResultBuilder 
        { 
            _isSuccess = false,
            _error = error
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance configured for failure with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A ResultBuilder instance configured for a failed result with the specified error message.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or whitespace.</exception>
    public static ResultBuilder Failure(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        return new ResultBuilder 
        { 
            _isSuccess = false,
            _error = new Error("General.Failure", message)
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance configured for failure with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A ResultBuilder instance configured for a failed result with the specified error.</returns>
    /// <exception cref="ArgumentException">Thrown when the code or message is null or whitespace.</exception>
    public static ResultBuilder Failure(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        return new ResultBuilder 
        { 
            _isSuccess = false,
            _error = new Error(code, message)
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance from an existing Result.
    /// </summary>
    /// <param name="result">The result to copy properties from.</param>
    /// <returns>A ResultBuilder instance initialized with the properties of the specified result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the result is null.</exception>
    public static ResultBuilder FromResult(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ResultBuilder
        {
            _isSuccess = result.IsSuccess,
            _error = result.Error
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance based on a condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>A ResultBuilder configured for success if condition is true, otherwise for failure.</returns>
    public static ResultBuilder Create(bool condition)
    {
        return condition ? Success() : Failure();
    }

    /// <summary>
    ///     Creates a new ResultBuilder instance based on a condition with a specific error for failure.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="error">The error to use if the condition is false.</param>
    /// <returns>A ResultBuilder configured for success if condition is true, otherwise for failure with the specified error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public static ResultBuilder Create(bool condition, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return condition ? Success() : Failure(error);
    }

    /// <summary>
    ///     Sets the error for the result being built.
    /// </summary>
    /// <param name="error">The error to set.</param>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public ResultBuilder WithError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        _error = error;
        _isSuccess = false;
        return this;
    }

    /// <summary>
    ///     Sets the error for the result being built using code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the code or message is null or whitespace.</exception>
    public ResultBuilder WithError(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        _error = new Error(code, message);
        _isSuccess = false;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Validation for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsValidation()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsValidation().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to NotFound for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsNotFound()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsNotFound().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Conflict for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsConflict()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsConflict().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Unauthorized for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsUnauthorized()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsUnauthorized().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Forbidden for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsForbidden()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsForbidden().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Internal for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsInternal()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsInternal().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to External for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsExternal()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsExternal().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Custom for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder AsCustom()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsCustom().Build();
        return this;
    }

    /// <summary>
    ///     Adds metadata to the current error.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public ResultBuilder WithErrorMetadata(string key, object value)
    {
        EnsureErrorExists();

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or whitespace.", nameof(key));

        ArgumentNullException.ThrowIfNull(value);

        _error = ErrorBuilder.FromError(_error!).WithMetadata(key, value).Build();
        return this;
    }

    /// <summary>
    ///     Adds multiple metadata entries to the current error.
    /// </summary>
    /// <param name="metadata">A dictionary containing the metadata to add.</param>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the metadata dictionary is null.</exception>
    public ResultBuilder WithErrorMetadata(IDictionary<string, object> metadata)
    {
        EnsureErrorExists();
        ArgumentNullException.ThrowIfNull(metadata);

        _error = ErrorBuilder.FromError(_error!).WithMetadata(metadata).Build();
        return this;
    }

    /// <summary>
    ///     Conditionally executes an action on the builder if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="configure">The action to execute on the builder if the condition is true.</param>
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configure action is null.</exception>
    public ResultBuilder If(bool condition, Action<ResultBuilder> configure)
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
    /// <returns>The current ResultBuilder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the conditionFactory or configure action is null.</exception>
    public ResultBuilder If(Func<bool> conditionFactory, Action<ResultBuilder> configure)
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
    ///     Gets a value indicating whether the builder has a valid configuration to build a Result.
    /// </summary>
    /// <returns>True if the builder can create a valid Result; otherwise, false.</returns>
    public bool CanBuild()
    {
        return _isSuccess || (_error != null);
    }

    /// <summary>
    ///     Validates the current state of the builder and throws an exception if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public ResultBuilder Validate()
    {
        if (!_isSuccess && _error == null)
            throw new InvalidOperationException("Failed result must have an error specified.");

        return this;
    }

    /// <summary>
    ///     Builds and returns the configured Result instance.
    /// </summary>
    /// <returns>A new Result instance with the configured properties.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public Result Build()
    {
        Validate();

        return _isSuccess ? Result.Success() : Result.Failure(_error!);
    }

    /// <summary>
    ///     Implicitly converts the ResultBuilder to a Result by calling Build().
    /// </summary>
    /// <param name="builder">The ResultBuilder to convert.</param>
    /// <returns>The built Result instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the builder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public static implicit operator Result(ResultBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }

    /// <summary>
    ///     Returns a string representation of the ResultBuilder's current state.
    /// </summary>
    /// <returns>A string describing the current configuration of the builder.</returns>
    public override string ToString()
    {
        var status = _isSuccess ? "Success" : "Failure";
        var errorInfo = _error != null ? $" (Error: {_error.Code})" : "";
        return $"ResultBuilder [{status}{errorInfo}]";
    }

    /// <summary>
    ///     Ensures that an error has been set for operations that require it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    private void EnsureErrorExists()
    {
        if (_error == null)
            throw new InvalidOperationException("Error must be set before modifying error properties.");
    }
}

/// <summary>
///     Provides a fluent builder interface for creating Result&lt;T&gt; instances with a readable and composable API.
///     This builder enables step-by-step construction of Result objects with values and validation.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
/// <remarks>
///     The ResultBuilder&lt;T&gt; follows the Fluent Builder pattern to provide an intuitive way to construct Result&lt;T&gt; objects.
///     It supports method chaining, conditional building, and validation to ensure consistent result creation.
///     
///     Example usage:
///     <code>
///     var result = ResultBuilder&lt;User&gt;
///         .Success(user)
///         .Build();
///     
///     var failureResult = ResultBuilder&lt;User&gt;
///         .Failure()
///         .WithError("USER_NOT_FOUND", "User with specified ID was not found")
///         .AsNotFound()
///         .WithErrorMetadata("UserId", userId)
///         .Build();
///     </code>
/// </remarks>
public sealed class ResultBuilder<T>
{
    private bool _isSuccess = true;
    private T? _value;
    private Error? _error;

    /// <summary>
    ///     Initializes a new instance of the ResultBuilder&lt;T&gt; class.
    /// </summary>
    private ResultBuilder()
    {
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance configured for success with the specified value.
    /// </summary>
    /// <param name="value">The value to include in the successful result.</param>
    /// <returns>A ResultBuilder&lt;T&gt; instance configured for a successful result with the specified value.</returns>
    public static ResultBuilder<T> Success(T value)
    {
        return new ResultBuilder<T> 
        { 
            _isSuccess = true,
            _value = value
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance configured for failure.
    /// </summary>
    /// <returns>A ResultBuilder&lt;T&gt; instance configured for a failed result.</returns>
    public static ResultBuilder<T> Failure()
    {
        return new ResultBuilder<T> { _isSuccess = false };
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance configured for failure with the specified error.
    /// </summary>
    /// <param name="error">The error that caused the failure.</param>
    /// <returns>A ResultBuilder&lt;T&gt; instance configured for a failed result with the specified error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public static ResultBuilder<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ResultBuilder<T> 
        { 
            _isSuccess = false,
            _error = error
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance configured for failure with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A ResultBuilder&lt;T&gt; instance configured for a failed result with the specified error message.</returns>
    /// <exception cref="ArgumentException">Thrown when the message is null or whitespace.</exception>
    public static ResultBuilder<T> Failure(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        return new ResultBuilder<T> 
        { 
            _isSuccess = false,
            _error = new Error("General.Failure", message)
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance configured for failure with the specified error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A ResultBuilder&lt;T&gt; instance configured for a failed result with the specified error.</returns>
    /// <exception cref="ArgumentException">Thrown when the code or message is null or whitespace.</exception>
    public static ResultBuilder<T> Failure(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        return new ResultBuilder<T> 
        { 
            _isSuccess = false,
            _error = new Error(code, message)
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance from an existing Result&lt;T&gt;.
    /// </summary>
    /// <param name="result">The result to copy properties from.</param>
    /// <returns>A ResultBuilder&lt;T&gt; instance initialized with the properties of the specified result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the result is null.</exception>
    public static ResultBuilder<T> FromResult(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ResultBuilder<T>
        {
            _isSuccess = result.IsSuccess,
            _value = result.IsSuccess ? result.Value : default,
            _error = result.Error
        };
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance based on a condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The value to use if the condition is true.</param>
    /// <returns>A ResultBuilder&lt;T&gt; configured for success with the value if condition is true, otherwise for failure.</returns>
    public static ResultBuilder<T> Create(bool condition, T value)
    {
        return condition ? Success(value) : Failure();
    }

    /// <summary>
    ///     Creates a new ResultBuilder&lt;T&gt; instance based on a condition with a specific error for failure.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="value">The value to use if the condition is true.</param>
    /// <param name="error">The error to use if the condition is false.</param>
    /// <returns>A ResultBuilder&lt;T&gt; configured for success with the value if condition is true, otherwise for failure with the specified error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public static ResultBuilder<T> Create(bool condition, T value, Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return condition ? Success(value) : Failure(error);
    }

    /// <summary>
    ///     Sets the value for the result being built.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    public ResultBuilder<T> WithValue(T value)
    {
        _value = value;
        _isSuccess = true;
        _error = null;
        return this;
    }

    /// <summary>
    ///     Sets the error for the result being built.
    /// </summary>
    /// <param name="error">The error to set.</param>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the error is null.</exception>
    public ResultBuilder<T> WithError(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        _error = error;
        _isSuccess = false;
        _value = default;
        return this;
    }

    /// <summary>
    ///     Sets the error for the result being built using code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the code or message is null or whitespace.</exception>
    public ResultBuilder<T> WithError(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Error message cannot be null or whitespace.", nameof(message));

        _error = new Error(code, message);
        _isSuccess = false;
        _value = default;
        return this;
    }

    /// <summary>
    ///     Sets the error type to Validation for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsValidation()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsValidation().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to NotFound for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsNotFound()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsNotFound().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Conflict for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsConflict()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsConflict().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Unauthorized for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsUnauthorized()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsUnauthorized().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Forbidden for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsForbidden()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsForbidden().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Internal for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsInternal()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsInternal().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to External for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsExternal()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsExternal().Build();
        return this;
    }

    /// <summary>
    ///     Sets the error type to Custom for the current error.
    /// </summary>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    public ResultBuilder<T> AsCustom()
    {
        EnsureErrorExists();
        _error = ErrorBuilder.FromError(_error!).AsCustom().Build();
        return this;
    }

    /// <summary>
    ///     Adds metadata to the current error.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public ResultBuilder<T> WithErrorMetadata(string key, object value)
    {
        EnsureErrorExists();

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or whitespace.", nameof(key));

        ArgumentNullException.ThrowIfNull(value);

        _error = ErrorBuilder.FromError(_error!).WithMetadata(key, value).Build();
        return this;
    }

    /// <summary>
    ///     Adds multiple metadata entries to the current error.
    /// </summary>
    /// <param name="metadata">A dictionary containing the metadata to add.</param>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the metadata dictionary is null.</exception>
    public ResultBuilder<T> WithErrorMetadata(IDictionary<string, object> metadata)
    {
        EnsureErrorExists();
        ArgumentNullException.ThrowIfNull(metadata);

        _error = ErrorBuilder.FromError(_error!).WithMetadata(metadata).Build();
        return this;
    }

    /// <summary>
    ///     Conditionally executes an action on the builder if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="configure">The action to execute on the builder if the condition is true.</param>
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the configure action is null.</exception>
    public ResultBuilder<T> If(bool condition, Action<ResultBuilder<T>> configure)
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
    /// <returns>The current ResultBuilder&lt;T&gt; instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the conditionFactory or configure action is null.</exception>
    public ResultBuilder<T> If(Func<bool> conditionFactory, Action<ResultBuilder<T>> configure)
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
    ///     Gets a value indicating whether the builder has a valid configuration to build a Result&lt;T&gt;.
    /// </summary>
    /// <returns>True if the builder can create a valid Result&lt;T&gt;; otherwise, false.</returns>
    public bool CanBuild()
    {
        return _isSuccess || (!_isSuccess && _error != null);
    }

    /// <summary>
    ///     Validates the current state of the builder and throws an exception if invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public ResultBuilder<T> Validate()
    {
        if (_isSuccess && _value == null && default(T) != null)
            throw new InvalidOperationException("Successful result must have a value specified for non-nullable types.");

        if (!_isSuccess && _error == null)
            throw new InvalidOperationException("Failed result must have an error specified.");

        return this;
    }

    /// <summary>
    ///     Builds and returns the configured Result&lt;T&gt; instance.
    /// </summary>
    /// <returns>A new Result&lt;T&gt; instance with the configured properties.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public Result<T> Build()
    {
        Validate();

        return _isSuccess ? Result<T>.Success(_value!) : Result<T>.Failure(_error!);
    }

    /// <summary>
    ///     Implicitly converts the ResultBuilder&lt;T&gt; to a Result&lt;T&gt; by calling Build().
    /// </summary>
    /// <param name="builder">The ResultBuilder&lt;T&gt; to convert.</param>
    /// <returns>The built Result&lt;T&gt; instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the builder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the builder is in an invalid state.</exception>
    public static implicit operator Result<T>(ResultBuilder<T> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Build();
    }

    /// <summary>
    ///     Returns a string representation of the ResultBuilder&lt;T&gt;'s current state.
    /// </summary>
    /// <returns>A string describing the current configuration of the builder.</returns>
    public override string ToString()
    {
        var status = _isSuccess ? "Success" : "Failure";
        var valueInfo = _isSuccess && _value != null ? $" (Value: {_value})" : "";
        var errorInfo = _error != null ? $" (Error: {_error.Code})" : "";
        return $"ResultBuilder<{typeof(T).Name}> [{status}{valueInfo}{errorInfo}]";
    }

    /// <summary>
    ///     Ensures that an error has been set for operations that require it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no error has been set.</exception>
    private void EnsureErrorExists()
    {
        if (_error == null)
            throw new InvalidOperationException("Error must be set before modifying error properties.");
    }
}