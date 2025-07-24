using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.EntityFramework.Abstractions;
using BMAP.Core.Data.EntityFramework.Handlers;
using BMAP.Core.Data.EntityFramework.Services;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;
using BMAP.Core.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Data.EntityFramework.Tests.Integration;

/// <summary>
/// Simplified integration tests for Entity Framework Core handlers using InMemory database.
/// These tests verify individual CRUD operations work correctly with the new ContextResolver and audit system.
/// </summary>
public class SimplifiedEntityFrameworkIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;
    private readonly IContextResolver _contextResolver;
    private readonly IAuditService _auditService;

    public SimplifiedEntityFrameworkIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Reduce log noise
        });

        // Add Entity Framework with InMemory database
        services.AddDbContext<TestDbContext>(options =>
        {
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
        });

        // Register a test user context
        services.AddSingleton<IUserContext, TestUserContext>();

        // Register the audit service
        services.AddTransient<IAuditService, AuditService>();

        // Register a simple context resolver that always returns the test context
        services.AddTransient<IContextResolver, TestContextResolver>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TestDbContext>();
        _contextResolver = _serviceProvider.GetRequiredService<IContextResolver>();
        _auditService = _serviceProvider.GetRequiredService<IAuditService>();

        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateEntityHandler_Should_Create_Entity_With_Audit_Fields()
    {
        // Arrange
        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CreateEntityHandler<TestUser, int>>();
        var handler = new CreateEntityHandler<TestUser, int>(_contextResolver, _auditService, logger);

        var user = new TestUser
        {
            Name = "John Doe",
            Email = "john@example.com"
        };

        var command = new CreateEntityCommand<TestUser, int>(user);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);

        // Verify in database with audit fields
        var dbUser = await _context.Users.FindAsync(result.Value);
        Assert.NotNull(dbUser);
        Assert.Equal("John Doe", dbUser.Name);
        Assert.Equal("john@example.com", dbUser.Email);
        Assert.False(dbUser.IsDeleted);
        
        // Verify audit fields were set automatically
        Assert.Equal("test-user", dbUser.CreatedBy);
        Assert.Equal("test-user", dbUser.LastModifiedBy);
        Assert.True(dbUser.CreatedAt > DateTime.MinValue);
        Assert.True(dbUser.LastModifiedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task GetEntityByIdHandler_Should_Retrieve_Entity()
    {
        // Arrange - Create a user first
        var user = new TestUser
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            CreatedBy = "system",
            LastModifiedBy = "system",
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<GetEntityByIdHandler<TestUser, int>>();
        var handler = new GetEntityByIdHandler<TestUser, int>(_contextResolver, logger);

        var query = new GetEntityByIdQuery<TestUser, int>(user.Id);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Jane Doe", result.Value.Name);
        Assert.Equal("jane@example.com", result.Value.Email);
    }

    [Fact]
    public async Task SoftDeleteEntityHandler_Should_Soft_Delete_Entity_With_Audit_Fields()
    {
        // Arrange - Create a user first
        var user = new TestUser
        {
            Name = "To Be Deleted",
            Email = "delete@example.com",
            CreatedBy = "system",
            LastModifiedBy = "system",
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var userId = user.Id;

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<SoftDeleteEntityHandler<TestUser, int>>();
        var handler = new SoftDeleteEntityHandler<TestUser, int>(_contextResolver, _auditService, logger);

        var command = new SoftDeleteEntityCommand<int>(userId, "admin");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify soft delete worked with audit fields
        var dbUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(dbUser);
        Assert.True(dbUser.IsDeleted);
        Assert.NotNull(dbUser.DeletedAt);
        Assert.Equal("test-user", dbUser.DeletedBy); // Should use current user from audit service
        Assert.Equal("test-user", dbUser.LastModifiedBy); // Should be updated by audit service
    }

    [Fact]
    public async Task GetAllEntitiesHandler_Should_Return_All_Active_Entities()
    {
        // Arrange - Create multiple users, some deleted
        var users = new[]
        {
            new TestUser { Name = "User 1", Email = "user1@example.com", CreatedBy = "system", LastModifiedBy = "system", CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow },
            new TestUser { Name = "User 2", Email = "user2@example.com", CreatedBy = "system", LastModifiedBy = "system", CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow },
            new TestUser { Name = "User 3", Email = "user3@example.com", CreatedBy = "system", LastModifiedBy = "system", CreatedAt = DateTime.UtcNow, LastModifiedAt = DateTime.UtcNow, IsDeleted = true }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<GetAllEntitiesHandler<TestUser>>();
        var handler = new GetAllEntitiesHandler<TestUser>(_contextResolver, logger);

        var query = new GetAllEntitiesQuery<TestUser>(includeDeleted: false);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        var activeUsers = result.Value.ToList();
        Assert.Equal(2, activeUsers.Count); // Should only return non-deleted users
        Assert.All(activeUsers, u => Assert.False(u.IsDeleted));
    }

    [Fact]
    public async Task GetEntitiesPagedHandler_Should_Return_Paged_Results()
    {
        // Arrange - Create multiple users
        for (int i = 1; i <= 15; i++)
        {
            var user = new TestUser
            {
                Name = $"User {i}",
                Email = $"user{i}@example.com",
                CreatedBy = "system",
                LastModifiedBy = "system",
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
        }
        await _context.SaveChangesAsync();

        var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<GetEntitiesPagedHandler<TestUser>>();
        var handler = new GetEntitiesPagedHandler<TestUser>(_contextResolver, logger);

        var query = new GetEntitiesPagedQuery<TestUser>(pageNumber: 1, pageSize: 10);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        var page = result.Value;
        Assert.Equal(10, page.Items.Count());
        Assert.Equal(15, page.TotalCount);
        Assert.Equal(1, page.PageNumber);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(2, page.TotalPages);
        Assert.False(page.HasPreviousPage);
        Assert.True(page.HasNextPage);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }

    #region Test Helper Classes

    /// <summary>
    /// Test user context that provides consistent user information for testing.
    /// </summary>
    public class TestUserContext : IUserContext
    {
        public string? UserId => "test-user";
        public string? UserName => "Test User";
        public string? Email => "test@example.com";
        public IEnumerable<string> Roles => new[] { "TestRole" };
        public IDictionary<string, string> Properties => new Dictionary<string, string>();
        public bool IsAuthenticated => true;
        public string? TenantId => "test-tenant";
    }

    /// <summary>
    /// Simple test context resolver that always returns the TestDbContext.
    /// </summary>
    public class TestContextResolver : IContextResolver
    {
        private readonly TestDbContext _context;

        public TestContextResolver(TestDbContext context)
        {
            _context = context;
        }

        public DbContext ResolveContext<TEntity>() where TEntity : class
        {
            return _context;
        }

        public DbContext ResolveContext(Type entityType)
        {
            return _context;
        }

        public bool HasContext<TEntity>() where TEntity : class
        {
            return true;
        }

        public bool HasContext(Type entityType)
        {
            return true;
        }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure automatic soft delete filters
            modelBuilder.Entity<TestUser>().HasQueryFilter(e => !e.IsDeleted);

            base.OnModelCreating(modelBuilder);
        }
    }

    [Table("Users")]
    public class TestUser : IAuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    #endregion
}