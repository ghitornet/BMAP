namespace BMAP.Core.Mediator.Integration.Tests;

// Service Interfaces
public interface IProductRepository
{
    Task<int> CreateAsync(Product product);
    Task<Product?> GetByIdAsync(int id);
    Task UpdateAsync(Product product);
    Task<List<Product>> SearchAsync(string nameFilter);
}

public interface IOrderRepository
{
    Task<int> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
}

public interface IEmailService
{
    Task SendAsync(Email email);
}

public interface IInventoryService
{
    Task ReserveAsync(int productId, int quantity);
    Task TrackSaleAsync(int productId, int quantity);
}

public interface IAuditService
{
    Task LogAsync(string message);
}

// In-Memory Implementations
public class InMemoryProductRepository : IProductRepository
{
    private readonly Lock _lock = new();
    private readonly Dictionary<int, Product> _products = [];
    private int _nextId = 1;

    public Task<int> CreateAsync(Product product)
    {
        lock (_lock)
        {
            product.Id = _nextId++;
            _products[product.Id] = product;
            return Task.FromResult(product.Id);
        }
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }
    }

    public Task UpdateAsync(Product product)
    {
        lock (_lock)
        {
            if (_products.ContainsKey(product.Id)) _products[product.Id] = product;
            return Task.CompletedTask;
        }
    }

    public Task<List<Product>> SearchAsync(string nameFilter)
    {
        lock (_lock)
        {
            var results = _products.Values
                .Where(p => string.IsNullOrEmpty(nameFilter) ||
                            p.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(results);
        }
    }
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Lock _lock = new();
    private readonly Dictionary<int, Order> _orders = [];
    private int _nextId = 1;

    public Task<int> CreateAsync(Order order)
    {
        lock (_lock)
        {
            order.Id = _nextId++;
            _orders[order.Id] = order;
            return Task.FromResult(order.Id);
        }
    }

    public Task<Order?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            _orders.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }
    }

    public Task<List<Order>> GetByCustomerEmailAsync(string email)
    {
        lock (_lock)
        {
            var results = _orders.Values
                .Where(o => o.CustomerEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult(results);
        }
    }
}

public class InMemoryEmailService : IEmailService
{
    private readonly Lock _lock = new();
    public List<Email> SentEmails { get; } = [];

    public Task SendAsync(Email email)
    {
        lock (_lock)
        {
            SentEmails.Add(email);
        }

        return Task.CompletedTask;
    }
}

public class InMemoryInventoryService : IInventoryService
{
    private readonly Lock _lock = new();
    public List<InventoryReservation> ReservationHistory { get; } = [];
    public List<SaleRecord> SaleHistory { get; } = [];

    public Task ReserveAsync(int productId, int quantity)
    {
        lock (_lock)
        {
            ReservationHistory.Add(new InventoryReservation
            {
                ProductId = productId,
                Quantity = quantity,
                ReservedAt = DateTime.UtcNow
            });
        }

        return Task.CompletedTask;
    }

    public Task TrackSaleAsync(int productId, int quantity)
    {
        lock (_lock)
        {
            SaleHistory.Add(new SaleRecord
            {
                ProductId = productId,
                Quantity = quantity
            });
        }

        return Task.CompletedTask;
    }
}

public class InMemoryAuditService : IAuditService
{
    private readonly Lock _lock = new();
    public List<string> AuditEntries { get; } = [];

    public Task LogAsync(string message)
    {
        lock (_lock)
        {
            AuditEntries.Add($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        return Task.CompletedTask;
    }
}

// Additional models
public class SaleRecord
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}