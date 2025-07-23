using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for CQRS command handling functionality.
/// </summary>
public class CqrsCommandTests
{
    [Fact]
    public async Task Command_Without_Response_Should_Execute_Successfully()
    {
        // Arrange
        TestCreateUserCommandHandler.Reset();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediator();
        
        // Use the convenient extension method that registers both interfaces automatically
        services.AddCommandHandler<TestCreateUserCommand, TestCreateUserCommandHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestCreateUserCommand { Name = "John Doe", Email = "john@example.com" };
        
        // Act
        await mediator.SendAsync(command);
        
        // Assert
        Assert.True(TestCreateUserCommandHandler.WasExecuted);
        Assert.Equal("John Doe", TestCreateUserCommandHandler.LastCommand?.Name);
    }
    
    [Fact]
    public async Task Command_With_Response_Should_Execute_And_Return_Result()
    {
        // Arrange
        TestCreateProductCommandHandler.Reset();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediator();
        
        // Use the convenient extension method that registers both interfaces automatically
        services.AddCommandHandler<TestCreateProductCommand, int, TestCreateProductCommandHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new TestCreateProductCommand { Name = "Gaming Laptop", Price = 1299.99m };
        
        // Act
        var result = await mediator.SendAsync<int>(command);
        
        // Assert
        Assert.True(result > 0);
        Assert.True(TestCreateProductCommandHandler.WasExecuted);
        Assert.Equal("Gaming Laptop", TestCreateProductCommandHandler.LastCommand?.Name);
    }
    
    [Fact]
    public async Task Multiple_Commands_Should_Execute_Independently()
    {
        // Arrange
        TestCreateUserCommandHandler.Reset();
        TestCreateProductCommandHandler.Reset();
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediator();
        
        // Use the convenient extension methods that register both interfaces automatically
        services.AddCommandHandler<TestCreateUserCommand, TestCreateUserCommandHandler>();
        services.AddCommandHandler<TestCreateProductCommand, int, TestCreateProductCommandHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var userCommand = new TestCreateUserCommand { Name = "Jane Doe", Email = "jane@example.com" };
        var productCommand = new TestCreateProductCommand { Name = "Wireless Mouse", Price = 49.99m };
        
        // Act
        await mediator.SendAsync(userCommand);
        var productId = await mediator.SendAsync<int>(productCommand);
        
        // Assert
        Assert.True(TestCreateUserCommandHandler.WasExecuted);
        Assert.True(TestCreateProductCommandHandler.WasExecuted);
        Assert.True(productId > 0);
    }

    // Test command classes
    public class TestCreateUserCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TestCreateUserCommandHandler : ICommandHandler<TestCreateUserCommand>
    {
        public static bool WasExecuted { get; private set; }
        public static TestCreateUserCommand? LastCommand { get; private set; }

        public Task HandleAsync(TestCreateUserCommand request, CancellationToken cancellationToken = default)
        {
            WasExecuted = true;
            LastCommand = request;
            return Task.CompletedTask;
        }
        
        public static void Reset()
        {
            WasExecuted = false;
            LastCommand = null;
        }
    }

    public class TestCreateProductCommand : ICommand<int>
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class TestCreateProductCommandHandler : ICommandHandler<TestCreateProductCommand, int>
    {
        public static bool WasExecuted { get; private set; }
        public static TestCreateProductCommand? LastCommand { get; private set; }
        private static int _nextId = 1;

        public Task<int> HandleAsync(TestCreateProductCommand request, CancellationToken cancellationToken = default)
        {
            WasExecuted = true;
            LastCommand = request;
            return Task.FromResult(_nextId++);
        }
        
        public static void Reset()
        {
            WasExecuted = false;
            LastCommand = null;
            _nextId = 1;
        }
    }
}