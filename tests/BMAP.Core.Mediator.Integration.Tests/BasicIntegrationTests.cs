using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Integration.Tests;

/// <summary>
///     Basic integration tests that verify the mediator works end-to-end with real implementations.
///     These tests are designed to be reliable and not depend on complex timing or state management.
///     Each test creates its own isolated service provider to avoid state sharing.
/// </summary>
public class BasicIntegrationTests
{
    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Register mediator with assembly scanning
        services.AddMediatorFromAssemblyContaining<BasicIntegrationTests>();

        // Register real services as singletons so all handlers and tests use the same instances
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IEmailService, InMemoryEmailService>();
        services.AddSingleton<IInventoryService, InMemoryInventoryService>();
        services.AddSingleton<IAuditService, InMemoryAuditService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreateProduct_Should_WorkEndToEnd()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var auditService = serviceProvider.GetRequiredService<IAuditService>() as InMemoryAuditService;

        var command = new CreateProductCommand
        {
            Name = "Basic Test Product",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var productId = await mediator.SendAsync<int>(command);

        // Assert
        Assert.True(productId > 0);

        // Verify product was created
        var product = await productRepository.GetByIdAsync(productId);
        Assert.NotNull(product);
        Assert.Equal("Basic Test Product", product.Name);
        Assert.Equal(99.99m, product.Price);
        Assert.Equal(10, product.Stock);

        // Verify notification was sent
        Assert.Single(emailService!.SentEmails);
        Assert.Contains("New Product Available", emailService.SentEmails[0].Subject);

        // Verify audit entry was created
        Assert.Single(auditService!.AuditEntries);
        Assert.Contains("Product 'Basic Test Product' created", auditService.AuditEntries[0]);
    }

    [Fact]
    public async Task PlaceOrder_Should_WorkEndToEnd()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var inventoryService = serviceProvider.GetRequiredService<IInventoryService>() as InMemoryInventoryService;

        // Create a product first
        var productId = await mediator.SendAsync<int>(new CreateProductCommand
        {
            Name = "Order Test Product",
            Price = 50.00m,
            Stock = 5
        });

        // Clear product creation email to focus on order email
        emailService!.SentEmails.Clear();
        inventoryService!.ReservationHistory.Clear();
        inventoryService.SaleHistory.Clear();

        var orderCommand = new PlaceOrderCommand
        {
            CustomerEmail = "basictest@example.com",
            CustomerName = "Basic Test Customer",
            ProductId = productId,
            Quantity = 2
        };

        // Act
        var orderId = await mediator.SendAsync<int>(orderCommand);

        // Assert
        Assert.True(orderId > 0);

        // Verify order was created
        var order = await orderRepository.GetByIdAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal("basictest@example.com", order.CustomerEmail);
        Assert.Equal(2, order.Quantity);
        Assert.Equal(100.00m, order.TotalAmount);

        // Verify stock was reduced
        var product = await productRepository.GetByIdAsync(productId);
        Assert.Equal(3, product!.Stock);

        // Verify email was sent
        Assert.Single(emailService.SentEmails);
        Assert.Equal("basictest@example.com", emailService.SentEmails[0].To);
        Assert.Contains("Order Confirmation", emailService.SentEmails[0].Subject);

        // Verify inventory operations
        Assert.Single(inventoryService.ReservationHistory);
        Assert.Single(inventoryService.SaleHistory);
    }

    [Fact]
    public async Task SearchProducts_Should_WorkEndToEnd()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync<int>(new CreateProductCommand
            { Name = "Search Gaming Laptop", Price = 1000m, Stock = 5 });
        await mediator.SendAsync<int>(new CreateProductCommand
            { Name = "Search Office Laptop", Price = 800m, Stock = 3 });
        await mediator.SendAsync<int>(
            new CreateProductCommand { Name = "Search Gaming Mouse", Price = 50m, Stock = 20 });

        var query = new SearchProductsQuery { NameFilter = "Search Gaming" };

        // Act
        var result = await mediator.SendAsync<ProductSearchResult>(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Products.Count);
        Assert.All(result.Products, p => Assert.Contains("Search Gaming", p.Name));
    }

    [Fact]
    public async Task GetProduct_Should_WorkEndToEnd()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var productId = await mediator.SendAsync<int>(new CreateProductCommand
        {
            Name = "Get Test Retrieval Product",
            Price = 199.99m,
            Stock = 15
        });

        var query = new GetProductQuery { ProductId = productId };

        // Act
        var result = await mediator.SendAsync<ProductDto>(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
        Assert.Equal("Get Test Retrieval Product", result.Name);
        Assert.Equal(199.99m, result.Price);
        Assert.Equal(15, result.Stock);
    }

    [Fact]
    public async Task PublishNotification_Should_TriggerAllHandlers()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var auditService = serviceProvider.GetRequiredService<IAuditService>() as InMemoryAuditService;

        var notification = new ProductCreatedNotification
        {
            ProductId = 999,
            ProductName = "Direct Notification Test",
            Price = 123.45m,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await mediator.PublishAndWaitAsync(notification);

        // Assert
        Assert.Single(emailService!.SentEmails);
        Assert.Contains("New Product Available", emailService.SentEmails[0].Subject);
        Assert.Contains("Direct Notification Test", emailService.SentEmails[0].Body);

        Assert.Single(auditService!.AuditEntries);
        Assert.Contains("Product 'Direct Notification Test' created", auditService.AuditEntries[0]);
    }

    [Fact]
    public async Task MultipleOperations_Should_MaintainDataConsistency()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();

        // Create multiple products
        var product1Id = await mediator.SendAsync<int>(new CreateProductCommand
            { Name = "Multi Product 1", Price = 10m, Stock = 100 });
        var product2Id = await mediator.SendAsync<int>(new CreateProductCommand
            { Name = "Multi Product 2", Price = 20m, Stock = 100 });

        // Act - Create multiple orders
        var order1Id = await mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "multicustomer1@test.com",
            CustomerName = "Multi Customer 1",
            ProductId = product1Id,
            Quantity = 5
        });

        var order2Id = await mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "multicustomer2@test.com",
            CustomerName = "Multi Customer 2",
            ProductId = product2Id,
            Quantity = 3
        });

        // Assert
        var product1 = await productRepository.GetByIdAsync(product1Id);
        var product2 = await productRepository.GetByIdAsync(product2Id);
        var order1 = await orderRepository.GetByIdAsync(order1Id);
        var order2 = await orderRepository.GetByIdAsync(order2Id);

        Assert.Equal(95, product1!.Stock);
        Assert.Equal(97, product2!.Stock);

        Assert.Equal(50m, order1!.TotalAmount);
        Assert.Equal(60m, order2!.TotalAmount);
    }
}