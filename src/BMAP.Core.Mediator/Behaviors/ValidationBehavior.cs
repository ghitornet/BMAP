namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Defines a validator for requests.
///     Validators are used to validate requests before they are processed.
/// </summary>
/// <typeparam name="TRequest">The type of request being validated.</typeparam>
public interface IValidator<in TRequest> where TRequest : IRequest
{
    /// <summary>
    ///     Validates the specified request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous validation operation and contains the validation result.</returns>
    Task<ValidationResult> ValidateAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
///     Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    ///     Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; private init; }

    /// <summary>
    ///     Gets the collection of validation errors.
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; private init; } = [];

    /// <summary>
    ///     Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    ///     Creates a failed validation result with the specified errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(params ValidationError[] errors)
    {
        return new ValidationResult { IsValid = false, Errors = errors };
    }

    /// <summary>
    ///     Creates a failed validation result with the specified error messages.
    /// </summary>
    /// <param name="errorMessages">The validation error messages.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(params string[] errorMessages)
    {
        return new ValidationResult { IsValid = false, Errors = errorMessages.Select(msg => new ValidationError(msg)) };
    }
}

/// <summary>
///     Represents a validation error.
/// </summary>
/// <remarks>
///     Initializes a new instance of the ValidationError class.
/// </remarks>
/// <param name="message">The error message.</param>
/// <param name="propertyName">The property name that caused the error.</param>
public class ValidationError(string message, string? propertyName = null)
{

    /// <summary>
    ///     Gets the error message.
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    ///     Gets the property name that caused the error.
    /// </summary>
    public string? PropertyName { get; } = propertyName;
}

/// <summary>
///     Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    ///     Initializes a new instance of the ValidationException class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="errors">The validation errors.</param>
    public ValidationException(string message, IEnumerable<ValidationError> errors)
        : base(message)
    {
        Errors = errors;
    }

    /// <summary>
    ///     Gets the validation errors.
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; }
}

/// <summary>
///     Pipeline behavior that validates requests before they are processed.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <remarks>
///     Initializes a new instance of the ValidationBehavior class.
/// </remarks>
/// <param name="validators">The validators for the request type.</param>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     Handles the request and validates it before processing.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation and contains the response.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any()) return await next();

        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken));
        var validationResults = await Task.WhenAll(validationTasks);

        var errors = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Count != 0) throw new ValidationException(errors);

        return await next();
    }
}

/// <summary>
///     Pipeline behavior that validates requests without response before they are processed.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <remarks>
///     Initializes a new instance of the ValidationBehavior class.
/// </remarks>
/// <param name="validators">The validators for the request type.</param>
public class ValidationBehavior<TRequest>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    ///     Handles the request and validates it before processing.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ValidationException">Thrown when validation fails.</exception>
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        if (!validators.Any())
        {
            await next();
            return;
        }

        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken));
        var validationResults = await Task.WhenAll(validationTasks);

        var errors = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (errors.Count != 0) throw new ValidationException(errors);

        await next();
    }
}