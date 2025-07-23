using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator;

/// <summary>
///     Default implementation of IServiceLocator that uses IServiceProvider.
///     This implementation integrates with Microsoft Dependency Injection.
/// </summary>
/// <remarks>
///     Initializes a new instance of the ServiceLocator class.
/// </remarks>
/// <param name="serviceProvider">The service provider to use for dependency resolution.</param>
/// <param name="logger">The logger instance.</param>
/// <exception cref="ArgumentNullException">Thrown when serviceProvider or logger is null.</exception>
public class ServiceLocator(IServiceProvider serviceProvider, ILogger<ServiceLocator> logger) : IServiceLocator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<ServiceLocator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public T GetService<T>() where T : class
    {
        _logger.LogDebug("Resolving service of type {ServiceType}", typeof(T).Name);
        
        var service = _serviceProvider.GetService<T>();
        if (service == null)
        {
            _logger.LogError("Service of type {ServiceType} is not registered", typeof(T).Name);
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered.");
        }
        
        _logger.LogDebug("Successfully resolved service of type {ServiceType}", typeof(T).Name);
        return service;
    }

    /// <inheritdoc />
    public object GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        
        _logger.LogDebug("Resolving service of type {ServiceType}", serviceType.Name);

        var service = _serviceProvider.GetService(serviceType);
        if (service == null)
        {
            _logger.LogError("Service of type {ServiceType} is not registered", serviceType.Name);
            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        }
        
        _logger.LogDebug("Successfully resolved service of type {ServiceType}", serviceType.Name);
        return service;
    }

    /// <inheritdoc />
    public IEnumerable<T> GetServices<T>() where T : class
    {
        _logger.LogDebug("Resolving all services of type {ServiceType}", typeof(T).Name);
        
        var services = _serviceProvider.GetServices<T>();
        var serviceList = services.ToList();
        
        _logger.LogDebug("Found {ServiceCount} services of type {ServiceType}", serviceList.Count, typeof(T).Name);
        return serviceList;
    }

    /// <inheritdoc />
    public IEnumerable<object?> GetServices(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        
        _logger.LogDebug("Resolving all services of type {ServiceType}", serviceType.Name);
        
        var services = _serviceProvider.GetServices(serviceType);
        var serviceList = services.ToList();
        
        _logger.LogDebug("Found {ServiceCount} services of type {ServiceType}", serviceList.Count, serviceType.Name);
        return serviceList;
    }

    /// <inheritdoc />
    public T? GetServiceOrDefault<T>() where T : class
    {
        _logger.LogDebug("Attempting to resolve service of type {ServiceType}", typeof(T).Name);
        
        var service = _serviceProvider.GetService<T>();
        if (service == null)
        {
            _logger.LogDebug("Service of type {ServiceType} is not registered, returning null", typeof(T).Name);
        }
        else
        {
            _logger.LogDebug("Successfully resolved service of type {ServiceType}", typeof(T).Name);
        }
        
        return service;
    }

    /// <inheritdoc />
    public object? GetServiceOrDefault(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        
        _logger.LogDebug("Attempting to resolve service of type {ServiceType}", serviceType.Name);
        
        var service = _serviceProvider.GetService(serviceType);
        if (service == null)
        {
            _logger.LogDebug("Service of type {ServiceType} is not registered, returning null", serviceType.Name);
        }
        else
        {
            _logger.LogDebug("Successfully resolved service of type {ServiceType}", serviceType.Name);
        }
        
        return service;
    }
}