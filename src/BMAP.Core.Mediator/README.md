# BMAP.Core.Mediator

A powerful and lightweight implementation of the Mediator pattern for .NET 9, featuring both request/response and publish-subscribe patterns with comprehensive pipeline behaviors and **full CQRS (Command Query Responsibility Segregation) support**.

## Features

- ? **Request/Response Pattern**: Send commands and queries with or without responses
- ? **Publish-Subscribe Pattern**: Publish notifications to multiple handlers
- ? **CQRS Implementation**: Full Command Query Responsibility Segregation support
- ? **Pipeline Behaviors**: Support for cross-cutting concerns (logging, validation, etc.)
- ? **CQRS-Specific Behaviors**: Specialized logging and validation for commands and queries
- ? **Dependency Injection Integration**: Seamless integration with Microsoft.Extensions.DependencyInjection
- ? **Async/Await Support**: Full asynchronous operation support
- ? **Cancellation Token Support**: Proper cancellation handling throughout
- ? **Assembly Scanning**: Automatic handler registration from assemblies
- ? **Exception Handling**: Comprehensive error handling with custom exceptions
- ? **Validation Support**: Built-in validation pipeline behavior
- ? **Enhanced Logging**: Context-aware logging that distinguishes between commands, queries, and events
- ? **Performance Monitoring**: Built-in performance metrics and optimization suggestions

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

// Basic mediator registration
services.AddMediatorFromAssemblyContaining<Program>();

// Or with full CQRS support including specialized behaviors
services.AddMediatorWithCqrsFromAssemblyContaining<Program>();

var serviceProvider = services.BuildServiceProvider();
var mediator = serviceProvider.GetRequiredService<IMediator>();
```

## CQRS Implementation

### Commands (Write Operations)

Commands represent the intent to change the state of the system. They should be named with imperative verbs.

#### Command without Response

```csharp
using BMAP.Core.Mediator;

// Define a command
public class CreateUserCommand : ICommand
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Define the handler
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand>
{
    public async Task HandleAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Business logic to create user
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

#### Command with Response

```csharp
// Define a command that returns data (typically identifiers)
public class CreateProductCommand : ICommand<int>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Define the handler
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, int>
{
    public async Task<int> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        // Business logic to create product
        await Task.Delay(50, cancellationToken); // Simulate database operation
        
        var productId = Random.Shared.Next(1000, 9999);
        Console.WriteLine($"Created product: {request.Name} with ID: {productId}");
        
        return productId;
    }
}

// Usage
var productId = await mediator.SendAsync<int>(new CreateProductCommand 
{ 
    Name = "Gaming Laptop", 
    Price = 1299.99m, 
    Stock = 5 
});
```

### Queries (Read Operations)

Queries are read-only operations that retrieve data without modifying system state.

```csharp
// Define a query
public class GetUserQuery : IQuery<UserDto>
{
    public int UserId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Define the handler
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        // Read-only data access logic
        await Task.Delay(30, cancellationToken); // Simulate database call
        
        return new UserDto
        {
            Id = request.UserId,
            Name = $"User {request.UserId}",
            Email = $"user{request.UserId}@example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
    }
}

// Usage
var user = await mediator.SendAsync<UserDto>(new GetUserQuery { UserId = 123 });
Console.WriteLine($"Retrieved user: {user.Name}");
```

### Domain Events and Notifications

```csharp
// Define a domain event
public class UserCreatedEvent : INotification
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Define multiple handlers for the same event
public class EmailNotificationHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        // Send welcome email
        Console.WriteLine($"Sending welcome email to {notification.Email}");
        await Task.Delay(100, cancellationToken);
    }
}

public class AuditLogHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        // Log to audit system
        Console.WriteLine($"Audit: User {notification.UserId} created at {notification.CreatedAt}");
        await Task.Delay(50, cancellationToken);
    }
}

// Usage - Fire and forget
await mediator.PublishAsync(new UserCreatedEvent 
{ 
    UserId = 123, 
    UserName = "John Doe", 
    Email = "john@example.com",
    CreatedAt = DateTime.UtcNow
});

// Usage - Wait for all handlers to complete
await mediator.PublishAndWaitAsync(new UserCreatedEvent 
{ 
    UserId = 123, 
    UserName = "John Doe", 
    Email = "john@example.com",
    CreatedAt = DateTime.UtcNow
});
```

## CQRS-Specific Pipeline Behaviors

### Command Logging Behavior

Provides specialized logging for commands with performance monitoring:

```csharp
using BMAP.Core.Mediator.Behaviors;

// Automatically logs:
// - Command execution start/completion
// - Execution timing
// - Performance warnings for slow commands (>5 seconds)
// - Command data (at debug level)
// - Error details with timing information

// Example log output:
// [Information] Executing command CreateUserCommand at 2024-01-15T10:30:00Z
// [Information] Command CreateUserCommand executed successfully in 150ms
// [Warning] Command CreateProductCommand execution took 6000ms which exceeds the recommended threshold
```

### Query Logging Behavior

Provides specialized logging for queries with optimization suggestions:

```csharp
using BMAP.Core.Mediator.Behaviors;

// Automatically logs:
// - Query execution start/completion
// - Execution timing
// - Caching opportunities for slow queries (>1 second)
// - Performance warnings for very slow queries (>10 seconds)
// - Large dataset warnings with pagination suggestions
// - Query parameters and response size

// Example log output:
// [Information] Executing query GetUsersQuery expecting response List<UserDto> at 2024-01-15T10:30:00Z
// [Information] Query GetUsersQuery executed successfully in 1200ms
// [Information] Query GetUsersQuery took 1200ms - consider caching for performance optimization
// [Warning] Query GetProductsQuery returned 1500 items - consider pagination for large datasets
```

### CQRS Validation Behaviors

Specialized validation for commands and queries:

```csharp
// Command validation
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public Task<ValidationResult> ValidateAsync(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(new ValidationError("Name is required", nameof(request.Name)));

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            errors.Add(new ValidationError("Valid email is required", nameof(request.Email)));

        return Task.FromResult(errors.Any() 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success());
    }
}

// Query validation (for parameters)
public class GetUserQueryValidator : IValidator<GetUserQuery>
{
    public Task<ValidationResult> ValidateAsync(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (request.UserId <= 0)
            errors.Add(new ValidationError("User ID must be positive", nameof(request.UserId)));

        return Task.FromResult(errors.Any() 
            ? ValidationResult.Failure(errors.ToArray()) 
            : ValidationResult.Success());
    }
}
```

## Registration Options

### Basic Mediator Registration

```csharp
services.AddMediator();
```

### CQRS-Enhanced Registration

```csharp
// Register with CQRS-specific behaviors
services.AddMediatorWithCqrs();
```

### Assembly Scanning

```csharp
// Scan specific assemblies
services.AddMediator(typeof(Program).Assembly, typeof(OtherClass).Assembly);

// Scan assembly containing a specific type
services.AddMediatorFromAssemblyContaining<Program>();

// Scan with CQRS support
services.AddMediatorWithCqrsFromAssemblyContaining<Program>();

// Scan assemblies containing multiple types
services.AddMediatorFromAssemblyContaining(typeof(Program), typeof(OtherClass));
services.AddMediatorWithCqrsFromAssemblyContaining(typeof(Program), typeof(OtherClass));
```

### Manual Handler Registration

```csharp
services.AddMediator();

// Option 1: Register request handlers (generic)
services.AddTransient<IRequestHandler<CreateUserCommand>, CreateUserCommandHandler>();
services.AddTransient<IRequestHandler<GetUserQuery, UserDto>, GetUserQueryHandler>();

// Option 2: Register CQRS handlers (more explicit) - requires both registrations
services.AddTransient<ICommandHandler<CreateUserCommand>, CreateUserCommandHandler>();
services.AddTransient<IRequestHandler<CreateUserCommand>, CreateUserCommandHandler>();
services.AddTransient<IQueryHandler<GetUserQuery, UserDto>, GetUserQueryHandler>();
services.AddTransient<IRequestHandler<GetUserQuery, UserDto>, GetUserQueryHandler>();

// Option 3: Use convenient extension methods (RECOMMENDED)
services.AddCommandHandler<CreateUserCommand, CreateUserCommandHandler>();
services.AddCommandHandler<CreateProductCommand, int, CreateProductCommandHandler>();
services.AddQueryHandler<GetUserQuery, UserDto, GetUserQueryHandler>();

// Register event handlers
services.AddTransient<INotificationHandler<UserCreatedEvent>, EmailNotificationHandler>();

// Register validators
services.AddTransient<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
```

## Advanced CQRS Patterns

### Separation of Write and Read Models

```csharp
// Write model (optimized for commands)
public class CreateOrderCommand : ICommand<int>
{
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public PaymentDetails Payment { get; set; } = new();
}

// Read model (optimized for queries)
public class OrderSummaryQuery : IQuery<OrderSummaryDto>
{
    public int OrderId { get; set; }
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FormattedTotal { get; set; } = string.Empty;
    public List<string> ItemDescriptions { get; set; } = [];
    public DateTime OrderDate { get; set; }
}
```

### Complex Workflows with CQRS

```csharp
public async Task ProcessOrderWorkflow(CreateOrderCommand command)
{
    // 1. Execute command
    var orderId = await mediator.SendAsync<int>(command);
    
    // 2. Query for confirmation
    var orderSummary = await mediator.SendAsync<OrderSummaryDto>(
        new OrderSummaryQuery { OrderId = orderId });
    
    // 3. Publish domain events
    await mediator.PublishAsync(new OrderCreatedEvent 
    { 
        OrderId = orderId,
        CustomerEmail = command.CustomerEmail,
        TotalAmount = command.TotalAmount
    });
    
    return orderSummary;
}
```

## Error Handling

The library provides comprehensive error handling with CQRS context:

```csharp
try
{
    await mediator.SendAsync(new CreateUserCommand { Name = "", Email = "invalid" });
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
catch (HandlerNotFoundException ex)
{
    Console.WriteLine($"No handler found for {ex.RequestType.Name}");
}
catch (MediatorException ex)
{
    Console.WriteLine($"Mediator error: {ex.Message}");
}
```

## Performance Considerations

- **Commands**: Optimized for write operations with performance monitoring
- **Queries**: Optimized for read operations with caching suggestions
- **Events**: Handlers execute in parallel for better performance
- **Validation**: Runs before handlers to fail fast
- **Logging**: Provides performance metrics and optimization suggestions

## CQRS Best Practices

1. **Command Naming**: Use imperative verbs (CreateUser, UpdateProduct, DeleteOrder)
2. **Query Naming**: Use nouns or questions (GetUser, FindProducts, UserExists)
3. **Separation**: Keep write and read models separate for optimal performance
4. **Validation**: Validate commands thoroughly, queries minimally
5. **Events**: Use for cross-boundary communication and side effects
6. **Performance**: Monitor execution times and follow optimization suggestions

## Using with ASP.NET Core

```csharp
// Program.cs
using BMAP.Core.Mediator.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register mediator with CQRS support
builder.Services.AddMediatorWithCqrsFromAssemblyContaining<Program>();

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
    public async Task<ActionResult<int>> CreateUser(CreateUserCommand command)
    {
        try
        {
            var userId = await _mediator.SendAsync<int>(command);
            return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
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

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.