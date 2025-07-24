# BMAP.Core.Data.Dapper

Dapper implementation for BMAP.Core.Data providing high-performance data access with automatic CRUD operations and CQRS pattern support.

## Features

- High-Performance Data Access: Built on Dapper for maximum performance
- Automatic SQL Generation: Attributes-based SQL generation for CRUD operations
- CQRS Handler Implementation: Complete implementation of all CRUD handlers
- Attribute-Based Mapping: Simple attributes to control database mapping
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
using BMAP.Core.Data.Dapper.Attributes;

// Configure services
builder.Services.AddDapperDataServices();
builder.Services.AddSqlServerConnection(connectionString);
builder.Services.AddEntityHandlers<User>();

// Define entity with attributes
[Table("Users")]
public class User : IAuditableEntity
{
    [Column("Id", IsPrimaryKey = true, IsIdentity = true)]
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

## Usage

```csharp
// Use the standard BMAP.Core.Data interfaces
var query = new GetEntityByIdQuery<User>(1);
var result = await mediator.SendAsync<Result<User>>(query);

var createCommand = new CreateEntityCommand<User>(newUser);
var createResult = await mediator.SendAsync<Result<int>>(createCommand);
```

## License

This project is licensed under the MIT License.