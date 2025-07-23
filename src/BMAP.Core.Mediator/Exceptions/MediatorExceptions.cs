namespace BMAP.Core.Mediator.Exceptions;

/// <summary>
///     Exception thrown when an error occurs during mediator processing.
/// </summary>
public class MediatorException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the MediatorException class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MediatorException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the MediatorException class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MediatorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Exception thrown when a request handler is not found for a specific request type.
/// </summary>
public class HandlerNotFoundException : MediatorException
{
    /// <summary>
    ///     Initializes a new instance of the HandlerNotFoundException class.
    /// </summary>
    /// <param name="requestType">The type of request that could not find a handler.</param>
    public HandlerNotFoundException(Type requestType)
        : base($"No handler found for request type '{requestType.Name}'.")
    {
        RequestType = requestType;
    }

    /// <summary>
    ///     Initializes a new instance of the HandlerNotFoundException class with a custom message.
    /// </summary>
    /// <param name="requestType">The type of request that could not find a handler.</param>
    /// <param name="message">The custom error message.</param>
    public HandlerNotFoundException(Type requestType, string message)
        : base(message)
    {
        RequestType = requestType;
    }

    /// <summary>
    ///     Initializes a new instance of the HandlerNotFoundException class with a custom message and inner exception.
    /// </summary>
    /// <param name="requestType">The type of request that could not find a handler.</param>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public HandlerNotFoundException(Type requestType, string message, Exception innerException)
        : base(message, innerException)
    {
        RequestType = requestType;
    }

    /// <summary>
    ///     Gets the type of request that could not find a handler.
    /// </summary>
    public Type RequestType { get; }
}

/// <summary>
///     Exception thrown when multiple handlers are found for a request that should have only one handler.
/// </summary>
public class MultipleHandlersFoundException : MediatorException
{
    /// <summary>
    ///     Initializes a new instance of the MultipleHandlersFoundException class.
    /// </summary>
    /// <param name="requestType">The type of request that has multiple handlers.</param>
    /// <param name="handlerCount">The number of handlers found.</param>
    public MultipleHandlersFoundException(Type requestType, int handlerCount)
        : base(
            $"Multiple handlers ({handlerCount}) found for request type '{requestType.Name}'. Expected exactly one handler.")
    {
        RequestType = requestType;
        HandlerCount = handlerCount;
    }

    /// <summary>
    ///     Initializes a new instance of the MultipleHandlersFoundException class with a custom message.
    /// </summary>
    /// <param name="requestType">The type of request that has multiple handlers.</param>
    /// <param name="handlerCount">The number of handlers found.</param>
    /// <param name="message">The custom error message.</param>
    public MultipleHandlersFoundException(Type requestType, int handlerCount, string message)
        : base(message)
    {
        RequestType = requestType;
        HandlerCount = handlerCount;
    }

    /// <summary>
    ///     Initializes a new instance of the MultipleHandlersFoundException class with a custom message and inner exception.
    /// </summary>
    /// <param name="requestType">The type of request that has multiple handlers.</param>
    /// <param name="handlerCount">The number of handlers found.</param>
    /// <param name="message">The custom error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MultipleHandlersFoundException(Type requestType, int handlerCount, string message, Exception innerException)
        : base(message, innerException)
    {
        RequestType = requestType;
        HandlerCount = handlerCount;
    }

    /// <summary>
    ///     Gets the type of request that has multiple handlers.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    ///     Gets the number of handlers found.
    /// </summary>
    public int HandlerCount { get; }
}