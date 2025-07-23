using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for CQRS query handling functionality.
/// </summary>
public class CqrsQueryTests
{
    [Fact]
    public async Task Query_Should_Execute_And_Return_Result()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediator();
        
        // Use the convenient extension method that registers both interfaces automatically
        services.AddQueryHandler<TestGetUserQuery, TestUserDto, TestGetUserQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var query = new TestGetUserQuery { UserId = 123 };
        
        // Act
        var result = await mediator.SendAsync<TestUserDto>(query);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("User 123", result.Name);
        Assert.True(TestGetUserQueryHandler.WasExecuted);
    }
    
    [Fact]
    public async Task Multiple_Queries_Should_Execute_Independently()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediator();
        
        // Use the convenient extension methods that register both interfaces automatically
        services.AddQueryHandler<TestGetUserQuery, TestUserDto, TestGetUserQueryHandler>();
        services.AddQueryHandler<TestGetProductQuery, TestProductDto, TestGetProductQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var userQuery = new TestGetUserQuery { UserId = 456 };
        var productQuery = new TestGetProductQuery { ProductId = 789 };
        
        // Act
        var userResult = await mediator.SendAsync<TestUserDto>(userQuery);
        var productResult = await mediator.SendAsync<TestProductDto>(productQuery);
        
        // Assert
        Assert.NotNull(userResult);
        Assert.Equal(456, userResult.Id);
        Assert.NotNull(productResult);
        Assert.Equal(789, productResult.Id);
        Assert.True(TestGetUserQueryHandler.WasExecuted);
        Assert.True(TestGetProductQueryHandler.WasExecuted);
    }
    
    [Fact]
    public async Task Query_Should_Be_Read_Only_And_Cacheable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        services.AddMediator();
        
        // Use the convenient extension method that registers both interfaces automatically
        services.AddQueryHandler<TestGetUserQuery, TestUserDto, TestGetUserQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var query = new TestGetUserQuery { UserId = 999 };
        
        // Act - Execute same query multiple times
        var result1 = await mediator.SendAsync<TestUserDto>(query);
        var result2 = await mediator.SendAsync<TestUserDto>(query);
        
        // Assert - Results should be consistent (demonstrating read-only nature)
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1.Id, result2.Id);
        Assert.Equal(result1.Name, result2.Name);
        Assert.Equal(result1.Email, result2.Email);
    }

    // Test query classes
    public class TestGetUserQuery : IQuery<TestUserDto>
    {
        public int UserId { get; set; }
    }

    public class TestUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TestGetUserQueryHandler : IQueryHandler<TestGetUserQuery, TestUserDto>
    {
        public static bool WasExecuted { get; private set; }

        public Task<TestUserDto> HandleAsync(TestGetUserQuery request, CancellationToken cancellationToken = default)
        {
            WasExecuted = true;
            return Task.FromResult(new TestUserDto
            {
                Id = request.UserId,
                Name = $"User {request.UserId}",
                Email = $"user{request.UserId}@example.com"
            });
        }
    }

    public class TestGetProductQuery : IQuery<TestProductDto>
    {
        public int ProductId { get; set; }
    }

    public class TestProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class TestGetProductQueryHandler : IQueryHandler<TestGetProductQuery, TestProductDto>
    {
        public static bool WasExecuted { get; private set; }

        public Task<TestProductDto> HandleAsync(TestGetProductQuery request, CancellationToken cancellationToken = default)
        {
            WasExecuted = true;
            return Task.FromResult(new TestProductDto
            {
                Id = request.ProductId,
                Name = $"Product {request.ProductId}",
                Price = request.ProductId * 10.0m
            });
        }
    }
}