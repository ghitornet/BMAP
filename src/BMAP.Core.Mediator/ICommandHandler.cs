namespace BMAP.Core.Mediator;

/// <summary>
///     Defines a handler for a command that doesn't return a response.
///     Command handlers encapsulate the business logic for processing commands in the CQRS pattern.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
/// <remarks>
///     Command handlers should:
///     - Validate the command (or delegate to validation behaviors)
///     - Execute the business logic
///     - Persist changes to the write store
///     - Publish domain events/notifications if needed
///     - Be focused on a single command type for better separation of concerns
/// </remarks>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}

/// <summary>
///     Defines a handler for a command that returns a response.
///     Command handlers encapsulate the business logic for processing commands in the CQRS pattern.
/// </summary>
/// <typeparam name="TCommand">The type of command being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <remarks>
///     Command handlers should:
///     - Validate the command (or delegate to validation behaviors)
///     - Execute the business logic
///     - Persist changes to the write store
///     - Return only essential data (typically identifiers or confirmation data)
///     - Publish domain events/notifications if needed
///     - Be focused on a single command type for better separation of concerns
/// </remarks>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}