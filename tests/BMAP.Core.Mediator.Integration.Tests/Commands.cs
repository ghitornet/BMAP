namespace BMAP.Core.Mediator.Integration.Tests;

// Commands
public class CreateProductCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
}

public class PlaceOrderCommand : IRequest<int>
{
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

// Queries
public class SearchProductsQuery : IRequest<ProductSearchResult>
{
    public string NameFilter { get; init; } = string.Empty;
}

public class GetProductQuery : IRequest<ProductDto>
{
    public int ProductId { get; init; }
}

// Notifications
public class ProductCreatedNotification : INotification
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class OrderPlacedNotification : INotification
{
    public int OrderId { get; init; }
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime OrderDate { get; init; }
}