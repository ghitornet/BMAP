using Microsoft.Extensions.DependencyInjection;

namespace BMAP.Core.Mediator;

/// <summary>
///     Default implementation of IServiceLocator that uses IServiceProvider.
///     This implementation integrates with Microsoft Dependency Injection.
/// </summary>
/// <remarks>
///     Initializes a new instance of the ServiceLocator class.
/// </remarks>
/// <param name="serviceProvider">The service provider to use for dependency resolution.</param>
/// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
public class ServiceLocator(IServiceProvider serviceProvider) : IServiceLocator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    /// <inheritdoc />
    public T GetService<T>() where T : class
    {
        var service = _serviceProvider.GetService<T>() ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        return service;
    }

    /// <inheritdoc />
    public object GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        var service = _serviceProvider.GetService(serviceType) ?? throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        return service;
    }

    /// <inheritdoc />
    public IEnumerable<T> GetServices<T>() where T : class
    {
        return _serviceProvider.GetServices<T>();
    }

    /// <inheritdoc />
    public IEnumerable<object?> GetServices(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return _serviceProvider.GetServices(serviceType);
    }

    /// <inheritdoc />
    public T? GetServiceOrDefault<T>() where T : class
    {
        return _serviceProvider.GetService<T>();
    }

    /// <inheritdoc />
    public object? GetServiceOrDefault(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        return _serviceProvider.GetService(serviceType);
    }
}