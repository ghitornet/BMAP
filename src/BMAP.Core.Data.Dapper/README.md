# BMAP.Core.Data.Dapper

Dapper implementation for BMAP.Core.Data providing high-performance data access with automatic CRUD operations and CQRS pattern support.

## Features

- High-Performance Data Access: Built on Dapper for maximum performance
- Automatic SQL Generation: Uses standard .NET DataAnnotations for SQL generation
- CQRS Handler Implementation: Complete implementation of all CRUD handlers
- Standard Annotations: Uses familiar DataAnnotations for mapping control
- Audit Trail Support: Automatic handling of audit fields
- Soft Deletion: Built-in soft deletion support
- Result Pattern Integration: All operations return Result types
- Dependency Injection Ready: Easy registration with DI container

## Installation

```bash
dotnet add package BMAP.Core.Data.Dapper
```

## Quick Start

```csharp
using BMAP.Core.Data.Dapper.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Configure services
builder.Services.AddDapperDataServices();
builder.Services.AddSqlServerConnection(connectionString);
builder.Services.AddEntityHandlers<User>();

// Define entity with standard DataAnnotations
[Table("Users")]
public class User : IAuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public int Id { get; set; }
    
    [Column("Name")]
    public string Name { get; set; } = string.Empty;
    
    [Column("Email")]
    public string Email { get; set; } = string.Empty;
    
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
    
    [Column("CreatedBy")]
    public string? CreatedBy { get; set; }
    
    [Column("LastModifiedAt")]
    public DateTime? LastModifiedAt { get; set; }
    
    [Column("LastModifiedBy")]
    public string? LastModifiedBy { get; set; }
    
    [Column("IsDeleted")]
    public bool IsDeleted { get; set; }
    
    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }
    
    [Column("DeletedBy")]
    public string? DeletedBy { get; set; }
}
```

## Supported DataAnnotations

### Table Mapping
- `[Table("table_name")]` - Specifies the database table name

### Column Mapping
- `[Column("column_name")]` - Specifies the database column name
- `[Key]` - Marks a property as the primary key
- `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` - Marks a column as auto-increment
- `[NotMapped]` - Excludes a property from database mapping

### Example Entity

```csharp
[Table("Products")]
public class Product : IAuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("product_id")]
    public int Id { get; set; }
    
    [Column("product_name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Column("price")]
    public decimal Price { get; set; }
    
    [NotMapped]
    public string DisplayName => $"{Name} - ${Price}";
    
    // IAuditableEntity properties...
}
```

## Usage

```csharp
// Use the standard BMAP.Core.Data interfaces
var query = new GetEntityByIdQuery<User>(1);
var result = await mediator.SendAsync<Result<User>>(query);

var createCommand = new CreateEntityCommand<User>(newUser);
var createResult = await mediator.SendAsync<Result<int>>(createCommand);
```

## Benefits of Using DataAnnotations

- **Standard**: Uses the same annotations as Entity Framework and ASP.NET Core
- **Validation**: Can be used with model validation in web applications  
- **Familiar**: Developers already know these attributes
- **Tooling**: Better IDE support and IntelliSense
- **Interoperability**: Works seamlessly with other .NET components

## License

This project is licensed under the MIT License.