using BMAP.Core.Mediator.Exceptions;
using BMAP.Core.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Integration.Tests;

/// <summary>
///     Complex integration tests that simulate real-world scenarios with multiple operations and edge cases.
///     Each test creates its own isolated service provider to avoid state sharing.
/// </summary>
public class ComplexScenariosIntegrationTests
{
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Add logging services (required for mediator logging dependencies)
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Register mediator with assembly scanning
        services.AddMediatorFromAssemblyContaining<ComplexScenariosIntegrationTests>();

        // Register real services as singletons so all handlers and tests use the same instances
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddSingleton<IEmailService, InMemoryEmailService>();
        services.AddSingleton<IInventoryService, InMemoryInventoryService>();
        services.AddSingleton<IAuditService, InMemoryAuditService>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task HighVolumeOperations_Should_ProcessCorrectly()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;

        // Create multiple products
        var productCreationTasks = Enumerable.Range(1, 10).Select(i =>
            mediator.SendAsync<int>(new CreateProductCommand
            {
                Name = $"HVProduct {i}",
                Price = 10.00m * i,
                Stock = 100
            })).ToArray();

        var productIds = await Task.WhenAll(productCreationTasks);

        // Act - Create many orders concurrently
        var orderTasks = new List<Task<int>>();
        for (var i = 0; i < 50; i++)
        {
            var productId = productIds[i % productIds.Length];
            orderTasks.Add(mediator.SendAsync<int>(new PlaceOrderCommand
            {
                CustomerEmail = $"hvcustomer{i}@example.com",
                CustomerName = $"HV Customer {i}",
                ProductId = productId,
                Quantity = 1
            }));
        }

        var orderIds = await Task.WhenAll(orderTasks);

        // Assert
        Assert.Equal(50, orderIds.Length);
        Assert.All(orderIds, id => Assert.True(id > 0));

        // Verify all products had their stock reduced
        foreach (var productId in productIds)
        {
            var product = await productRepository.GetByIdAsync(productId);
            Assert.True(product!.Stock < 100); // Some stock should have been consumed
        }

        // Verify a reasonable number of emails were sent (products + orders)
        Assert.Equal(60, emailService!.SentEmails.Count); // 10 product notifications + 50 order confirmations
    }

    [Fact]
    public async Task BusinessWorkflow_InventoryManagement_Should_WorkCorrectly()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();

        // Create a product with limited stock
        var productId = await mediator.SendAsync<int>(new CreateProductCommand
        {
            Name = "Limited Stock Item",
            Price = 299.99m,
            Stock = 3
        });

        // Act & Assert - Sequential orders should work until stock runs out

        // First order - should succeed
        var order1Id = await mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "customer1@example.com",
            CustomerName = "Customer 1",
            ProductId = productId,
            Quantity = 2
        });

        Assert.True(order1Id > 0);
        var product = await productRepository.GetByIdAsync(productId);
        Assert.Equal(1, product!.Stock);

        // Second order - should succeed for remaining stock
        var order2Id = await mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "customer2@example.com",
            CustomerName = "Customer 2",
            ProductId = productId,
            Quantity = 1
        });

        Assert.True(order2Id > 0);
        product = await productRepository.GetByIdAsync(productId);
        Assert.Equal(0, product!.Stock);

        // Third order - should fail due to insufficient stock
        var exception = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "customer3@example.com",
            CustomerName = "Customer 3",
            ProductId = productId,
            Quantity = 1
        }));

        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Insufficient stock available", exception.InnerException!.Message);
    }

    [Fact]
    public async Task ErrorScenarios_Should_HandleGracefully()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Test 1: Invalid product ID in order
        var exception1 = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "test@example.com",
            CustomerName = "Test",
            ProductId = 99999, // Non-existent product
            Quantity = 1
        }));

        Assert.IsType<ArgumentException>(exception1.InnerException);
        Assert.Contains("Product not found", exception1.InnerException!.Message);

        // Test 2: Invalid customer data
        var productId = await mediator.SendAsync<int>(new CreateProductCommand
        {
            Name = "Error Test Product",
            Price = 50.00m,
            Stock = 10
        });

        var exception2 = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "", // Empty email
            CustomerName = "Test",
            ProductId = productId,
            Quantity = 1
        }));

        Assert.IsType<ArgumentException>(exception2.InnerException);
        Assert.Contains("Customer email cannot be empty", exception2.InnerException!.Message);

        // Test 3: Zero or negative quantity
        var exception3 = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(new PlaceOrderCommand
        {
            CustomerEmail = "test@example.com",
            CustomerName = "Test",
            ProductId = productId,
            Quantity = 0 // Invalid quantity
        }));

        Assert.IsType<ArgumentException>(exception3.InnerException);
        Assert.Contains("Quantity must be positive", exception3.InnerException!.Message);
    }

    [Fact]
    public async Task NotificationReliability_Should_DeliverAllMessages()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>() as InMemoryEmailService;
        var auditService = serviceProvider.GetRequiredService<IAuditService>() as InMemoryAuditService;

        // Act - Create multiple products and orders to generate many notifications
        var productIds = new List<int>();
        for (var i = 1; i <= 5; i++)
        {
            var productId = await mediator.SendAsync<int>(new CreateProductCommand
            {
                Name = $"Notification Test Product {i}",
                Price = 50.00m * i,
                Stock = 20
            });
            productIds.Add(productId);
        }

        var orderIds = new List<int>();
        if (orderIds == null) throw new ArgumentNullException(nameof(orderIds));
        for (var i = 1; i <= 5; i++)
        {
            var orderId = await mediator.SendAsync<int>(new PlaceOrderCommand
            {
                CustomerEmail = $"ncustomer{i}@example.com",
                CustomerName = $"N Customer {i}",
                ProductId = productIds[i - 1],
                Quantity = 2
            });
            orderIds.Add(orderId);
        }

        // Assert - Verify all notifications were delivered
        // Should have: 5 product creation emails + 5 order confirmation emails = 10 total
        Assert.Equal(10, emailService!.SentEmails.Count);

        // Verify product creation notifications
        var productEmails = emailService.SentEmails.Where(e => e.Subject.Contains("New Product Available")).ToList();
        Assert.Equal(5, productEmails.Count);

        // Verify order confirmation notifications
        var orderEmails = emailService.SentEmails.Where(e => e.Subject.Contains("Order Confirmation")).ToList();
        Assert.Equal(5, orderEmails.Count);

        // Should have: 5 product creation audits + 5 order placement audits = 10 total
        Assert.Equal(10, auditService!.AuditEntries.Count);

        // Verify audit entries
        var productAudits = auditService.AuditEntries.Where(e => e.Contains("Product") && e.Contains("created"))
            .ToList();
        Assert.Equal(5, productAudits.Count);

        var orderAudits = auditService.AuditEntries.Where(e => e.Contains("Order") && e.Contains("placed")).ToList();
        Assert.Equal(5, orderAudits.Count);
    }

    [Fact]
    public async Task QueryPerformance_Should_HandleLargeDatasets()
    {
        // Arrange - Create a fresh service provider for this test
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Create many products with unique names for this test
        var productNames = new[]
            { "QPLaptop", "QPDesktop", "QPTablet", "QPPhone", "QPWatch", "QPCamera", "QPSpeaker", "QPHeadphones" };

        for (var i = 0; i < 100; i++)
            await mediator.SendAsync<int>(new CreateProductCommand
            {
                Name = $"{productNames[i % productNames.Length]} Model {i}",
                Price = 100.00m + i * 10,
                Stock = 50
            });

        // Act - Perform various search queries
        var laptopQuery = new SearchProductsQuery { NameFilter = "QPLaptop" };
        var laptopResults = await mediator.SendAsync<ProductSearchResult>(laptopQuery);

        var phoneQuery = new SearchProductsQuery { NameFilter = "QPPhone" };
        var phoneResults = await mediator.SendAsync<ProductSearchResult>(phoneQuery);

        var allQuery = new SearchProductsQuery { NameFilter = "" };
        var allResults = await mediator.SendAsync<ProductSearchResult>(allQuery);

        // Assert
        // 100 products / 8 names = 12.5, so some categories will have 12, some 13
        Assert.InRange(laptopResults.Products.Count, 12, 13);
        Assert.Equal(laptopResults.Products.Count, laptopResults.TotalCount);
        Assert.All(laptopResults.Products, p => Assert.Contains("QPLaptop", p.Name));

        Assert.InRange(phoneResults.Products.Count, 12, 13);
        Assert.All(phoneResults.Products, p => Assert.Contains("QPPhone", p.Name));

        Assert.Equal(100, allResults.Products.Count);
        Assert.Equal(100, allResults.TotalCount);
    }

    [Fact]
    public async Task DataConsistency_Should_MaintainIntegrity()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var productRepository = serviceProvider.GetRequiredService<IProductRepository>();
        var orderRepository = serviceProvider.GetRequiredService<IOrderRepository>();
        var inventoryService = serviceProvider.GetRequiredService<IInventoryService>() as InMemoryInventoryService;

        var productId = await mediator.SendAsync<int>(new CreateProductCommand
        {
            Name = "Consistency Test Product",
            Price = 75.00m,
            Stock = 10
        });

        // Act - Create multiple orders
        var orderTasks = Enumerable.Range(1, 5).Select(i =>
            mediator.SendAsync<int>(new PlaceOrderCommand
            {
                CustomerEmail = $"consistency{i}@example.com",
                CustomerName = $"Consistency Test {i}",
                ProductId = productId,
                Quantity = 1
            })).ToArray();

        var orderIds = await Task.WhenAll(orderTasks);

        // Assert - Verify data consistency
        var finalProduct = await productRepository.GetByIdAsync(productId);
        Assert.Equal(5, finalProduct!.Stock); // 10 - 5 = 5

        // Verify all orders were created
        var allOrders = new List<Order>();
        foreach (var orderId in orderIds)
        {
            var order = await orderRepository.GetByIdAsync(orderId);
            Assert.NotNull(order);
            allOrders.Add(order);
        }

        Assert.Equal(5, allOrders.Count);
        Assert.All(allOrders, o => Assert.Equal(productId, o.ProductId));
        Assert.All(allOrders, o => Assert.Equal(1, o.Quantity));

        // Verify inventory tracking
        Assert.Equal(5, inventoryService!.ReservationHistory.Count);
        Assert.Equal(5, inventoryService.SaleHistory.Count);

        var totalReserved = inventoryService.ReservationHistory.Sum(r => r.Quantity);
        var totalSold = inventoryService.SaleHistory.Sum(s => s.Quantity);

        Assert.Equal(5, totalReserved);
        Assert.Equal(5, totalSold);
    }

    [Fact]
    public async Task ProductCreation_Validation_Should_PreventInvalidData()
    {
        // Arrange
        await using var serviceProvider = CreateServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Test empty product name
        var exception1 = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(
            new CreateProductCommand
            {
                Name = "",
                Price = 100.00m,
                Stock = 10
            }));

        Assert.IsType<ArgumentException>(exception1.InnerException);
        Assert.Contains("Product name cannot be empty", exception1.InnerException!.Message);

        // Test negative price
        var exception2 = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(
            new CreateProductCommand
            {
                Name = "Validation Test Product",
                Price = -10.00m,
                Stock = 10
            }));

        Assert.IsType<ArgumentException>(exception2.InnerException);
        Assert.Contains("Product price must be positive", exception2.InnerException!.Message);

        // Test negative stock
        var exception3 = await Assert.ThrowsAsync<MediatorException>(() => mediator.SendAsync<int>(
            new CreateProductCommand
            {
                Name = "Validation Test Product 2",
                Price = 100.00m,
                Stock = -5
            }));

        Assert.IsType<ArgumentException>(exception3.InnerException);
        Assert.Contains("Product stock cannot be negative", exception3.InnerException!.Message);
    }
}