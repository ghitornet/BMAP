using BMAP.Core.Mediator.Behaviors;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Behaviors;

/// <summary>
///     Pipeline behavior that provides validation specifically for commands in the CQRS pattern.
///     This behavior validates commands before they modify system state.
/// </summary>
/// <typeparam name="TCommand">The type of command being validated.</typeparam>
/// <remarks>
///     Initializes a new instance of the CommandValidationBehavior class.
/// </remarks>
/// <param name="validators">The collection of validators for the command.</param>
/// <param name="logger">The logger instance for logging validation details.</param>
public class CommandValidationBehavior<TCommand>(
    IEnumerable<IValidator<TCommand>> validators,
    ILogger<CommandValidationBehavior<TCommand>> logger) 
    : IPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    private readonly IValidator<TCommand>[] _validators = validators.ToArray();
    private readonly ILogger<CommandValidationBehavior<TCommand>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task HandleAsync(TCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken = default)
    {
        var commandType = typeof(TCommand);
        
        if (_validators.Length == 0)
        {
            _logger.LogDebug("No validators registered for command {CommandType}", commandType.Name);
            await next().ConfigureAwait(false);
            return;
        }

        _logger.LogDebug("Validating command {CommandType} with {ValidatorCount} validators", 
            commandType.Name, _validators.Length);

        var validationErrors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            try
            {
                _logger.LogTrace("Running validator {ValidatorType} for command {CommandType}", 
                    validator.GetType().Name, commandType.Name);
                    
                var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
                
                if (!validationResult.IsValid)
                {
                    validationErrors.AddRange(validationResult.Errors);
                    _logger.LogDebug("Validator {ValidatorType} found {ErrorCount} validation errors for command {CommandType}", 
                        validator.GetType().Name, validationResult.Errors.Count(), commandType.Name);
                }
                else
                {
                    _logger.LogTrace("Validator {ValidatorType} passed for command {CommandType}", 
                        validator.GetType().Name, commandType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running validator {ValidatorType} for command {CommandType}", 
                    validator.GetType().Name, commandType.Name);
                throw new ValidationException($"Error occurred during validation of command '{commandType.Name}'.", [new ValidationError(ex.Message)]);
            }
        }

        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Command {CommandType} validation failed with {ErrorCount} errors", 
                commandType.Name, validationErrors.Count);
                
            foreach (var error in validationErrors)
            {
                _logger.LogDebug("Validation error for command {CommandType}: {ErrorMessage} (Property: {PropertyName})", 
                    commandType.Name, error.Message, error.PropertyName ?? "N/A");
            }
            
            throw new ValidationException($"Validation failed for command '{commandType.Name}'.", validationErrors);
        }

        _logger.LogDebug("Command {CommandType} validation passed successfully", commandType.Name);
        await next().ConfigureAwait(false);
    }
}

/// <summary>
///     Pipeline behavior that provides validation specifically for commands with response in the CQRS pattern.
///     This behavior validates commands before they modify system state.
/// </summary>
/// <typeparam name="TCommand">The type of command being validated.</typeparam>
/// <typeparam name="TResponse">The type of response from the command.</typeparam>
/// <remarks>
///     Initializes a new instance of the CommandValidationBehavior class.
/// </remarks>
/// <param name="validators">The collection of validators for the command.</param>
/// <param name="logger">The logger instance for logging validation details.</param>
public class CommandValidationBehavior<TCommand, TResponse>(
    IEnumerable<IValidator<TCommand>> validators,
    ILogger<CommandValidationBehavior<TCommand, TResponse>> logger) 
    : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    private readonly IValidator<TCommand>[] _validators = validators.ToArray();
    private readonly ILogger<CommandValidationBehavior<TCommand, TResponse>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(TCommand request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var commandType = typeof(TCommand);
        
        if (_validators.Length == 0)
        {
            _logger.LogDebug("No validators registered for command {CommandType}", commandType.Name);
            return await next().ConfigureAwait(false);
        }

        _logger.LogDebug("Validating command {CommandType} with {ValidatorCount} validators", 
            commandType.Name, _validators.Length);

        var validationErrors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            try
            {
                _logger.LogTrace("Running validator {ValidatorType} for command {CommandType}", 
                    validator.GetType().Name, commandType.Name);
                    
                var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
                
                if (!validationResult.IsValid)
                {
                    validationErrors.AddRange(validationResult.Errors);
                    _logger.LogDebug("Validator {ValidatorType} found {ErrorCount} validation errors for command {CommandType}", 
                        validator.GetType().Name, validationResult.Errors.Count(), commandType.Name);
                }
                else
                {
                    _logger.LogTrace("Validator {ValidatorType} passed for command {CommandType}", 
                        validator.GetType().Name, commandType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running validator {ValidatorType} for command {CommandType}", 
                    validator.GetType().Name, commandType.Name);
                throw new ValidationException($"Error occurred during validation of command '{commandType.Name}'.", [new ValidationError(ex.Message)]);
            }
        }

        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Command {CommandType} validation failed with {ErrorCount} errors", 
                commandType.Name, validationErrors.Count);
                
            foreach (var error in validationErrors)
            {
                _logger.LogDebug("Validation error for command {CommandType}: {ErrorMessage} (Property: {PropertyName})", 
                    commandType.Name, error.Message, error.PropertyName ?? "N/A");
            }
            
            throw new ValidationException($"Validation failed for command '{commandType.Name}'.", validationErrors);
        }

        _logger.LogDebug("Command {CommandType} validation passed successfully", commandType.Name);
        return await next().ConfigureAwait(false);
    }
}