# BMAP.Core.Data

A comprehensive data access abstraction layer for .NET 9 applications, providing interfaces and CQRS patterns for entity management with support for multiple data providers.

## Features

- Provider-Agnostic Design: Abstract interfaces that work with any data provider
- CQRS Implementation: Full Command Query Responsibility Segregation support
- Entity Interfaces: Well-designed interfaces for ID, creation, modification, and soft deletion tracking
- Pagination Support: Built-in pagination with comprehensive result metadata
- Type-Safe Operations: Generic interfaces with proper type constraints
- Audit Trail Support: Comprehensive audit tracking with creation and modification metadata
- Soft Deletion: Logical deletion support with audit trail
- Result Pattern Integration: Full integration with BMAP.Core.Result for robust error handling
- Mediator Integration: Seamless integration with BMAP.Core.Mediator for CQRS operations

## Installation

```bash
dotnet add package BMAP.Core.Data
```

## Basic Usage

```csharp
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using BMAP.Core.Result;

// Get entity by ID
var getUserQuery = new GetEntityByIdQuery<User>(123);
var userResult = await mediator.SendAsync<Result<User>>(getUserQuery);

// Create entity
var newUser = new User { Name = "John Doe", Email = "john@example.com" };
var createCommand = new CreateEntityCommand<User>(newUser);
var createResult = await mediator.SendAsync<Result<int>>(createCommand);
```

## Entity Interfaces

```csharp
using BMAP.Core.Data.Entities;

public class User : IAuditableEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // ICreatable
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    
    // IModifiable
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
```

## License

This project is licensed under the MIT License.