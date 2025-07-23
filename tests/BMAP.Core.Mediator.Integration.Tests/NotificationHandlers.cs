namespace BMAP.Core.Mediator.Integration.Tests;

// Notification Handlers
public class ProductCreatedEmailNotificationHandler(IEmailService emailService)
    : INotificationHandler<ProductCreatedNotification>
{
    public async Task HandleAsync(ProductCreatedNotification notification,
        CancellationToken cancellationToken = default)
    {
        await emailService.SendAsync(new Email
        {
            To = "admin@company.com",
            Subject = "New Product Available",
            Body = $"A new product '{notification.ProductName}' has been created with price ${notification.Price:F2}",
            SentAt = DateTime.UtcNow
        });
    }
}

public class ProductCreatedAuditHandler(IAuditService auditService) : INotificationHandler<ProductCreatedNotification>
{
    public async Task HandleAsync(ProductCreatedNotification notification,
        CancellationToken cancellationToken = default)
    {
        await auditService.LogAsync(
            $"Product '{notification.ProductName}' created with ID {notification.ProductId} at {notification.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    }
}

public class OrderPlacedEmailNotificationHandler(IEmailService emailService)
    : INotificationHandler<OrderPlacedNotification>
{
    public async Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken = default)
    {
        await emailService.SendAsync(new Email
        {
            To = notification.CustomerEmail,
            Subject = $"Order Confirmation #{notification.OrderId}",
            Body = $@"Dear {notification.CustomerName},

Thank you for your order!

Order Details:
- Product: {notification.ProductName}
- Quantity: {notification.Quantity}
- Total Amount: ${notification.TotalAmount:F2}
- Order Date: {notification.OrderDate:yyyy-MM-dd HH:mm:ss}

Your order will be processed soon.

Best regards,
The E-Commerce Team",
            SentAt = DateTime.UtcNow
        });
    }
}

public class OrderPlacedAuditHandler(IAuditService auditService) : INotificationHandler<OrderPlacedNotification>
{
    public async Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken = default)
    {
        await auditService.LogAsync(
            $"Order {notification.OrderId} placed by {notification.CustomerName} ({notification.CustomerEmail}) for product {notification.ProductId} - Quantity: {notification.Quantity}, Amount: ${notification.TotalAmount:F2}");
    }
}

public class OrderPlacedInventoryUpdateHandler(IInventoryService inventoryService)
    : INotificationHandler<OrderPlacedNotification>
{
    public async Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken = default)
    {
        // Additional inventory tracking after order placement
        await inventoryService.TrackSaleAsync(notification.ProductId, notification.Quantity);
    }
}