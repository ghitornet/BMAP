namespace BMAP.Core.Mediator.Integration.Tests;

// Command Handlers
public class CreateProductCommandHandler(IProductRepository productRepository, IMediator mediator)
    : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product name cannot be empty");

        if (request.Price <= 0)
            throw new ArgumentException("Product price must be positive");

        if (request.Stock < 0)
            throw new ArgumentException("Product stock cannot be negative");

        // Create product
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        var productId = await productRepository.CreateAsync(product);

        // Publish notification
        await mediator.PublishAsync(new ProductCreatedNotification
        {
            ProductId = productId,
            ProductName = product.Name,
            Price = product.Price,
            CreatedAt = product.CreatedAt
        }, cancellationToken);

        return productId;
    }
}

public class PlaceOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IInventoryService inventoryService,
    IMediator mediator)
    : IRequestHandler<PlaceOrderCommand, int>
{
    public async Task<int> HandleAsync(PlaceOrderCommand request, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            throw new ArgumentException("Customer email cannot be empty");

        if (request.Quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        // Get product
        var product = await productRepository.GetByIdAsync(request.ProductId) ?? throw new ArgumentException("Product not found");

        // Check stock availability
        if (product.Stock < request.Quantity)
            throw new InvalidOperationException("Insufficient stock available");

        // Reserve inventory
        await inventoryService.ReserveAsync(request.ProductId, request.Quantity);

        // Reduce stock
        product.Stock -= request.Quantity;
        await productRepository.UpdateAsync(product);

        // Create order
        var order = new Order
        {
            CustomerEmail = request.CustomerEmail,
            CustomerName = request.CustomerName,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            TotalAmount = product.Price * request.Quantity,
            OrderDate = DateTime.UtcNow
        };

        var orderId = await orderRepository.CreateAsync(order);
        order.Id = orderId;

        // Publish notification
        await mediator.PublishAsync(new OrderPlacedNotification
        {
            OrderId = orderId,
            CustomerEmail = order.CustomerEmail,
            CustomerName = order.CustomerName,
            ProductId = order.ProductId,
            ProductName = product.Name,
            Quantity = order.Quantity,
            TotalAmount = order.TotalAmount,
            OrderDate = order.OrderDate
        }, cancellationToken);

        return orderId;
    }
}

// Query Handlers
public class SearchProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<SearchProductsQuery, ProductSearchResult>
{
    public async Task<ProductSearchResult> HandleAsync(SearchProductsQuery request,
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.SearchAsync(request.NameFilter);

        var list = new List<ProductDto>();
        foreach (var p in products)
            list.Add(new ProductDto { Id = p.Id, Name = p.Name, Price = p.Price, Stock = p.Stock });

        return new ProductSearchResult
        {
            Products = list,
            TotalCount = products.Count
        };
    }
}

public class GetProductQueryHandler(IProductRepository productRepository) : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> HandleAsync(GetProductQuery request, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId) ?? throw new ArgumentException("Product not found");
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock
        };
    }
}

// Helper class for getting mediator instance in handlers (kept for compatibility but no longer used)
public static class MediatorTestHelper
{
    private static IMediator? _mediator;

    public static void SetMediator(IMediator mediator)
    {
        _mediator = mediator;
    }

    public static IMediator GetMediator()
    {
        return _mediator ?? throw new InvalidOperationException("Mediator not set");
    }
}