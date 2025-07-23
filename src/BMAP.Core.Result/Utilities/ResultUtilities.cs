namespace BMAP.Core.Result.Utilities;

/// <summary>
///     Provides utility methods for working with Result types.
///     These utilities help with common scenarios like exception handling, validation, and result creation.
/// </summary>
public static class ResultUtilities
{
    /// <summary>
    ///     Executes a function and wraps any exceptions in a Result.
    /// </summary>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorMapper">Optional function to map exceptions to custom errors.</param>
    /// <returns>A successful Result if no exception occurs, otherwise a failure Result with the exception details.</returns>
    public static Result Try(Action func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            func();
            return Result.Success();
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? 
                       Error.Internal("Exception.Caught", ex.Message)
                            .WithMetadata("ExceptionType", ex.GetType().Name)
                            .WithMetadata("StackTrace", ex.StackTrace ?? string.Empty);
            return Result.Failure(error);
        }
    }

    /// <summary>
    ///     Executes a function and wraps any exceptions in a Result.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="errorMapper">Optional function to map exceptions to custom errors.</param>
    /// <returns>A successful Result with the function result if no exception occurs, otherwise a failure Result.</returns>
    public static Result<TValue> Try<TValue>(Func<TValue> func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            var value = func();
            return Result<TValue>.Success(value);
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? 
                       Error.Internal("Exception.Caught", ex.Message)
                            .WithMetadata("ExceptionType", ex.GetType().Name)
                            .WithMetadata("StackTrace", ex.StackTrace ?? string.Empty);
            return Result<TValue>.Failure(error);
        }
    }

    /// <summary>
    ///     Executes an async function and wraps any exceptions in a Result.
    /// </summary>
    /// <param name="func">The async function to execute.</param>
    /// <param name="errorMapper">Optional function to map exceptions to custom errors.</param>
    /// <returns>A successful Result if no exception occurs, otherwise a failure Result with the exception details.</returns>
    public static async Task<Result> TryAsync(Func<Task> func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            await func().ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? 
                       Error.Internal("Exception.Caught", ex.Message)
                            .WithMetadata("ExceptionType", ex.GetType().Name)
                            .WithMetadata("StackTrace", ex.StackTrace ?? string.Empty);
            return Result.Failure(error);
        }
    }

    /// <summary>
    ///     Executes an async function and wraps any exceptions in a Result.
    /// </summary>
    /// <typeparam name="TValue">The type of the value returned by the function.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <param name="errorMapper">Optional function to map exceptions to custom errors.</param>
    /// <returns>A successful Result with the function result if no exception occurs, otherwise a failure Result.</returns>
    public static async Task<Result<TValue>> TryAsync<TValue>(Func<Task<TValue>> func, Func<Exception, Error>? errorMapper = null)
    {
        try
        {
            var value = await func().ConfigureAwait(false);
            return Result<TValue>.Success(value);
        }
        catch (Exception ex)
        {
            var error = errorMapper?.Invoke(ex) ?? 
                       Error.Internal("Exception.Caught", ex.Message)
                            .WithMetadata("ExceptionType", ex.GetType().Name)
                            .WithMetadata("StackTrace", ex.StackTrace ?? string.Empty);
            return Result<TValue>.Failure(error);
        }
    }

    /// <summary>
    ///     Creates a Result based on a boolean condition with detailed validation information.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="actualValue">The actual value that was validated.</param>
    /// <param name="expectedCriteria">Description of what was expected.</param>
    /// <returns>A successful Result if the condition is true, otherwise a validation failure Result.</returns>
    public static Result Validate(bool condition, string fieldName, object? actualValue, string expectedCriteria)
    {
        if (condition)
            return Result.Success();

        var error = Error.Validation(
            $"Validation.{fieldName}",
            $"Validation failed for field '{fieldName}': {expectedCriteria}")
            .WithMetadata("FieldName", fieldName)
            .WithMetadata("ActualValue", actualValue ?? "null")
            .WithMetadata("ExpectedCriteria", expectedCriteria);

        return Result.Failure(error);
    }

    /// <summary>
    ///     Creates a Result based on a value validation with detailed information.
    /// </summary>
    /// <typeparam name="TValue">The type of the value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="predicate">The validation predicate.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="expectedCriteria">Description of what was expected.</param>
    /// <returns>A successful Result with the value if validation passes, otherwise a validation failure Result.</returns>
    public static Result<TValue> Validate<TValue>(TValue value, Func<TValue, bool> predicate, string fieldName, string expectedCriteria)
    {
        if (predicate(value))
            return Result<TValue>.Success(value);

        var error = Error.Validation(
            $"Validation.{fieldName}",
            $"Validation failed for field '{fieldName}': {expectedCriteria}")
            .WithMetadata("FieldName", fieldName)
            .WithMetadata("ActualValue", value?.ToString() ?? "null")
            .WithMetadata("ExpectedCriteria", expectedCriteria);

        return Result<TValue>.Failure(error);
    }

    /// <summary>
    ///     Creates a Result from a nullable value with detailed not found information.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="resourceName">The name of the resource that was not found.</param>
    /// <param name="identifier">The identifier used to search for the resource.</param>
    /// <returns>A successful Result with the value if not null, otherwise a not found failure Result.</returns>
    public static Result<TValue> NotNull<TValue>(TValue? value, string resourceName, object identifier)
        where TValue : class
    {
        if (value != null)
            return Result<TValue>.Success(value);

        var error = Error.NotFound(
            $"NotFound.{resourceName}",
            $"{resourceName} with identifier '{identifier}' was not found")
            .WithMetadata("ResourceName", resourceName)
            .WithMetadata("Identifier", identifier.ToString() ?? "null");

        return Result<TValue>.Failure(error);
    }

    /// <summary>
    ///     Creates a Result from a nullable value with detailed not found information.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="resourceName">The name of the resource that was not found.</param>
    /// <param name="identifier">The identifier used to search for the resource.</param>
    /// <returns>A successful Result with the value if not null, otherwise a not found failure Result.</returns>
    public static Result<TValue> NotNull<TValue>(TValue? value, string resourceName, object identifier)
        where TValue : struct
    {
        if (value.HasValue)
            return Result<TValue>.Success(value.Value);

        var error = Error.NotFound(
            $"NotFound.{resourceName}",
            $"{resourceName} with identifier '{identifier}' was not found")
            .WithMetadata("ResourceName", resourceName)
            .WithMetadata("Identifier", identifier.ToString() ?? "null");

        return Result<TValue>.Failure(error);
    }

    /// <summary>
    ///     Creates a Result that succeeds only if a collection is not empty.
    /// </summary>
    /// <typeparam name="TValue">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="resourceName">The name of the resource collection.</param>
    /// <returns>A successful Result with the collection if not empty, otherwise a not found failure Result.</returns>
    public static Result<IEnumerable<TValue>> NotEmpty<TValue>(IEnumerable<TValue> collection, string resourceName)
    {
        var enumerable = collection.ToList();
        if (enumerable.Any())
            return Result<IEnumerable<TValue>>.Success(enumerable);

        var error = Error.NotFound(
            $"NotFound.{resourceName}",
            $"No {resourceName} were found")
            .WithMetadata("ResourceName", resourceName)
            .WithMetadata("CollectionType", typeof(TValue).Name);

        return Result<IEnumerable<TValue>>.Failure(error);
    }

    /// <summary>
    ///     Aggregates multiple validation results into a single result.
    /// </summary>
    /// <param name="validations">The validation results to aggregate.</param>
    /// <returns>A successful Result if all validations pass, otherwise a failure Result with aggregated errors.</returns>
    public static Result AggregateValidations(params Result[] validations)
    {
        var failures = validations.Where(v => v.IsFailure).ToList();
        
        if (!failures.Any())
            return Result.Success();

        if (failures.Count == 1)
            return failures.First();

        // Aggregate multiple validation errors
        var errorMessages = failures.Select(f => f.Error!.Message);
        var aggregatedMessage = string.Join("; ", errorMessages);
        
        var error = Error.Validation(
            "Validation.Multiple",
            $"Multiple validation errors occurred: {aggregatedMessage}")
            .WithMetadata("ErrorCount", failures.Count)
            .WithMetadata("Errors", failures.Select(f => f.Error!).ToList());

        return Result.Failure(error);
    }

    /// <summary>
    ///     Creates a Result that ensures a value meets multiple criteria.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validations">The validation functions with their descriptions.</param>
    /// <returns>A successful Result with the value if all validations pass, otherwise a failure Result.</returns>
    public static Result<TValue> EnsureAll<TValue>(TValue value, params (Func<TValue, bool> Predicate, string Description)[] validations)
    {
        var failures = new List<string>();

        foreach (var (predicate, description) in validations)
        {
            if (!predicate(value))
                failures.Add(description);
        }

        if (!failures.Any())
            return Result<TValue>.Success(value);

        var aggregatedMessage = string.Join("; ", failures);
        var error = Error.Validation(
            "Validation.Multiple",
            $"Value validation failed: {aggregatedMessage}")
            .WithMetadata("Value", value?.ToString() ?? "null")
            .WithMetadata("FailedValidations", failures);

        return Result<TValue>.Failure(error);
    }
}