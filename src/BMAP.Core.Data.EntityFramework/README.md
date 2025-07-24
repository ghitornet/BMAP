# BMAP.Core.Data.EntityFramework

Entity Framework Core implementation for BMAP.Core.Data providing comprehensive ORM support with automatic CRUD operations, change tracking, and CQRS pattern support.

## Features

- **?? NEW: ContextAttribute System** - Entities automatically discover their DbContext
- **?? NEW: Context-Agnostic Handlers** - Handlers automatically resolve the correct DbContext
- **?? NEW: AutoDiscovery DbContext** - Automatic entity discovery and configuration
- **Full EF Core Integration**: Leverages Entity Framework Core for comprehensive ORM functionality
- **Automatic Change Tracking**: Built-in change tracking with optimized queries
- **Multiple Database Support**: SQL Server, SQLite out of the box
- **Automatic CRUD Operations**: Complete CRUD handlers with minimal configuration
- **Global Query Filters**: Automatic soft delete filtering at the EF Core level
- **Standard DataAnnotations**: Uses familiar EF Core mapping conventions
- **Audit Trail Support**: Automatic handling of audit fields
- **Soft Deletion**: Built-in soft deletion with global query filters
- **Result Pattern Integration**: All operations return Result types
- **Dependency Injection Ready**: Easy registration with DI container
- **Migration Support**: Full EF Core migrations and database schema management

## Installation

```bash
dotnet add package BMAP.Core.Data.EntityFramework
```

## ?? Quick Start with ContextAttribute

### 1. Define Your Entities with Context Mapping

```csharp
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// User entity belongs to UserDbContext
[Context("UserDbContext")]
[Table("Users")]
public class User : IAuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    // IAuditableEntity properties
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

// Product entity belongs to ProductDbContext
[Context("ProductDbContext")]
[Table("Products")]
public class Product : IAuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    // IAuditableEntity properties
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

// Entity that can belong to multiple contexts
[Context("UserDbContext", "AuditDbContext")]
[Table("UserProfiles")]
public class UserProfile : IAuditableEntity
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public string Bio { get; set; } = string.Empty;
    
    // IAuditableEntity properties
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

### 2. Create AutoDiscovery DbContexts

```csharp
using BMAP.Core.Data.EntityFramework.Base;
using Microsoft.EntityFrameworkCore;

public class UserDbContext : AutoDiscoveryDbContext<UserDbContext>
{
    public UserDbContext(DbContextOptions<UserDbContext> options) 
        : base(options, typeof(User).Assembly) // Scan assembly for entities
    {
    }

    // DbSets are automatically discovered and configured
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreatingCore(ModelBuilder modelBuilder)
    {
        // Additional configuration specific to UserDbContext
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        base.OnModelCreatingCore(modelBuilder);
    }
}

public class ProductDbContext : AutoDiscoveryDbContext<ProductDbContext>
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) 
        : base(options, typeof(Product).Assembly)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreatingCore(ModelBuilder modelBuilder)
    {
        // Additional configuration specific to ProductDbContext
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name);
            
        base.OnModelCreatingCore(modelBuilder);
    }
}
```

### 3. Configure Services with Auto-Discovery

```csharp
using BMAP.Core.Data.EntityFramework.Extensions;

// Configure services with automatic entity discovery
builder.Services.AddEntityFrameworkDataServicesWithAutoScan(
    typeof(User).Assembly, 
    typeof(Product).Assembly);

// Add contexts with automatic entity registration
builder.Services.AddSqlServerContextWithAutoDiscovery<UserDbContext>(
    "Server=localhost;Database=Users;Integrated Security=true;",
    new[] { typeof(User).Assembly });

builder.Services.AddSqliteContextWithAutoDiscovery<ProductDbContext>(
    "Data Source=products.db",
    new[] { typeof(Product).Assembly });
```

### 4. Use the Handlers - They Automatically Find the Right Context!

```csharp
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using BMAP.Core.Result;

public class UserService
{
    private readonly IMediator _mediator;

    public UserService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<User>> CreateUserAsync(string name, string email)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            CreatedBy = "system"
        };

        // Handler automatically resolves to UserDbContext based on [Context] attribute
        var command = new CreateEntityCommand<User>(user);
        var createResult = await _mediator.SendAsync<Result<int>>(command);
        
        if (createResult.IsFailure)
            return Result<User>.Failure(createResult.Error);

        // Get the created user
        var query = new GetEntityByIdQuery<User>(createResult.Value);
        return await _mediator.SendAsync<Result<User>>(query);
    }
}

public class ProductService
{
    private readonly IMediator _mediator;

    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<Product>> CreateProductAsync(string name, decimal price)
    {
        var product = new Product
        {
            Name = name,
            Price = price,
            CreatedBy = "system"
        };

        // Handler automatically resolves to ProductDbContext based on [Context] attribute
        var command = new CreateEntityCommand<Product>(product);
        var createResult = await _mediator.SendAsync<Result<int>>(command);
        
        if (createResult.IsFailure)
            return Result<Product>.Failure(createResult.Error);

        var query = new GetEntityByIdQuery<Product>(createResult.Value);
        return await _mediator.SendAsync<Result<Product>>(query);
    }
}
```

## ?? Key Benefits of the New ContextAttribute System

### ? **Context Isolation**
```csharp
// Users and Products are automatically isolated in different databases
var user = await userService.CreateUserAsync("John", "john@example.com");     // ? UserDbContext ? Users DB
var product = await productService.CreateProductAsync("Laptop", 999.99m);    // ? ProductDbContext ? Products DB
```

### ? **No DbContext Dependencies**
```csharp
// Handlers don't depend on specific DbContext anymore!
public class CreateEntityHandler<TEntity, TId> : ICommandHandler<...>
{
    // Takes IContextResolver instead of DbContext
    public CreateEntityHandler(IContextResolver contextResolver, ILogger logger) { }
}
```

### ? **Automatic Discovery**
```csharp
// Entities are automatically discovered and configured
[Context("UserDbContext")]
public class User : IAuditableEntity
{
    // No manual registration needed!
}
```

### ? **Multiple Context Support**
```csharp
// Entity can belong to multiple contexts
[Context("UserDbContext", "AuditDbContext")]
public class UserProfile : IAuditableEntity
{
    // Will be available in both contexts
}
```

## Advanced Configuration

### Custom Context Resolution

```csharp
public class CustomContextResolver : IContextResolver
{
    public DbContext ResolveContext<TEntity>() where TEntity : class
    {
        // Custom logic to resolve context
        // e.g., based on tenant, user permissions, etc.
    }
}

// Register custom resolver
builder.Services.AddScoped<IContextResolver, CustomContextResolver>();
```

### Manual Handler Registration

```csharp
// For fine-grained control, register handlers manually
builder.Services.AddEntityHandlers<User>();
builder.Services.AddEntityHandlers<Product>();
```

### Legacy Support

```csharp
// Legacy methods still work for backward compatibility
builder.Services.AddSqlServerContext<ApplicationDbContext>(connectionString);
builder.Services.AddEntityHandlers<User>();
```

## Database Providers

### SQL Server
```csharp
builder.Services.AddSqlServerContextWithAutoDiscovery<ApplicationDbContext>(
    "Server=(localdb)\\mssqllocaldb;Database=MyApp;Trusted_Connection=true;",
    new[] { typeof(User).Assembly });
```

### SQLite
```csharp
builder.Services.AddSqliteContextWithAutoDiscovery<ApplicationDbContext>(
    "Data Source=myapp.db",
    new[] { typeof(User).Assembly });
```

## Migrations

```bash
# Add a migration
dotnet ef migrations add InitialCreate --context UserDbContext

# Update the database
dotnet ef database update --context UserDbContext

# Generate SQL script
dotnet ef migrations script --context UserDbContext
```

## ?? Comparison with Dapper Provider

| Feature | EF Core Provider | Dapper Provider |
|---------|------------------|-----------------|
| **Context Management** | ? Automatic | ? Manual |
| **Entity Discovery** | ? Automatic | ? Manual |
| **Performance** | ???? | ????? |
| **Developer Experience** | ????? | ??? |
| **Change Tracking** | ? Automatic | ? Manual |
| **Query Complexity** | ? LINQ Support | ? Raw SQL |
| **Migrations** | ? Built-in | ? External tools |
| **Learning Curve** | ? Gentle | ? Steeper |
| **Multi-Context** | ? Automatic | ? Manual |

## When to Use EF Core Provider

- ? **Multi-tenant applications** with context isolation
- ? **Microservices** with bounded contexts
- ? **Rapid development** with minimal boilerplate
- ? **Complex domain models** with relationships
- ? **Team familiar** with LINQ and EF Core
- ? **Database schema evolution** through migrations
- ? **Automatic context resolution** based on entity type

## Migration Guide

### From Manual DbContext Registration

**Before:**
```csharp
services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
services.AddEntityHandlers<User>();
```

**After:**
```csharp
services.AddSqlServerContextWithAutoDiscovery<ApplicationDbContext>(
    connectionString,
    new[] { typeof(User).Assembly });
```

### Adding Context Attributes

**Before:**
```csharp
public class User : IAuditableEntity
{
    // No context information
}
```

**After:**
```csharp
[Context("UserDbContext")]
public class User : IAuditableEntity
{
    // Automatically maps to UserDbContext
}
```

## License

This project is licensed under the MIT License.