namespace BMAP.Core.Result.Extensions;

/// <summary>
///     Provides extension methods for Result and Result&lt;T&gt; types to enable functional programming patterns.
///     These extensions support method chaining, transformations, and functional composition.
/// </summary>
public static class ResultExtensions
{
    #region Bind Extensions

    /// <summary>
    ///     Binds the result to another operation if the current result is successful.
    ///     This enables chaining of operations that return Results.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="func">The function to execute if the current result is successful.</param>
    /// <returns>The result of the function if successful, otherwise the current error.</returns>
    public static Result Bind(this Result result, Func<Result> func)
    {
        return result.IsSuccess ? func() : result.Error!;
    }

    /// <summary>
    ///     Binds the result to another operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the new result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The function to execute if the current result is successful.</param>
    /// <returns>The result of the function if successful, otherwise the current error.</returns>
    public static Result<TValue> Bind<TValue>(this Result result, Func<Result<TValue>> func)
    {
        return result.IsSuccess ? func() : result.Error!;
    }

    /// <summary>
    ///     Binds the result to another operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the current result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The function to execute if the current result is successful.</param>
    /// <returns>The result of the function if successful, otherwise the current error.</returns>
    public static Result Bind<TValue>(this Result<TValue> result, Func<TValue, Result> func)
    {
        return result.IsSuccess ? func(result.Value) : result.Error!;
    }

    /// <summary>
    ///     Binds the result to another operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the current result.</typeparam>
    /// <typeparam name="TNewValue">The type of the value in the new result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The function to execute if the current result is successful.</param>
    /// <returns>The result of the function if successful, otherwise the current error.</returns>
    public static Result<TNewValue> Bind<TValue, TNewValue>(this Result<TValue> result, Func<TValue, Result<TNewValue>> func)
    {
        return result.IsSuccess ? func(result.Value) : result.Error!;
    }

    #endregion

    #region Map Extensions

    /// <summary>
    ///     Maps the value of a successful result to a new value using the provided function.
    /// </summary>
    /// <typeparam name="TValue">The type of the current value.</typeparam>
    /// <typeparam name="TNewValue">The type of the new value.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The function to transform the value.</param>
    /// <returns>A new result with the transformed value if successful, otherwise the current error.</returns>
    public static Result<TNewValue> Map<TValue, TNewValue>(this Result<TValue> result, Func<TValue, TNewValue> func)
    {
        return result.IsSuccess ? Result<TNewValue>.Success(func(result.Value)) : result.Error!;
    }

    /// <summary>
    ///     Maps a successful result to a result with a value using the provided function.
    /// </summary>
    /// <typeparam name="TValue">The type of the new value.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The function to provide the value.</param>
    /// <returns>A new result with the provided value if successful, otherwise the current error.</returns>
    public static Result<TValue> Map<TValue>(this Result result, Func<TValue> func)
    {
        return result.IsSuccess ? Result<TValue>.Success(func()) : result.Error!;
    }

    #endregion

    #region Match Extensions

    /// <summary>
    ///     Executes one of two functions depending on whether the result is successful or not.
    /// </summary>
    /// <typeparam name="TReturn">The type of the return value.</typeparam>
    /// <param name="result">The result to match against.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TReturn Match<TReturn>(this Result result, Func<TReturn> onSuccess, Func<Error, TReturn> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Error!);
    }

    /// <summary>
    ///     Executes one of two functions depending on whether the result is successful or not.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <typeparam name="TReturn">The type of the return value.</typeparam>
    /// <param name="result">The result to match against.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TReturn Match<TValue, TReturn>(this Result<TValue> result, Func<TValue, TReturn> onSuccess, Func<Error, TReturn> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error!);
    }

    /// <summary>
    ///     Executes one of two actions depending on whether the result is successful or not.
    /// </summary>
    /// <param name="result">The result to match against.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    public static void Match(this Result result, Action onSuccess, Action<Error> onFailure)
    {
        if (result.IsSuccess)
            onSuccess();
        else
            onFailure(result.Error!);
    }

    /// <summary>
    ///     Executes one of two actions depending on whether the result is successful or not.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <param name="result">The result to match against.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    public static void Match<TValue>(this Result<TValue> result, Action<TValue> onSuccess, Action<Error> onFailure)
    {
        if (result.IsSuccess)
            onSuccess(result.Value);
        else
            onFailure(result.Error!);
    }

    #endregion

    #region Tap Extensions

    /// <summary>
    ///     Executes an action if the result is successful, without changing the result.
    ///     This is useful for side effects like logging.
    /// </summary>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute if successful.</param>
    /// <returns>The original result.</returns>
    public static Result Tap(this Result result, Action action)
    {
        if (result.IsSuccess)
            action();
        
        return result;
    }

    /// <summary>
    ///     Executes an action if the result is successful, without changing the result.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute if successful.</param>
    /// <returns>The original result.</returns>
    public static Result<TValue> Tap<TValue>(this Result<TValue> result, Action<TValue> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        
        return result;
    }

    /// <summary>
    ///     Executes an action if the result is a failure, without changing the result.
    /// </summary>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute if failed.</param>
    /// <returns>The original result.</returns>
    public static Result TapError(this Result result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error!);
        
        return result;
    }

    /// <summary>
    ///     Executes an action if the result is a failure, without changing the result.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The action to execute if failed.</param>
    /// <returns>The original result.</returns>
    public static Result<TValue> TapError<TValue>(this Result<TValue> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error!);
        
        return result;
    }

    #endregion

    #region Ensure Extensions

    /// <summary>
    ///     Ensures a condition is true, otherwise returns a failure result.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="predicate">The condition to check.</param>
    /// <param name="error">The error to return if the condition is false.</param>
    /// <returns>The original result if successful and condition is true, otherwise a failure result.</returns>
    public static Result Ensure(this Result result, Func<bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate() ? result : Result.Failure(error);
    }

    /// <summary>
    ///     Ensures a condition is true for the value, otherwise returns a failure result.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="predicate">The condition to check against the value.</param>
    /// <param name="error">The error to return if the condition is false.</param>
    /// <returns>The original result if successful and condition is true, otherwise a failure result.</returns>
    public static Result<TValue> Ensure<TValue>(this Result<TValue> result, Func<TValue, bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value) ? result : Result<TValue>.Failure(error);
    }

    #endregion

    #region Async Extensions

    /// <summary>
    ///     Binds the result to an async operation if the current result is successful.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="func">The async function to execute if the current result is successful.</param>
    /// <returns>The result of the async function if successful, otherwise the current error.</returns>
    public static async Task<Result> BindAsync(this Result result, Func<Task<Result>> func)
    {
        return result.IsSuccess ? await func().ConfigureAwait(false) : result.Error!;
    }

    /// <summary>
    ///     Binds the result to an async operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the current result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The async function to execute if the current result is successful.</param>
    /// <returns>The result of the async function if successful, otherwise the current error.</returns>
    public static async Task<Result> BindAsync<TValue>(this Result<TValue> result, Func<TValue, Task<Result>> func)
    {
        return result.IsSuccess ? await func(result.Value).ConfigureAwait(false) : result.Error!;
    }

    /// <summary>
    ///     Binds the result to an async operation if the current result is successful.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the current result.</typeparam>
    /// <typeparam name="TNewValue">The type of the value in the new result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The async function to execute if the current result is successful.</param>
    /// <returns>The result of the async function if successful, otherwise the current error.</returns>
    public static async Task<Result<TNewValue>> BindAsync<TValue, TNewValue>(this Result<TValue> result, Func<TValue, Task<Result<TNewValue>>> func)
    {
        return result.IsSuccess ? await func(result.Value).ConfigureAwait(false) : result.Error!;
    }

    /// <summary>
    ///     Maps the value of a successful result to a new value using an async function.
    /// </summary>
    /// <typeparam name="TValue">The type of the current value.</typeparam>
    /// <typeparam name="TNewValue">The type of the new value.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="func">The async function to transform the value.</param>
    /// <returns>A new result with the transformed value if successful, otherwise the current error.</returns>
    public static async Task<Result<TNewValue>> MapAsync<TValue, TNewValue>(this Result<TValue> result, Func<TValue, Task<TNewValue>> func)
    {
        return result.IsSuccess 
            ? Result<TNewValue>.Success(await func(result.Value).ConfigureAwait(false)) 
            : result.Error!;
    }

    /// <summary>
    ///     Executes an async action if the result is successful, without changing the result.
    /// </summary>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The async action to execute if successful.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result> TapAsync(this Result result, Func<Task> action)
    {
        if (result.IsSuccess)
            await action().ConfigureAwait(false);
        
        return result;
    }

    /// <summary>
    ///     Executes an async action if the result is successful, without changing the result.
    /// </summary>
    /// <typeparam name="TValue">The type of the value in the result.</typeparam>
    /// <param name="result">The result to tap.</param>
    /// <param name="action">The async action to execute if successful.</param>
    /// <returns>The original result.</returns>
    public static async Task<Result<TValue>> TapAsync<TValue>(this Result<TValue> result, Func<TValue, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value).ConfigureAwait(false);
        
        return result;
    }

    #endregion

    #region Combine Extensions

    /// <summary>
    ///     Combines multiple results into a single result. If any result is a failure, returns the first failure.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs are successful, otherwise the first failure.</returns>
    public static Result Combine(params Result[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    /// <summary>
    ///     Combines multiple results into a single result. If any result is a failure, returns the first failure.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A successful result if all inputs are successful, otherwise the first failure.</returns>
    public static Result Combine(IEnumerable<Result> results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    #endregion
}