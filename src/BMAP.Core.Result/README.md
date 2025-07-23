# BMAP.Core.Result

[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![C# 13.0](https://img.shields.io/badge/C%23-13.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/BMAP.Core.Result.svg)](https://www.nuget.org/packages/BMAP.Core.Result/)

A comprehensive and robust **Result pattern implementation** for .NET 9 applications, providing a functional approach to error handling and operation outcomes. This library enables you to write more maintainable, readable, and predictable code by explicitly handling success and failure cases without relying on exceptions.

## ?? Features

- ?? **Type-Safe Error Handling**: Eliminates null reference exceptions and provides compile-time safety
- ?? **Functional Programming Support**: Comprehensive extension methods for method chaining and functional composition
- ?? **Rich Error Information**: Detailed error types with metadata support for better debugging and monitoring
- ? **Async/Await Support**: Full asynchronous operation support with proper async patterns
- ??? **Utility Methods**: Common operations like validation, exception handling, and result aggregation
- ?? **Performance Optimized**: Minimal allocations and efficient memory usage
- ?? **Zero Dependencies**: No external dependencies, lightweight and focused
- ?? **Extensively Tested**: Over 130+ unit tests ensuring reliability and robustness

## ?? Installation

### Package Manager Console
```powershell
Install-Package BMAP.Core.Result
```

### .NET CLI
```bash
dotnet add package BMAP.Core.Result
```

### PackageReference
```xml
<PackageReference Include="BMAP.Core.Result" Version="1.0.0" />
```

## ?? Quick Start

### Basic Usage

```csharp
using BMAP.Core.Result;

// Simple success/failure operations
public Result ValidateUser(string email)
{
    if (string.IsNullOrWhiteSpace(email))
        return Result.Failure("Email is required");
    
    if (!email.Contains("@"))
        return Result.Failure("Invalid email format");
    
    return Result.Success();
}

// Operations that return values
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        return Result<User>.Failure("User not found");
    
    return Result<User>.Success(user);
}
```

### Using Extension Methods

```csharp
using BMAP.Core.Result.Extensions;

public async Task<Result<string>> ProcessUserWorkflow(int userId)
{
    return await GetUser(userId)
        .Bind(user => ValidateUser(user.Email))
        .Bind(_ => UpdateLastLogin(userId))
        .Map(() => "User processed successfully")
        .TapAsync(message => LogAsync(message))
        .TapError(error => LogErrorAsync(error));
}
```

### Error Types and Metadata

```csharp
// Different error types for better categorization
var validationError = Error.Validation("User.Email", "Invalid email format")
    .WithMetadata("Field", "Email")
    .WithMetadata("Value", invalidEmail);

var notFoundError = Error.NotFound("User.NotFound", "User does not exist")
    .WithMetadata("UserId", userId);

var conflictError = Error.Conflict("User.Duplicate", "User already exists")
    .WithMetadata("Email", existingEmail);
```

## ?? Core Concepts

### Result Types

#### Result
Represents an operation that can succeed or fail without returning a value:

```csharp
Result saveResult = SaveUser(user);
if (saveResult.IsSuccess)
{
    // Operation succeeded
}
else
{
    // Handle error: saveResult.Error
}
```

#### Result&lt;T&gt;
Represents an operation that can succeed with a value or fail with an error:

```csharp
Result<User> userResult = GetUser(123);
if (userResult.IsSuccess)
{
    User user = userResult.Value;
    // Use the user
}
else
{
    Error error = userResult.Error;
    // Handle the error
}
```

### Safe Value Access

```csharp
var userResult = GetUser(userId);

// Safe access patterns
if (userResult.TryGetValue(out var user))
{
    // Use user safely
}

// With fallback
var user = userResult.GetValueOrDefault(defaultUser);

// Or get null if failed
var userOrNull = userResult.GetValueOrDefault();
```

### Error Information

The `Error` class provides rich error information:

```csharp
public sealed class Error
{
    public string Code { get; }           // Unique error identifier
    public string Message { get; }        // Human-readable message
    public ErrorType Type { get; }        // Error category
    public IReadOnlyDictionary<string, object> Metadata { get; } // Additional context
    public bool IsNone { get; }          // True if no error (success case)
}
```

#### Error Types

| ErrorType | Description | Use Case |
|-----------|-------------|----------|
| `None` | No error (success) | Default for successful operations |
| `General` | General/uncategorized error | Fallback for unspecified errors |
| `Validation` | Input validation error | Data validation failures |
| `NotFound` | Resource not found | Entity or resource lookup failures |
| `Conflict` | Conflict with current state | Concurrency or business rule violations |
| `Unauthorized` | Authentication required | Missing or invalid authentication |
| `Forbidden` | Access denied | Insufficient permissions |
| `Internal` | Internal system error | System or infrastructure failures |
| `External` | External dependency error | Third-party service failures |
| `Custom` | Application-specific error | Domain-specific error conditions |

#### Error Factory Methods

```csharp
// Create specific error types
var validationError = Error.Validation("EMAIL_INVALID", "Email format is invalid");
var notFoundError = Error.NotFound("USER_NOT_FOUND", "User does not exist");
var conflictError = Error.Conflict("EMAIL_EXISTS", "Email already in use");
var unauthorizedError = Error.Unauthorized("AUTH_REQUIRED", "Authentication required");
var forbiddenError = Error.Forbidden("ACCESS_DENIED", "Insufficient permissions");
var internalError = Error.Internal("DB_CONNECTION", "Database connection failed");
var externalError = Error.External("API_TIMEOUT", "External API timeout");
var customError = Error.Custom("BUSINESS_RULE", "Custom business rule violation");
```

## ?? Functional Programming Extensions

### Bind Operations
Chain operations that return Results:

```csharp
Result<decimal> CalculateOrderTotal(int orderId)
{
    return GetOrder(orderId)
        .Bind(order => ValidateOrder(order))
        .Bind(order => CalculateSubtotal(order))
        .Bind(subtotal => ApplyDiscounts(subtotal))
        .Bind(total => ApplyTaxes(total));
}
```

### Map Operations
Transform successful values:

```csharp
Result<string> FormatUser(int userId)
{
    return GetUser(userId)
        .Map(user => $"{user.FirstName} {user.LastName}")
        .Map(fullName => fullName.ToUpper());
}
```

### Match Operations
Handle both success and failure cases:

```csharp
string message = GetUser(userId)
    .Match(
        user => $"Welcome, {user.Name}!",
        error => $"Error: {error.Message}"
    );

// Action-based matching
GetUser(userId).Match(
    user => Console.WriteLine($"Found: {user.Name}"),
    error => Console.WriteLine($"Failed: {error.Message}")
);
```

### Tap Operations
Execute side effects without changing the result:

```csharp
Result<User> result = GetUser(userId)
    .Tap(user => _logger.LogInformation("User retrieved: {UserId}", user.Id))
    .TapError(error => _logger.LogError("Failed to get user: {Error}", error))
    .Tap(user => _metrics.IncrementCounter("user.retrieved"));
```

### Ensure Operations
Add additional validation:

```csharp
Result<User> result = GetUser(userId)
    .Ensure(user => user.IsActive, Error.Validation("User.Inactive", "User is not active"))
    .Ensure(user => !user.IsBlocked, Error.Forbidden("User.Blocked", "User is blocked"));
```

### Combine Operations
Aggregate multiple results:

```csharp
using BMAP.Core.Result.Extensions;

// All must succeed
Result combinedResult = ResultExtensions.Combine(
    ValidateEmail(user.Email),
    ValidateAge(user.Age),
    ValidateCountry(user.Country)
);
```

## ? Async Support

All extension methods have async counterparts:

```csharp
Result<UserProfile> result = await GetUserAsync(userId)
    .BindAsync(user => ValidateUserAsync(user))
    .MapAsync(user => BuildProfileAsync(user))
    .TapAsync(profile => CacheProfileAsync(profile))
    .TapAsync(profile => PublishEventAsync(profile));
```

### Async Utility Methods

```csharp
// Async exception handling
Result<string> result = await ResultUtilities.TryAsync(async () => 
{
    return await File.ReadAllTextAsync("config.json");
});

// Async operation chaining
var finalResult = await GetUserAsync(id)
    .BindAsync(async user => await UpdateUserAsync(user))
    .MapAsync(async user => await FormatUserAsync(user));
```

## ??? Utility Methods

### Exception Handling

```csharp
using BMAP.Core.Result.Utilities;

// Synchronous exception wrapping
Result<string> result = ResultUtilities.Try(() => 
{
    return File.ReadAllText("config.json");
});

// Custom error mapping
Result<string> result = ResultUtilities.Try(
    () => JsonSerializer.Deserialize<MyObject>(json),
    ex => ex switch
    {
        JsonException => Error.Validation("JSON_INVALID", "Invalid JSON format"),
        FileNotFoundException => Error.NotFound("FILE_MISSING", "Configuration file not found"),
        _ => Error.Internal("UNKNOWN_ERROR", ex.Message)
    }
);

// Async exception handling
Result<ApiResponse> result = await ResultUtilities.TryAsync(async () => 
{
    return await httpClient.GetFromJsonAsync<ApiResponse>("api/data");
});
```

### Validation Utilities

```csharp
// Simple condition validation
Result validation = ResultUtilities.Validate(
    age >= 18, 
    "Age", 
    age, 
    "Must be 18 or older"
);

// Value validation with transformation
Result<string> emailResult = ResultUtilities.Validate(
    email, 
    e => e.Contains("@") && e.Contains("."),
    "Email", 
    "Must be a valid email address"
);

// Multiple validation criteria
Result<string> passwordResult = ResultUtilities.EnsureAll(password,
    (p => !string.IsNullOrWhiteSpace(p), "Password is required"),
    (p => p.Length >= 8, "Password must be at least 8 characters"),
    (p => p.Any(char.IsUpper), "Password must contain uppercase letter"),
    (p => p.Any(char.IsDigit), "Password must contain a digit")
);
```

### Null Checking

```csharp
// Reference type null checking
Result<User> userResult = ResultUtilities.NotNull(user, "User", userId);

// Value type null checking
Result<DateTime> dateResult = ResultUtilities.NotNull(nullableDate, "Date", "appointment");

// Collection emptiness checking
Result<IEnumerable<Product>> productsResult = ResultUtilities.NotEmpty(products, "Products");
```

### Result Aggregation

```csharp
// Aggregate multiple validations
Result combinedValidation = ResultUtilities.AggregateValidations(
    ValidateName(user.Name),
    ValidateEmail(user.Email),
    ValidateAge(user.Age),
    ValidateCountry(user.Country)
);

// Returns success only if ALL validations pass
// Returns detailed error information if any fail
```

## ?? Real-World Examples

### ASP.NET Core Web API

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetUserAsync(id);
        
        return result.Match(
            user => Ok(user),
            error => error.Type switch
            {
                ErrorType.NotFound => NotFound(error.Message),
                ErrorType.Validation => BadRequest(error.Message),
                ErrorType.Unauthorized => Unauthorized(error.Message),
                ErrorType.Forbidden => Forbid(error.Message),
                _ => StatusCode(500, "Internal server error")
            }
        );
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _userService.ValidateAndCreateUserAsync(request)
            .TapAsync(user => _logger.LogInformation("User created: {UserId}", user.Id))
            .TapError(error => _logger.LogWarning("User creation failed: {Error}", error));
            
        return result.Match(
            user => CreatedAtAction(nameof(GetUser), new { id = user.Id }, user),
            error => error.Type switch
            {
                ErrorType.Validation => BadRequest(new { error.Message, Details = error.Metadata }),
                ErrorType.Conflict => Conflict(error.Message),
                _ => StatusCode(500, "Internal server error")
            }
        );
    }
}
```

### Service Layer Implementation

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IValidator<CreateUserRequest> _validator;

    public async Task<Result<User>> GetUserAsync(int id)
    {
        return await ResultUtilities.TryAsync(async () =>
        {
            var user = await _repository.GetByIdAsync(id);
            return ResultUtilities.NotNull(user, "User", id);
        })
        .BindAsync(result => result);
    }
    
    public async Task<Result<User>> ValidateAndCreateUserAsync(CreateUserRequest request)
    {
        return await ValidateCreateRequest(request)
            .BindAsync(_ => CheckEmailAvailabilityAsync(request.Email))
            .BindAsync(_ => CreateUserEntityAsync(request))
            .BindAsync(user => SendWelcomeEmailAsync(user))
            .TapAsync(user => PublishUserCreatedEventAsync(user.Id));
    }

    private async Task<Result> ValidateCreateRequest(CreateUserRequest request)
    {
        return await ResultUtilities.TryAsync(async () =>
        {
            var validationResult = await _validator.ValidateAsync(request);
            return validationResult.IsValid 
                ? Result.Success()
                : Result.Failure(Error.Validation("REQUEST_INVALID", string.Join("; ", validationResult.Errors)));
        });
    }

    private async Task<Result> CheckEmailAvailabilityAsync(string email)
    {
        var existingUser = await _repository.GetByEmailAsync(email);
        return existingUser == null 
            ? Result.Success()
            : Result.Failure(Error.Conflict("EMAIL_EXISTS", "Email address is already in use"));
    }
}
```

### Domain Logic with Business Rules

```csharp
public class OrderService
{
    public Result<Order> ProcessOrder(CreateOrderRequest request)
    {
        return ValidateOrderRequest(request)
            .Bind(_ => CreateOrderFromRequest(request))
            .Bind(order => ApplyBusinessRules(order))
            .Bind(order => CalculateTotals(order))
            .Tap(order => LogOrderCreation(order));
    }

    private Result<Order> ApplyBusinessRules(Order order)
    {
        return Result<Order>.Success(order)
            .Ensure(o => o.Items.Any(), Error.Validation("NO_ITEMS", "Order must have at least one item"))
            .Ensure(o => o.CustomerId > 0, Error.Validation("INVALID_CUSTOMER", "Valid customer required"))
            .Bind(o => CheckInventoryAvailability(o))
            .Bind(o => ValidateCustomerCredit(o));
    }

    private Result<Order> CheckInventoryAvailability(Order order)
    {
        var unavailableItems = order.Items.Where(item => !_inventory.IsAvailable(item.ProductId, item.Quantity));
        
        return unavailableItems.Any()
            ? Result<Order>.Failure(Error.Conflict("INSUFFICIENT_INVENTORY", "Some items are out of stock")
                .WithMetadata("UnavailableItems", unavailableItems.ToList()))
            : Result<Order>.Success(order);
    }
}
```

### Error Handling Patterns

```csharp
public class PaymentService
{
    public async Task<Result<PaymentResult>> ProcessPaymentAsync(PaymentRequest request)
    {
        return await ValidatePaymentRequest(request)
            .BindAsync(_ => ChargePaymentMethodAsync(request))
            .TapAsync(result => LogSuccessfulPayment(result))
            .TapError(error => LogPaymentFailure(error, request))
            .MapAsync(async result => await UpdatePaymentRecordsAsync(result));
    }

    private async Task<Result<PaymentResult>> ChargePaymentMethodAsync(PaymentRequest request)
    {
        return await ResultUtilities.TryAsync(
            async () =>
            {
                var response = await _paymentGateway.ChargeAsync(request);
                return response.IsSuccessful
                    ? Result<PaymentResult>.Success(response.ToPaymentResult())
                    : Result<PaymentResult>.Failure(Error.External("PAYMENT_DECLINED", response.ErrorMessage)
                        .WithMetadata("GatewayCode", response.ErrorCode)
                        .WithMetadata("TransactionId", response.TransactionId));
            },
            ex => ex switch
            {
                TimeoutException => Error.External("PAYMENT_TIMEOUT", "Payment gateway timeout"),
                HttpRequestException => Error.External("PAYMENT_NETWORK", "Network error during payment"),
                _ => Error.Internal("PAYMENT_SYSTEM", $"Internal payment system error: {ex.Message}")
            }
        );
    }
}
```

## ?? Best Practices

### 1. Use Appropriate Error Types
```csharp
// ? Good: Specific error types with meaningful codes
return Error.NotFound("USER_NOT_FOUND", $"User with ID {id} does not exist");
return Error.Validation("EMAIL_INVALID", "Email format is invalid");
return Error.Conflict("USERNAME_TAKEN", "Username is already taken");

// ? Avoid: Generic errors without context
return Result.Failure("Something went wrong");
return Result.Failure("Error occurred");
```

### 2. Include Meaningful Metadata
```csharp
// ? Good: Rich metadata for debugging and monitoring
return Error.Validation("PASSWORD_WEAK", "Password does not meet security requirements")
    .WithMetadata("MinLength", 8)
    .WithMetadata("RequiresUppercase", true)
    .WithMetadata("RequiresDigit", true)
    .WithMetadata("ProvidedLength", password.Length);

// ? Avoid: Missing context
return Error.Validation("PASSWORD_INVALID", "Invalid password");
```

### 3. Use Method Chaining for Readability
```csharp
// ? Good: Functional approach with clear flow
return GetUser(id)
    .Ensure(user => user.IsActive, Error.Validation("USER_INACTIVE", "User account is inactive"))
    .Bind(user => ValidatePermissions(user, action))
    .Bind(user => ExecuteAction(user, action))
    .Map(result => FormatResponse(result));

// ? Avoid: Imperative approach with multiple if statements
var userResult = GetUser(id);
if (userResult.IsFailure) return userResult.Error;

var user = userResult.Value;
if (!user.IsActive) return Error.Validation("USER_INACTIVE", "User account is inactive");

var permissionResult = ValidatePermissions(user, action);
if (permissionResult.IsFailure) return permissionResult.Error;
// ... and so on
```

### 4. Handle Errors Appropriately by Type
```csharp
// ? Good: Type-specific error handling
return userResult.Match(
    user => Ok(user),
    error => error.Type switch
    {
        ErrorType.NotFound => NotFound(new { Message = error.Message, Code = error.Code }),
        ErrorType.Validation => BadRequest(new { Message = error.Message, Metadata = error.Metadata }),
        ErrorType.Unauthorized => Unauthorized(error.Message),
        ErrorType.Forbidden => Forbid(error.Message),
        ErrorType.Conflict => Conflict(error.Message),
        _ => StatusCode(500, new { Message = "Internal server error", TraceId = Activity.Current?.Id })
    }
);
```

### 5. Use Async Methods Consistently
```csharp
// ? Good: Proper async usage
Result<User> result = await GetUserAsync(id)
    .BindAsync(user => ValidateUserAsync(user))
    .MapAsync(user => ProcessUserAsync(user))
    .TapAsync(user => NotifyUserAsync(user));

// ? Avoid: Blocking async operations
var user = GetUserAsync(id).Result; // DON'T DO THIS
```

### 6. Create Domain-Specific Error Codes
```csharp
// ? Good: Consistent error code patterns
public static class UserErrors
{
    public static Error NotFound(int userId) => 
        Error.NotFound("USER_NOT_FOUND", $"User with ID {userId} was not found")
            .WithMetadata("UserId", userId);
    
    public static Error EmailAlreadyExists(string email) => 
        Error.Conflict("USER_EMAIL_EXISTS", "A user with this email already exists")
            .WithMetadata("Email", email);
    
    public static Error InvalidAge(int age) => 
        Error.Validation("USER_AGE_INVALID", "User age must be between 13 and 120")
            .WithMetadata("ProvidedAge", age)
            .WithMetadata("MinAge", 13)
            .WithMetadata("MaxAge", 120);
}
```

## ?? Migration Guide

### From Exceptions to Results

```csharp
// Before: Exception-based approach
public User GetUser(int id)
{
    var user = _repository.FindById(id);
    if (user == null)
        throw new UserNotFoundException($"User {id} not found");
    
    if (!user.IsActive)
        throw new InvalidOperationException("User is not active");
        
    return user;
}

public void ProcessUser(int id)
{
    try
    {
        var user = GetUser(id);
        // Process user
    }
    catch (UserNotFoundException ex)
    {
        // Handle not found
    }
    catch (InvalidOperationException ex)
    {
        // Handle validation error
    }
}

// After: Result-based approach
public Result<User> GetUser(int id)
{
    var user = _repository.FindById(id);
    return ResultUtilities.NotNull(user, "User", id)
        .Ensure(u => u.IsActive, Error.Validation("USER_INACTIVE", "User is not active"));
}

public Result ProcessUser(int id)
{
    return GetUser(id)
        .Bind(user => PerformProcessing(user))
        .Tap(user => LogProcessingSuccess(user))
        .TapError(error => LogProcessingFailure(error));
}
```

### From Nullable to Results

```csharp
// Before: Nullable returns
public User? FindUser(string email) => _repository.FindByEmail(email);

public string? GetUserDisplayName(string email)
{
    var user = FindUser(email);
    return user?.DisplayName;
}

// After: Result-based
public Result<User> FindUser(string email)
{
    var user = _repository.FindByEmail(email);
    return ResultUtilities.NotNull(user, "User", email);
}

public Result<string> GetUserDisplayName(string email)
{
    return FindUser(email)
        .Map(user => user.DisplayName);
}
```

### From Boolean Returns to Results

```csharp
// Before: Boolean with out parameters
public bool TryParseConfiguration(string json, out Configuration? config)
{
    config = null;
    try
    {
        config = JsonSerializer.Deserialize<Configuration>(json);
        return config != null;
    }
    catch
    {
        return false;
    }
}

// After: Result-based
public Result<Configuration> ParseConfiguration(string json)
{
    return ResultUtilities.Try(
        () => JsonSerializer.Deserialize<Configuration>(json),
        ex => Error.Validation("CONFIG_INVALID", $"Invalid configuration format: {ex.Message}")
    )
    .Bind(config => ResultUtilities.NotNull(config, "Configuration", "parsed"));
}
```

## ?? Testing

### Unit Testing Results

```csharp
[Test]
public void GetUser_WithValidId_ShouldReturnSuccess()
{
    // Arrange
    var userId = 123;
    var expectedUser = new User { Id = userId, Name = "John Doe" };
    _repository.Setup(r => r.FindById(userId)).Returns(expectedUser);

    // Act
    var result = _userService.GetUser(userId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedUser, result.Value);
}

[Test]
public void GetUser_WithInvalidId_ShouldReturnNotFoundError()
{
    // Arrange
    var userId = 999;
    _repository.Setup(r => r.FindById(userId)).Returns((User?)null);

    // Act
    var result = _userService.GetUser(userId);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    Assert.Contains("User", result.Error.Message);
}
```

### Testing Error Scenarios

```csharp
[Test]
public void CreateUser_WithExistingEmail_ShouldReturnConflictError()
{
    // Arrange
    var request = new CreateUserRequest("existing@example.com", "John", "Doe");
    _repository.Setup(r => r.GetByEmail(request.Email)).Returns(new User());

    // Act
    var result = _userService.CreateUser(request);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    Assert.Equal("EMAIL_EXISTS", result.Error.Code);
    Assert.Contains(request.Email, result.Error.Metadata["Email"].ToString());
}
```

## ?? Performance Considerations

### Memory Efficiency
- Results are implemented as lightweight structs when possible
- Error objects are immutable and can be cached/reused
- Extension methods minimize allocations through careful implementation

### Allocation Optimization
```csharp
// ? Efficient: Reuse common errors
public static class CommonErrors
{
    public static readonly Error InvalidId = Error.Validation("INVALID_ID", "ID must be positive");
    public static readonly Error Unauthorized = Error.Unauthorized("AUTH_REQUIRED", "Authentication required");
}

// ? Efficient: Use static factory methods
public static Error UserNotFound(int id) => 
    Error.NotFound("USER_NOT_FOUND", $"User {id} not found").WithMetadata("UserId", id);
```

### Async Best Practices
```csharp
// ? Proper ConfigureAwait usage (handled internally)
var result = await GetUserAsync(id)
    .BindAsync(user => ProcessUserAsync(user))  // Uses ConfigureAwait(false) internally
    .MapAsync(user => FormatUserAsync(user));

// ? Efficient async chaining
var result = await _userService.GetUserAsync(id)
    .TapAsync(user => _cache.SetAsync($"user:{id}", user))  // Fire-and-forget caching
    .MapAsync(user => _mapper.MapAsync(user));
```

## ?? Contributing

Contributions are welcome! Please ensure that:

1. **Code Quality**: Follow the established patterns and conventions
2. **Testing**: Include comprehensive unit tests for new functionality
3. **Documentation**: Update documentation for any API changes
4. **Performance**: Consider performance implications of changes
5. **Compatibility**: Maintain backward compatibility when possible

### Development Setup

```bash
git clone https://github.com/your-org/BMAP.Core.Result.git
cd BMAP.Core.Result
dotnet restore
dotnet build
dotnet test
```

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## ?? Related Projects

- **BMAP.Core.Mediator** - CQRS and Mediator pattern implementation
- **BMAP.Core.Validation** - Comprehensive validation framework
- **BMAP.Core.Logging** - Structured logging utilities

---

**Happy coding with Results!** ??

For questions, issues, or contributions, please visit our [GitHub repository](https://github.com/your-org/BMAP.Core.Result) or reach out to the BMAP Development Team.