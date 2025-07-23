using BMAP.Core.Mediator.Exceptions;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BMAP.Core.Mediator.Integration.Tests;

/// <summary>
///     Integration tests that simulate a real e-commerce application workflow.
///     These tests use real implementations without mocks to verify the complete mediator functionality.
///     Each test creates its own isolated service provider to avoid state sharing.
/// </summary>
public class ECommerceIntegrationTests : IDisposable
{
    public void Dispose()
    {
        // Dispose of any resources if needed
        // In this case, we are using a scoped service provider so no explicit disposal is required
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Register mediator with assembly scanning
        services.AddMediatorFromAssemblyContaining<ECommerceIntegrationTests>();

        // Register real services as singletons so all handlers and tests use the same instances
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IEmailService, InMemoryEmailService>();
        services.AddSingleton<IInventoryService, InMemoryInventoryService>();
        services.AddSingleton<IAuditService, InMemoryAuditService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task CompleteOrderWorkflow_Should_ExecuteAllStepsCorrectly()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var inventoryService = serviceProvider.GetRequiredService<IInventoryService>() as InMemoryInventoryService;
        var auditService = serviceProvider.GetRequiredService<IAuditService>() as InMemoryAuditService;

        // Create a product first
        var createProductCommand = new CreateProductCommand
        {
            Name = "EC Gaming Laptop",
            Price = 1299.99m,
            Stock = 10
        };

        var productId = await mediator.SendAsync<int>(createProductCommand);

        // Act - Place an order
        var placeOrderCommand = new PlaceOrderCommand
        {
            CustomerEmail = "eccustomer@example.com",
            CustomerName = "EC John Doe",
            ProductId = productId,
            Quantity = 2
        };

        var orderId = await mediator.SendAsync<int>(placeOrderCommand);

        // Assert
        Assert.True(orderId > 0);

        // Verify order was created
        var order = await orderRepository.GetByIdAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal("eccustomer@example.com", order.CustomerEmail);
        Assert.Equal(productId, order.ProductId);
        Assert.Equal(2, order.Quantity);
        Assert.Equal(2599.98m, order.TotalAmount);

        // Verify inventory was reduced
        var product = await productRepository.GetByIdAsync(productId);
        Assert.Equal(8, product!.Stock);

        // Verify notifications were sent (product creation + order placement)
        Assert.Equal(2, emailService!.SentEmails.Count);
        Assert.Contains(emailService.SentEmails,
            e => e.To == "eccustomer@example.com" && e.Subject.Contains("Order Confirmation"));
        Assert.Contains(emailService.SentEmails,
            e => e.To == "admin@company.com" && e.Subject.Contains("New Product Available"));

        // Verify audit trail
        Assert.True(auditService!.AuditEntries.Count > 0);
        Assert.Contains(auditService.AuditEntries, e => e.Contains("Order") && e.Contains("placed"));
        Assert.Contains(auditService.AuditEntries, e => e.Contains("Product") && e.Contains("created"));

        // Verify inventory service was called
        Assert.Single(inventoryService!.ReservationHistory);
        Assert.Equal(productId, inventoryService.ReservationHistory[0].ProductId);
        Assert.Equal(2, inventoryService.ReservationHistory[0].Quantity);

        Assert.Single(inventoryService.SaleHistory);
        Assert.Equal(productId, inventoryService.SaleHistory[0].ProductId);
        Assert.Equal(2, inventoryService.SaleHistory[0].Quantity);
    }

    [Fact]
    public async Task QueryWorkflow_Should_ReturnCorrectData()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Create some test data with unique names
        var createProduct1 = new CreateProductCommand { Name = "EC Query Laptop", Price = 999.99m, Stock = 5 };

        var productId1 = await mediator.SendAsync<int>(createProduct1);

        // Act - Query products
        var searchQuery = new SearchProductsQuery { NameFilter = "EC Query Lap" };
        var searchResults = await mediator.SendAsync<ProductSearchResult>(searchQuery);

        // Assert
        Assert.NotNull(searchResults);
        Assert.Single(searchResults.Products);
        Assert.Equal("EC Query Laptop", searchResults.Products[0].Name);
        Assert.Equal(1, searchResults.TotalCount);

        // Act - Get specific product
        var getProductQuery = new GetProductQuery { ProductId = productId1 };
        var product = await mediator.SendAsync<ProductDto>(getProductQuery);

        // Assert
        Assert.NotNull(product);
        Assert.Equal(productId1, product.Id);
        Assert.Equal("EC Query Laptop", product.Name);
        Assert.Equal(999.99m, product.Price);
    }

    [Fact]
    public async Task NotificationWorkflow_Should_TriggerAllHandlers()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var auditService = serviceProvider.GetRequiredService<IAuditService>() as InMemoryAuditService;

        // Act - Publish a notification directly
        var notification = new ProductCreatedNotification
        {
            ProductId = 123,
            ProductName = "EC Direct Test Product",
            Price = 99.99m,
            CreatedAt = DateTime.UtcNow
        };

        await mediator.PublishAndWaitAsync(notification);

        // Assert - All notification handlers should have been executed
        Assert.Single(emailService!.SentEmails);
        Assert.Contains("New Product Available", emailService.SentEmails[0].Subject);
        Assert.Contains("EC Direct Test Product", emailService.SentEmails[0].Body);

        Assert.Single(auditService!.AuditEntries);
        Assert.Contains("Product", auditService.AuditEntries[0]);
        Assert.Contains("created", auditService.AuditEntries[0]);
        Assert.Contains("EC Direct Test Product", auditService.AuditEntries[0]);
    }

    [Fact]
    public async Task ErrorHandling_Should_PropagateExceptionsCorrectly()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Test invalid product creation
        var invalidCommand = new CreateProductCommand
        {
            Name = "", // Invalid: empty name
            Price = -10, // Invalid: negative price
            Stock = 5
        };

        var exception = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(invalidCommand));
        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("Product name cannot be empty", exception.InnerException!.Message);
    }

    [Fact]
    public async Task ConcurrentOperations_Should_HandleCorrectly()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var createProductCommand = new CreateProductCommand
        {
            Name = "EC Concurrent Edition Item",
            Price = 199.99m,
            Stock = 5
        };

        var productId = await mediator.SendAsync<int>(createProductCommand);

        // Act - Multiple concurrent orders
        var orderTasks = Enumerable.Range(1, 3).Select(i =>
            mediator.SendAsync<int>(new PlaceOrderCommand
            {
                CustomerEmail = $"ecconcurrent{i}@example.com",
                CustomerName = $"EC Concurrent Customer {i}",
                ProductId = productId,
                Quantity = 1
            })).ToArray();

        var orderIds = await Task.WhenAll(orderTasks);

        // Assert
        Assert.Equal(3, orderIds.Length);
        Assert.All(orderIds, id => Assert.True(id > 0));

        // Verify final stock level
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var finalProduct = await productRepository.GetByIdAsync(productId);
        Assert.Equal(2, finalProduct!.Stock); // 5 - 3 = 2
    }

    [Fact]
    public async Task ValidationWorkflow_Should_PreventInvalidOperations()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var createProductCommand = new CreateProductCommand
        {
            Name = "EC Validation Test Product",
            Price = 50.00m,
            Stock = 2
        };

        var productId = await mediator.SendAsync<int>(createProductCommand);

        // Act & Assert - Try to order more than available stock
        var invalidOrderCommand = new PlaceOrderCommand
        {
            CustomerEmail = "ecvalidation@example.com",
            CustomerName = "EC Validation Customer",
            ProductId = productId,
            Quantity = 5 // More than available stock
        };

        var exception = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(invalidOrderCommand));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Insufficient stock available", exception.InnerException!.Message);
    }

    [Fact]
    public async Task MultipleNotificationHandlers_Should_AllExecute()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var auditService = serviceProvider.GetRequiredService<IAuditService>() as InMemoryAuditService;
        var inventoryService = serviceProvider.GetRequiredService<IInventoryService>() as InMemoryInventoryService;

        // Act - Place an order which triggers OrderPlacedNotification
        var createProductCommand = new CreateProductCommand
        {
            Name = "EC Multi Notification Test Product",
            Price = 100.00m,
            Stock = 10
        };

        var productId = await mediator.SendAsync<int>(createProductCommand);

        var placeOrderCommand = new PlaceOrderCommand
        {
            CustomerEmail = "ecmulti@example.com",
            CustomerName = "EC Multi Test Customer",
            ProductId = productId,
            Quantity = 1
        };

        await mediator.SendAsync<int>(placeOrderCommand);

        // Assert - Verify all notification handlers were executed
        // Should have 2 emails: 1 for product creation, 1 for order confirmation
        Assert.Equal(2, emailService!.SentEmails.Count);
        Assert.Contains(emailService.SentEmails,
            e => e.To == "ecmulti@example.com" && e.Subject.Contains("Order Confirmation"));

        // Should have 2 audit entries: 1 for product creation, 1 for order placement
        Assert.Equal(2, auditService!.AuditEntries.Count);
        Assert.Contains(auditService.AuditEntries, e => e.Contains("Product") && e.Contains("created"));
        Assert.Contains(auditService.AuditEntries, e => e.Contains("Order") && e.Contains("placed"));

        // Should have 1 sale tracking entry from the inventory update handler
        Assert.Single(inventoryService!.SaleHistory);
        Assert.Equal(productId, inventoryService.SaleHistory[0].ProductId);
        Assert.Equal(1, inventoryService.SaleHistory[0].Quantity);
    }
}

// Domain Models (keeping them simple here for clarity)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class ProductSearchResult
{
    public List<ProductDto> Products { get; set; } = [];
    public int TotalCount { get; set; }
}

public class Email
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class InventoryReservation
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; }
}