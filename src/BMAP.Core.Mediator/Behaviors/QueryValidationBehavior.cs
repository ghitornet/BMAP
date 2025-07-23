using BMAP.Core.Mediator.Behaviors;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Pipeline behavior that provides validation specifically for queries in the CQRS pattern.
///     This behavior validates query parameters to ensure data integrity in read operations.
/// </summary>
/// <typeparam name="TQuery">The type of query being validated.</typeparam>
/// <typeparam name="TResponse">The type of response from the query.</typeparam>
/// <remarks>
///     Initializes a new instance of the QueryValidationBehavior class.
/// </remarks>
/// <param name="validators">The collection of validators for the query.</param>
/// <param name="logger">The logger instance for logging validation details.</param>
public class QueryValidationBehavior<TQuery, TResponse>(
    IEnumerable<IValidator<TQuery>> validators,
    ILogger<QueryValidationBehavior<TQuery, TResponse>> logger) 
    : IPipelineBehavior<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    private readonly IValidator<TQuery>[] _validators = validators.ToArray();
    private readonly ILogger<QueryValidationBehavior<TQuery, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(TQuery request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var queryType = typeof(TQuery);
        
        if (_validators.Length == 0)
        {
            _logger.LogDebug("No validators registered for query {QueryType}", queryType.Name);
            return await next().ConfigureAwait(false);
        }

        _logger.LogDebug("Validating query {QueryType} with {ValidatorCount} validators", 
            queryType.Name, _validators.Length);

        var validationErrors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            try
            {
                _logger.LogTrace("Running validator {ValidatorType} for query {QueryType}", 
                    validator.GetType().Name, queryType.Name);
                    
                var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
                
                if (!validationResult.IsValid)
                {
                    validationErrors.AddRange(validationResult.Errors);
                    _logger.LogDebug("Validator {ValidatorType} found {ErrorCount} validation errors for query {QueryType}", 
                        validator.GetType().Name, validationResult.Errors.Count(), queryType.Name);
                }
                else
                {
                    _logger.LogTrace("Validator {ValidatorType} passed for query {QueryType}", 
                        validator.GetType().Name, queryType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running validator {ValidatorType} for query {QueryType}", 
                    validator.GetType().Name, queryType.Name);
                throw new ValidationException($"Error occurred during validation of query '{queryType.Name}'.", [new ValidationError(ex.Message)]);
            }
        }

        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Query {QueryType} validation failed with {ErrorCount} errors", 
                queryType.Name, validationErrors.Count);
                
            foreach (var error in validationErrors)
            {
                _logger.LogDebug("Validation error for query {QueryType}: {ErrorMessage} (Property: {PropertyName})", 
                    queryType.Name, error.Message, error.PropertyName ?? "N/A");
            }
            
            // For queries, validation errors should not prevent execution but should be logged
            // Consider throwing only for critical validation failures
            throw new ValidationException($"Validation failed for query '{queryType.Name}'.", validationErrors);
        }

        _logger.LogDebug("Query {QueryType} validation passed successfully", queryType.Name);
        return await next().ConfigureAwait(false);
    }
}