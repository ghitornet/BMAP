# BMAP.Core.Mediator

A powerful and lightweight implementation of the Mediator pattern for .NET 9, featuring both request/response and publish-subscribe patterns with comprehensive pipeline behaviors.

## Features

- ? **Request/Response Pattern**: Send commands and queries with or without responses
- ? **Publish-Subscribe Pattern**: Publish notifications to multiple handlers
- ? **Pipeline Behaviors**: Support for cross-cutting concerns (logging, validation, etc.)
- ? **Dependency Injection Integration**: Seamless integration with Microsoft.Extensions.DependencyInjection
- ? **Async/Await Support**: Full asynchronous operation support
- ? **Cancellation Token Support**: Proper cancellation handling throughout
- ? **Assembly Scanning**: Automatic handler registration from assemblies
- ? **Exception Handling**: Comprehensive error handling with custom exceptions
- ? **Validation Support**: Built-in validation pipeline behavior
- ? **Logging Support**: Built-in logging pipeline behavior

## Installation

```bash
dotnet add package BMAP.Core.Mediator
```

## Quick Start

### 1. Register the Mediator

```csharp
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register mediator and scan current assembly for handlers
services.AddMediatorFromAssemblyContaining<Program>();

var serviceProvider = services.BuildServiceProvider();
var mediator = serviceProvider.GetRequiredService<IMediator>();
```

### 2. Define Requests and Handlers

#### Command (No Response)

```csharp
using BMAP.Core.Mediator;

// Define a command
public class CreateUserCommand : IRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Define the handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    public async Task HandleAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Create user logic here
        Console.WriteLine($"Creating user: {request.Name} ({request.Email})");
        await Task.Delay(100, cancellationToken); // Simulate work
    }
}

// Usage
await mediator.SendAsync(new CreateUserCommand 
{ 
    Name = "John Doe", 
    Email = "john@example.com" 
});
```

#### Query (With Response)

```csharp
// Define a query
public class GetUserQuery : IRequest<UserDto>
{
    public int UserId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Define the handler
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        // Get user logic here
        await Task.Delay(50, cancellationToken); // Simulate database call
        
        return new UserDto
        {
            Id = request.UserId,
            Name = $"User {request.UserId}",
            Email = $"user{request.UserId}@example.com"
        };
    }
}

// Usage
var user = await mediator.SendAsync<UserDto>(new GetUserQuery { UserId = 123 });
Console.WriteLine($"Retrieved user: {user.Name}");
```

### 3. Define Notifications and Handlers

```csharp
// Define a notification
public class UserCreatedNotification : INotification
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Define multiple handlers for the same notification
public class EmailNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Send welcome email
        Console.WriteLine($"Sending welcome email to {notification.Email}");
        await Task.Delay(100, cancellationToken);
    }
}

public class AuditNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Log to audit system
        Console.WriteLine($"Audit: User {notification.UserId} created at {DateTime.UtcNow}");
        await Task.Delay(50, cancellationToken);
    }
}

// Usage - Fire and forget (doesn't wait for handlers to complete)
await mediator.PublishAsync(new UserCreatedNotification 
{ 
    UserId = 123, 
    UserName = "John Doe", 
    Email = "john@example.com" 
});

// Usage - Wait for all handlers to complete
await mediator.PublishAndWaitAsync(new UserCreatedNotification 
{ 
    UserId = 123, 
    UserName = "John Doe", 
    Email = "john@example.com" 
});
```

## Pipeline Behaviors

Pipeline behaviors allow you to implement cross-cutting concerns that execute before your handlers.

### Built-in Logging Behavior

```csharp
using BMAP.Core.Mediator.Behaviors;

// The logging behavior is automatically included and will log execution times
// Output: [MEDIATOR] Handling GetUserQuery
// Output: [MEDIATOR] Handled GetUserQuery in 52ms
```

### Built-in Validation Behavior

```csharp
using BMAP.Core.Mediator.Behaviors;

// Define a validator
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError("Name is required", nameof(request.Name)));

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add(new ValidationError("Email is required", nameof(request.Email)));

        return Task.FromResult(errors.Any() 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success());
    }
}

// Register the validator
services.AddTransient<IValidator<CreateUserCommand>, CreateUserCommandValidator>();

// The validation will run automatically before the handler
// If validation fails, a ValidationException will be thrown
```

### Custom Pipeline Behavior

```csharp
public class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            return response;
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"Request {typeof(TRequest).Name} took {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}

// Register the behavior
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));
```

## Registration Options

### Register Mediator Only

```csharp
services.AddMediator();
```

### Register with Assembly Scanning

```csharp
// Scan specific assemblies
services.AddMediator(typeof(Program).Assembly, typeof(OtherClass).Assembly);

// Scan assembly containing a specific type
services.AddMediatorFromAssemblyContaining<Program>();

// Scan assemblies containing multiple types
services.AddMediatorFromAssemblyContaining(typeof(Program), typeof(OtherClass));
```

### Manual Handler Registration

```csharp
services.AddMediator();
services.AddTransient<IRequestHandler<CreateUserCommand>, CreateUserCommandHandler>();
services.AddTransient<IRequestHandler<GetUserQuery, UserDto>, GetUserQueryHandler>();
services.AddTransient<INotificationHandler<UserCreatedNotification>, EmailNotificationHandler>();
```

## Error Handling

The library provides several custom exceptions:

```csharp
try
{
    await mediator.SendAsync(new UnregisteredCommand());
}
catch (HandlerNotFoundException ex)
{
    Console.WriteLine($"No handler found for {ex.RequestType.Name}");
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"Validation error: {error.Message}");
        if (!string.IsNullOrEmpty(error.PropertyName))
            Console.WriteLine($"Property: {error.PropertyName}");
    }
}
catch (MediatorException ex)
{
    Console.WriteLine($"Mediator error: {ex.Message}");
}
```

## Advanced Usage

### Using with ASP.NET Core

```csharp
// Program.cs
using BMAP.Core.Mediator.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register mediator and scan all handlers in the current assembly
builder.Services.AddMediatorFromAssemblyContaining<Program>();

var app = builder.Build();

// Usage in a controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        try
        {
            await _mediator.SendAsync(command);
            return Ok();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _mediator.SendAsync<UserDto>(new GetUserQuery { UserId = id });
        return Ok(user);
    }
}
```

### Cancellation Support

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    await mediator.SendAsync(new LongRunningCommand(), cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

### Multiple Notification Handlers

```csharp
// All handlers will be executed for the same notification
public class FirstNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // First handler logic
    }
}

public class SecondNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public async Task HandleAsync(UserCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Second handler logic
    }
}

// Both handlers will be executed
await mediator.PublishAsync(new UserCreatedNotification { ... });
```

## Performance Considerations

- **Request/Response**: Handlers are resolved and executed synchronously
- **Notifications with PublishAsync**: Handlers are executed in parallel without waiting
- **Notifications with PublishAndWaitAsync**: Handlers are executed in parallel with waiting
- **Handler Registration**: Handlers are registered as transient by default
- **Pipeline Behaviors**: Execute in the order they are registered

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.