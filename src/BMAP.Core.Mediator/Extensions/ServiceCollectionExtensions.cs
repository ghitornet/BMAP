using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Extensions;

/// <summary>
///     Extension methods for registering mediator services with the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the mediator services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.TryAddTransient<IServiceLocator>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ServiceLocator>>();
            return new ServiceLocator(serviceProvider, logger);
        });
        
        services.TryAddTransient<IMediator>(serviceProvider =>
        {
            var serviceLocator = serviceProvider.GetRequiredService<IServiceLocator>();
            var logger = serviceProvider.GetRequiredService<ILogger<Mediator>>();
            return new Mediator(serviceLocator, logger);
        });

        return services;
    }

    /// <summary>
    ///     Adds the mediator services to the service collection and registers handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediator();
        services.RegisterHandlersFromAssemblies(assemblies);

        return services;
    }

    /// <summary>
    ///     Adds the mediator services to the service collection and registers handlers from the assemblies containing the
    ///     specified types.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorFromAssemblyContaining<T>(this IServiceCollection services)
    {
        return services.AddMediator(typeof(T).Assembly);
    }

    /// <summary>
    ///     Adds the mediator services to the service collection and registers handlers from the assemblies containing the
    ///     specified types.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="markerTypes">Types whose assemblies will be scanned for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorFromAssemblyContaining(this IServiceCollection services,
        params Type[] markerTypes)
    {
        var assemblies = markerTypes.Select(t => t.Assembly).Distinct().ToArray();
        return services.AddMediator(assemblies);
    }

    /// <summary>
    ///     Registers request and notification handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection RegisterHandlersFromAssemblies(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies) RegisterHandlersFromAssembly(services, assembly);

        return services;
    }

    /// <summary>
    ///     Registers request and notification handlers from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection RegisterHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        // Register request handlers without response
        RegisterRequestHandlers(services, types);

        // Register request handlers with response
        RegisterRequestHandlersWithResponse(services, types);

        // Register notification handlers
        RegisterNotificationHandlers(services, types);

        return services;
    }

    private static void RegisterRequestHandlers(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
                .Select(i => new { HandlerType = t, InterfaceType = i }))
            .ToList();

        foreach (var handler in handlerTypes) services.AddTransient(handler.InterfaceType, handler.HandlerType);
    }

    private static void RegisterRequestHandlersWithResponse(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { HandlerType = t, InterfaceType = i }))
            .ToList();

        foreach (var handler in handlerTypes) services.AddTransient(handler.InterfaceType, handler.HandlerType);
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(i => new { HandlerType = t, InterfaceType = i }))
            .ToList();

        foreach (var handler in handlerTypes) services.AddTransient(handler.InterfaceType, handler.HandlerType);
    }
}