namespace BMAP.Core.Mediator;

/// <summary>
///     Defines a contract for resolving service dependencies.
///     This abstraction allows the mediator to work with different DI containers.
/// </summary>
public interface IServiceLocator
{
    /// <summary>
    ///     Gets a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>An instance of the requested service type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved.</exception>
    T GetService<T>() where T : class;

    /// <summary>
    ///     Gets a service of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns>An instance of the requested service type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service cannot be resolved.</exception>
    object GetService(Type serviceType);

    /// <summary>
    ///     Gets all services of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of services to retrieve.</typeparam>
    /// <returns>An enumerable of all instances of the requested service type.</returns>
    IEnumerable<T> GetServices<T>() where T : class;

    /// <summary>
    ///     Gets all services of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of services to retrieve.</param>
    /// <returns>An enumerable of all instances of the requested service type.</returns>
    IEnumerable<object?> GetServices(Type serviceType);

    /// <summary>
    ///     Tries to get a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>An instance of the requested service type, or null if not found.</returns>
    T? GetServiceOrDefault<T>() where T : class;

    /// <summary>
    ///     Tries to get a service of the specified type.
    /// </summary>
    /// <param name="serviceType">The type of service to retrieve.</param>
    /// <returns>An instance of the requested service type, or null if not found.</returns>
    object? GetServiceOrDefault(Type serviceType);
}