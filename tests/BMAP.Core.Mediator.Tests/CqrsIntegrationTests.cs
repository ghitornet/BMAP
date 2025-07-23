using BMAP.Core.Mediator.Behaviors;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Integration tests for the complete CQRS implementation including mediator, handlers, and behaviors.
/// </summary>
public class CqrsIntegrationTests
{
    [Fact]
    public async Task Complete_CQRS_Workflow_Should_Execute_Successfully()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        services.AddMediatorWithCqrs();
        
        // Register as both CQRS and base interfaces for manual registration
        services.AddTransient<ICommandHandler<CqrsTestCreateUserCommand>, CqrsTestCreateUserCommandHandler>();
        services.AddTransient<IRequestHandler<CqrsTestCreateUserCommand>, CqrsTestCreateUserCommandHandler>();
        
        services.AddTransient<IQueryHandler<CqrsTestGetUserQuery, CqrsTestUserDto>, CqrsTestGetUserQueryHandler>();
        services.AddTransient<IRequestHandler<CqrsTestGetUserQuery, CqrsTestUserDto>, CqrsTestGetUserQueryHandler>();
        
        services.AddTransient<INotificationHandler<CqrsTestUserCreatedNotification>, CqrsTestUserCreatedNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Act - Execute CQRS workflow: Command -> Query -> Notification
        
        // Step 1: Create user with command
        var createCommand = new CqrsTestCreateUserCommand { Name = "John Doe", Email = "john@example.com" };
        await mediator.SendAsync(createCommand);
        
        // Step 2: Query user data
        var getUserQuery = new CqrsTestGetUserQuery { UserEmail = "john@example.com" };
        var userDto = await mediator.SendAsync<CqrsTestUserDto>(getUserQuery);
        
        // Step 3: Publish notification
        var notification = new CqrsTestUserCreatedNotification { UserId = userDto.Id, UserName = userDto.Name };
        await mediator.PublishAndWaitAsync(notification);
        
        // Assert
        Assert.NotNull(userDto);
        Assert.Equal("John Doe", userDto.Name);
        Assert.Equal("john@example.com", userDto.Email);
        
        var logMessages = logOutput.ToString();
        Assert.Contains("Sending command of type CqrsTestCreateUserCommand", logMessages);
        Assert.Contains("Sending query of type CqrsTestGetUserQuery", logMessages);
        Assert.Contains("Publishing and waiting for event/notification", logMessages);
    }
    
    [Fact]
    public async Task CQRS_With_Validation_Should_Enforce_Business_Rules()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediatorWithCqrs();
        
        // Register as both CQRS and base interfaces for manual registration
        services.AddTransient<ICommandHandler<CqrsTestValidatedCommand>, CqrsTestValidatedCommandHandler>();
        services.AddTransient<IRequestHandler<CqrsTestValidatedCommand>, CqrsTestValidatedCommandHandler>();
        services.AddTransient<IValidator<CqrsTestValidatedCommand>, CqrsTestValidatedCommandValidator>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Act & Assert - Valid command should succeed
        var validCommand = new CqrsTestValidatedCommand { Amount = 100, Description = "Valid transaction" };
        await mediator.SendAsync(validCommand); // Should not throw
        
        // Act & Assert - Invalid command should fail validation
        var invalidCommand = new CqrsTestValidatedCommand { Amount = -50, Description = "" };
        var exception = await Assert.ThrowsAsync<ValidationException>(() => mediator.SendAsync(invalidCommand));
        Assert.Contains("Amount must be positive", exception.Errors.First().Message);
    }
    
    [Fact]
    public async Task CQRS_Separation_Should_Allow_Different_Models()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediatorWithCqrs();
        
        // Register as both CQRS and base interfaces for manual registration
        services.AddTransient<ICommandHandler<CqrsTestComplexCommand, int>, CqrsTestComplexCommandHandler>();
        services.AddTransient<IRequestHandler<CqrsTestComplexCommand, int>, CqrsTestComplexCommandHandler>();
        services.AddTransient<IQueryHandler<CqrsTestComplexQuery, CqrsTestComplexReadModel>, CqrsTestComplexQueryHandler>();
        services.AddTransient<IRequestHandler<CqrsTestComplexQuery, CqrsTestComplexReadModel>, CqrsTestComplexQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Act - Command with write-optimized model
        var command = new CqrsTestComplexCommand 
        { 
            Title = "Test Product",
            BasePrice = 99.99m,
            CategoryId = 1,
            Tags = ["electronics", "gadget"]
        };
        var productId = await mediator.SendAsync<int>(command);
        
        // Act - Query with read-optimized model
        var query = new CqrsTestComplexQuery { ProductId = productId };
        var readModel = await mediator.SendAsync<CqrsTestComplexReadModel>(query);
        
        // Assert - Different models for different purposes
        Assert.True(productId > 0);
        Assert.NotNull(readModel);
        Assert.Equal("Test Product", readModel.DisplayName);
        Assert.Equal("$99.99", readModel.FormattedPrice);
        Assert.Equal("electronics, gadget", readModel.TagsDisplay);
    }
    
    [Fact]
    public async Task CQRS_Performance_Monitoring_Should_Log_Metrics()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var services = new ServiceCollection();
        
        services.AddLogging(builder =>
        {
            builder.AddProvider(new TestLoggerProvider(logOutput));
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        services.AddMediatorWithCqrs();
        
        // Register as both CQRS and base interfaces for manual registration
        services.AddTransient<ICommandHandler<CqrsTestPerformanceCommand>, CqrsTestPerformanceCommandHandler>();
        services.AddTransient<IRequestHandler<CqrsTestPerformanceCommand>, CqrsTestPerformanceCommandHandler>();
        services.AddTransient<IQueryHandler<CqrsTestPerformanceQuery, List<string>>, CqrsTestPerformanceQueryHandler>();
        services.AddTransient<IRequestHandler<CqrsTestPerformanceQuery, List<string>>, CqrsTestPerformanceQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Act - Execute command and query
        var command = new CqrsTestPerformanceCommand { DataSize = 1000 };
        await mediator.SendAsync(command);
        
        var query = new CqrsTestPerformanceQuery { ItemCount = 1500 };
        var result = await mediator.SendAsync<List<string>>(query);
        
        // Assert
        Assert.Equal(1500, result.Count);
        
        var logMessages = logOutput.ToString();
        Assert.Contains("Successfully executed command", logMessages); // Should log execution time
        Assert.Contains("Successfully executed query", logMessages); // Should log execution time
    }

    // Test classes for CQRS integration tests
    public class CqrsTestCreateUserCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CqrsTestCreateUserCommandHandler : ICommandHandler<CqrsTestCreateUserCommand>
    {
        public Task HandleAsync(CqrsTestCreateUserCommand request, CancellationToken cancellationToken = default)
        {
            // Simulate user creation
            return Task.CompletedTask;
        }
    }

    public class CqrsTestGetUserQuery : IQuery<CqrsTestUserDto>
    {
        public string UserEmail { get; set; } = string.Empty;
    }

    public class CqrsTestUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CqrsTestGetUserQueryHandler : IQueryHandler<CqrsTestGetUserQuery, CqrsTestUserDto>
    {
        public Task<CqrsTestUserDto> HandleAsync(CqrsTestGetUserQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CqrsTestUserDto
            {
                Id = 1,
                Name = "John Doe",
                Email = request.UserEmail
            });
        }
    }

    public class CqrsTestUserCreatedNotification : INotification
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class CqrsTestUserCreatedNotificationHandler : INotificationHandler<CqrsTestUserCreatedNotification>
    {
        public Task HandleAsync(CqrsTestUserCreatedNotification notification, CancellationToken cancellationToken = default)
        {
            // Simulate notification handling (e.g., send welcome email)
            return Task.CompletedTask;
        }
    }

    public class CqrsTestValidatedCommand : ICommand
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CqrsTestValidatedCommandHandler : ICommandHandler<CqrsTestValidatedCommand>
    {
        public Task HandleAsync(CqrsTestValidatedCommand request, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class CqrsTestValidatedCommandValidator : IValidator<CqrsTestValidatedCommand>
    {
        public Task<ValidationResult> ValidateAsync(CqrsTestValidatedCommand request, CancellationToken cancellationToken = default)
        {
            var errors = new List<ValidationError>();

            if (request.Amount <= 0)
                errors.Add(new ValidationError("Amount must be positive", nameof(request.Amount)));

            if (string.IsNullOrWhiteSpace(request.Description))
                errors.Add(new ValidationError("Description is required", nameof(request.Description)));

            return Task.FromResult(errors.Count == 0 
                ? ValidationResult.Success() 
                : ValidationResult.Failure(errors.ToArray()));
        }
    }

    public class CqrsTestComplexCommand : ICommand<int>
    {
        public string Title { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public class CqrsTestComplexCommandHandler : ICommandHandler<CqrsTestComplexCommand, int>
    {
        private static int _nextId = 1;
        
        public Task<int> HandleAsync(CqrsTestComplexCommand request, CancellationToken cancellationToken = default)
        {
            // Simulate complex business logic and persistence
            return Task.FromResult(_nextId++);
        }
    }

    public class CqrsTestComplexQuery : IQuery<CqrsTestComplexReadModel>
    {
        public int ProductId { get; set; }
    }

    public class CqrsTestComplexReadModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string FormattedPrice { get; set; } = string.Empty;
        public string TagsDisplay { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    public class CqrsTestComplexQueryHandler : IQueryHandler<CqrsTestComplexQuery, CqrsTestComplexReadModel>
    {
        public Task<CqrsTestComplexReadModel> HandleAsync(CqrsTestComplexQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CqrsTestComplexReadModel
            {
                DisplayName = "Test Product",
                FormattedPrice = "$99.99",
                TagsDisplay = "electronics, gadget",
                IsAvailable = true
            });
        }
    }

    public class CqrsTestPerformanceCommand : ICommand
    {
        public int DataSize { get; set; }
    }

    public class CqrsTestPerformanceCommandHandler : ICommandHandler<CqrsTestPerformanceCommand>
    {
        public Task HandleAsync(CqrsTestPerformanceCommand request, CancellationToken cancellationToken = default)
        {
            // Simulate some processing
            return Task.CompletedTask;
        }
    }

    public class CqrsTestPerformanceQuery : IQuery<List<string>>
    {
        public int ItemCount { get; set; }
    }

    public class CqrsTestPerformanceQueryHandler : IQueryHandler<CqrsTestPerformanceQuery, List<string>>
    {
        public Task<List<string>> HandleAsync(CqrsTestPerformanceQuery request, CancellationToken cancellationToken = default)
        {
            var result = new List<string>();
            for (int i = 0; i < request.ItemCount; i++)
            {
                result.Add($"Item {i}");
            }
            return Task.FromResult(result);
        }
    }
}