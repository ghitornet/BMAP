using System.Reflection;
using BMAP.Core.Mediator.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Extensions;

/// <summary>
///     Extension methods for registering mediator services with the dependency injection container.
///     Includes support for CQRS pattern with command and query handling.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the mediator services to the service collection.
    ///     This includes core mediator functionality and CQRS support.
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
    ///     Adds the mediator services with CQRS behaviors to the service collection.
    ///     This includes command and query specific logging and validation behaviors.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorWithCqrs(this IServiceCollection services)
    {
        services.AddMediator();
        
        // Register generic validation behavior (works for all requests)
        services.TryAddTransient(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // Register generic logging behavior (works for all requests)
        services.TryAddTransient(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>));
        services.TryAddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

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
    ///     Adds the mediator services with CQRS support to the service collection and registers handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorWithCqrs(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatorWithCqrs();
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
    ///     Adds the mediator services with CQRS support to the service collection and registers handlers from the assemblies containing the
    ///     specified types.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorWithCqrsFromAssemblyContaining<T>(this IServiceCollection services)
    {
        return services.AddMediatorWithCqrs(typeof(T).Assembly);
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
    ///     Adds the mediator services with CQRS support to the service collection and registers handlers from the assemblies containing the
    ///     specified types.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="markerTypes">Types whose assemblies will be scanned for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorWithCqrsFromAssemblyContaining(this IServiceCollection services,
        params Type[] markerTypes)
    {
        var assemblies = markerTypes.Select(t => t.Assembly).Distinct().ToArray();
        return services.AddMediatorWithCqrs(assemblies);
    }

    /// <summary>
    ///     Registers request and notification handlers from the specified assemblies.
    ///     Supports CQRS command and query handlers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection RegisterHandlersFromAssemblies(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies) 
            RegisterHandlersFromAssembly(services, assembly);

        return services;
    }

    /// <summary>
    ///     Registers request and notification handlers from the specified assembly.
    ///     Supports CQRS command and query handlers.
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

        // Register CQRS command handlers
        RegisterCommandHandlers(services, types);

        // Register CQRS command handlers with response
        RegisterCommandHandlersWithResponse(services, types);

        // Register CQRS query handlers
        RegisterQueryHandlers(services, types);

        // Register notification handlers
        RegisterNotificationHandlers(services, types);

        // Register validators
        RegisterValidators(services, types);

        // Register pipeline behaviors
        RegisterPipelineBehaviors(services, types);

        return services;
    }

    private static void RegisterRequestHandlers(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
                .Select(i => new { HandlerType = t, InterfaceType = i }))
            .ToList();

        foreach (var handler in handlerTypes) 
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
    }

    private static void RegisterRequestHandlersWithResponse(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { HandlerType = t, InterfaceType = i }))
            .ToList();

        foreach (var handler in handlerTypes) 
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
    }

    private static void RegisterCommandHandlers(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                .Select(i => new { HandlerType = t, InterfaceType = i, CommandType = i.GetGenericArguments()[0] }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            // Register as ICommandHandler<TCommand>
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
            
            // Also register as IRequestHandler<TCommand> so the mediator can find it
            var requestHandlerInterface = typeof(IRequestHandler<>).MakeGenericType(handler.CommandType);
            services.AddTransient(requestHandlerInterface, handler.HandlerType);
        }
    }

    private static void RegisterCommandHandlersWithResponse(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                .Select(i => new { HandlerType = t, InterfaceType = i, CommandType = i.GetGenericArguments()[0], ResponseType = i.GetGenericArguments()[1] }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            // Register as ICommandHandler<TCommand, TResponse>
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
            
            // Also register as IRequestHandler<TCommand, TResponse> so the mediator can find it
            var requestHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(handler.CommandType, handler.ResponseType);
            services.AddTransient(requestHandlerInterface, handler.HandlerType);
        }
    }

    private static void RegisterQueryHandlers(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                .Select(i => new { HandlerType = t, InterfaceType = i, QueryType = i.GetGenericArguments()[0], ResponseType = i.GetGenericArguments()[1] }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            // Register as IQueryHandler<TQuery, TResponse>
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
            
            // Also register as IRequestHandler<TQuery, TResponse> so the mediator can find it
            var requestHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(handler.QueryType, handler.ResponseType);
            services.AddTransient(requestHandlerInterface, handler.HandlerType);
        }
    }

    private static void RegisterNotificationHandlers(IServiceCollection services, IEnumerable<Type> types)
    {
        var handlerTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(i => new { HandlerType = t, InterfaceType = i }))
            .ToList();

        foreach (var handler in handlerTypes) 
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
    }

    private static void RegisterValidators(IServiceCollection services, IEnumerable<Type> types)
    {
        var validatorTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new { ValidatorType = t, InterfaceType = i }))
            .ToList();

        foreach (var validator in validatorTypes) 
            services.AddTransient(validator.InterfaceType, validator.ValidatorType);
    }

    private static void RegisterPipelineBehaviors(IServiceCollection services, IEnumerable<Type> types)
    {
        // Register pipeline behaviors without response
        var behaviorTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>))
                .Select(i => new { BehaviorType = t, InterfaceType = i }))
            .ToList();

        foreach (var behavior in behaviorTypes) 
            services.AddTransient(behavior.InterfaceType, behavior.BehaviorType);

        // Register pipeline behaviors with response
        var behaviorWithResponseTypes = types
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                .Select(i => new { BehaviorType = t, InterfaceType = i }))
            .ToList();

        foreach (var behavior in behaviorWithResponseTypes) 
            services.AddTransient(behavior.InterfaceType, behavior.BehaviorType);
    }

    /// <summary>
    ///     Registers a command handler and automatically registers it as the base request handler interface as well.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
        where TCommand : class, ICommand
        where THandler : class, ICommandHandler<TCommand>
    {
        services.AddTransient<ICommandHandler<TCommand>, THandler>();
        services.AddTransient<IRequestHandler<TCommand>, THandler>();
        return services;
    }

    /// <summary>
    ///     Registers a command handler with response and automatically registers it as the base request handler interface as well.
    /// </summary>
    /// <typeparam name="TCommand">The type of command.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : class, ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddTransient<ICommandHandler<TCommand, TResponse>, THandler>();
        services.AddTransient<IRequestHandler<TCommand, TResponse>, THandler>();
        return services;
    }

    /// <summary>
    ///     Registers a query handler and automatically registers it as the base request handler interface as well.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    /// <typeparam name="THandler">The type of handler.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddQueryHandler<TQuery, TResponse, THandler>(this IServiceCollection services)
        where TQuery : class, IQuery<TResponse>
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddTransient<IQueryHandler<TQuery, TResponse>, THandler>();
        services.AddTransient<IRequestHandler<TQuery, TResponse>, THandler>();
        return services;
    }
}