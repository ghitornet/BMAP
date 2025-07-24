using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using BMAP.Core.Data.Dapper.Extensions;
using BMAP.Core.Data.Dapper.Handlers;
using BMAP.Core.Data.Entities;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;
using BMAP.Core.Mediator;
using BMAP.Core.Mediator.Extensions;
using BMAP.Core.Result;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace BMAP.Core.Data.Dapper.Tests.Integration;

/// <summary>
/// Integration tests for Dapper handlers using SQLite in-memory database.
/// These tests verify the CRUD operations with actual database operations.
/// </summary>
public class DapperHandlersIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IDbConnection _connection;
    private readonly IMediator _mediator;

    public DapperHandlersIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Create SQLite in-memory connection
        var connectionString = "Data Source=:memory:";
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        _connection = connection;

        // Register services
        services.AddSingleton<IDbConnection>(_connection);
        
        // Add mediator with CQRS support
        services.AddMediatorWithCqrs();
        
        // Register specific handlers for TestUser directly
        RegisterTestUserHandlersDirectly(services);

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Create test table
        InitializeDatabase();
    }

    private void RegisterTestUserHandlersDirectly(IServiceCollection services)
    {
        // Register the concrete handlers directly 
        services.AddTransient<CreateEntityHandler<TestUser>>();
        services.AddTransient<ICommandHandler<CreateEntityCommand<TestUser>, BMAP.Core.Result.Result<int>>, CreateEntityHandler<TestUser>>();
        services.AddTransient<IRequestHandler<CreateEntityCommand<TestUser>, BMAP.Core.Result.Result<int>>, CreateEntityHandler<TestUser>>();

        services.AddTransient<GetEntityByIdHandler<TestUser>>();
        services.AddTransient<IQueryHandler<GetEntityByIdQuery<TestUser>, BMAP.Core.Result.Result<TestUser>>, GetEntityByIdHandler<TestUser>>();
        services.AddTransient<IRequestHandler<GetEntityByIdQuery<TestUser>, BMAP.Core.Result.Result<TestUser>>, GetEntityByIdHandler<TestUser>>();

        services.AddTransient<GetAllEntitiesHandler<TestUser>>();
        services.AddTransient<IQueryHandler<GetAllEntitiesQuery<TestUser>, BMAP.Core.Result.Result<IEnumerable<TestUser>>>, GetAllEntitiesHandler<TestUser>>();
        services.AddTransient<IRequestHandler<GetAllEntitiesQuery<TestUser>, BMAP.Core.Result.Result<IEnumerable<TestUser>>>, GetAllEntitiesHandler<TestUser>>();

        services.AddTransient<GetEntitiesPagedHandler<TestUser>>();
        services.AddTransient<IQueryHandler<GetEntitiesPagedQuery<TestUser>, BMAP.Core.Result.Result<PagedResult<TestUser>>>, GetEntitiesPagedHandler<TestUser>>();
        services.AddTransient<IRequestHandler<GetEntitiesPagedQuery<TestUser>, BMAP.Core.Result.Result<PagedResult<TestUser>>>, GetEntitiesPagedHandler<TestUser>>();

        services.AddTransient<UpdateEntityHandler<TestUser>>();
        services.AddTransient<ICommandHandler<UpdateEntityCommand<TestUser>, BMAP.Core.Result.Result>, UpdateEntityHandler<TestUser>>();
        services.AddTransient<IRequestHandler<UpdateEntityCommand<TestUser>, BMAP.Core.Result.Result>, UpdateEntityHandler<TestUser>>();

        // For soft delete, create a custom handler that knows about TestUser
        services.AddTransient<SoftDeleteEntityHandler<int>>(provider =>
        {
            var connection = provider.GetRequiredService<IDbConnection>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<SoftDeleteEntityHandler<int>>();
            return new SoftDeleteEntityHandler<int>(connection, logger, typeof(TestUser));
        });

        services.AddTransient<ICommandHandler<SoftDeleteEntityCommand, BMAP.Core.Result.Result>>(provider =>
            provider.GetRequiredService<SoftDeleteEntityHandler<int>>());

        services.AddTransient<IRequestHandler<SoftDeleteEntityCommand, BMAP.Core.Result.Result>>(provider =>
            provider.GetRequiredService<SoftDeleteEntityHandler<int>>());
    }

    private void InitializeDatabase()
    {
        var sql = """
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL,
                CreatedAt DATETIME NOT NULL,
                CreatedBy TEXT,
                LastModifiedAt DATETIME,
                LastModifiedBy TEXT,
                IsDeleted INTEGER NOT NULL DEFAULT 0,
                DeletedAt DATETIME,
                DeletedBy TEXT
            )
            """;

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    [Fact]
    public async Task GetEntityByIdHandler_With_NonExistent_Id_Should_Return_NotFound()
    {
        // Arrange
        var query = new GetEntityByIdQuery<TestUser>(999);

        // Act
        var result = await _mediator.SendAsync<BMAP.Core.Result.Result<TestUser>>(query);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Contains("not found", result.Error.Message);
    }

    [Fact]
    public async Task UpdateEntityHandler_With_NonExistent_Id_Should_Return_NotFound()
    {
        // Arrange
        var user = new TestUser { Name = "Test", Email = "test@example.com" };
        var command = new UpdateEntityCommand<TestUser>(999, user);

        // Act
        var result = await _mediator.SendAsync<BMAP.Core.Result.Result>(command);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task GetEntitiesPagedHandler_Should_Return_Correct_Pagination()
    {
        // Arrange - Create multiple users
        for (int i = 1; i <= 15; i++)
        {
            var user = new TestUser
            {
                Name = $"User {i}",
                Email = $"user{i}@example.com",
                CreatedBy = "system"
            };
            var createCommand = new CreateEntityCommand<TestUser>(user);
            await _mediator.SendAsync<BMAP.Core.Result.Result<int>>(createCommand);
        }

        // Act - Get first page
        var query1 = new GetEntitiesPagedQuery<TestUser>(pageNumber: 1, pageSize: 10);
        var result1 = await _mediator.SendAsync<BMAP.Core.Result.Result<PagedResult<TestUser>>>(query1);

        // Assert - First page
        Assert.True(result1.IsSuccess);
        var page1 = result1.Value;
        Assert.Equal(10, page1.Items.Count());
        Assert.Equal(15, page1.TotalCount);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(10, page1.PageSize);
        Assert.Equal(2, page1.TotalPages);
        Assert.False(page1.HasPreviousPage);
        Assert.True(page1.HasNextPage);

        // Act - Get second page
        var query2 = new GetEntitiesPagedQuery<TestUser>(pageNumber: 2, pageSize: 10);
        var result2 = await _mediator.SendAsync<BMAP.Core.Result.Result<PagedResult<TestUser>>>(query2);

        // Assert - Second page
        Assert.True(result2.IsSuccess);
        var page2 = result2.Value;
        Assert.Equal(5, page2.Items.Count());
        Assert.Equal(15, page2.TotalCount);
        Assert.Equal(2, page2.PageNumber);
        Assert.Equal(10, page2.PageSize);
        Assert.Equal(2, page2.TotalPages);
        Assert.True(page2.HasPreviousPage);
        Assert.False(page2.HasNextPage);
    }

    [Fact]
    public async Task Isolated_Update_Test_Should_Work()
    {
        // First create a user
        var user = new TestUser
        {
            Name = "John Doe",
            Email = "john@example.com",
            CreatedBy = "system"
        };

        var createCommand = new CreateEntityCommand<TestUser>(user);
        var createResult = await _mediator.SendAsync<BMAP.Core.Result.Result<int>>(createCommand);
        Assert.True(createResult.IsSuccess);
        var userId = createResult.Value;

        // Get the user to update
        var getQuery = new GetEntityByIdQuery<TestUser>(userId);
        var getResult = await _mediator.SendAsync<BMAP.Core.Result.Result<TestUser>>(getQuery);
        Assert.True(getResult.IsSuccess);
        var retrievedUser = getResult.Value;

        // Now try to update
        retrievedUser.Name = "Jane Doe";
        retrievedUser.Email = "jane@example.com"; 
        retrievedUser.LastModifiedBy = "admin";

        var updateCommand = new UpdateEntityCommand<TestUser>(userId, retrievedUser);
        var updateResult = await _mediator.SendAsync<BMAP.Core.Result.Result>(updateCommand);
        Assert.True(updateResult.IsSuccess);
    }

    [Fact]
    public async Task Debug_Simple_Create_And_Get_Test()
    {
        // Arrange
        var user = new TestUser
        {
            Name = "Debug User",
            Email = "debug@example.com",
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert - Create
        var createCommand = new CreateEntityCommand<TestUser>(user);
        var createResult = await _mediator.SendAsync<BMAP.Core.Result.Result<int>>(createCommand);

        Assert.True(createResult.IsSuccess, $"Create failed: {createResult.Error?.Code} - {createResult.Error?.Message}");
        var userId = createResult.Value;

        // Act & Assert - Get by ID  
        var getQuery = new GetEntityByIdQuery<TestUser>(userId);
        var getResult = await _mediator.SendAsync<BMAP.Core.Result.Result<TestUser>>(getQuery);

        Assert.True(getResult.IsSuccess, $"Get failed: {getResult.Error?.Code} - {getResult.Error?.Message}");
    }

    [Fact]
    public async Task Ultra_Simple_Create_Test()
    {
        // Arrange
        var user = new TestUser
        {
            Name = "Ultra Simple User",
            Email = "ultrasimple@example.com",
            CreatedBy = "test",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var createCommand = new CreateEntityCommand<TestUser>(user);
        var createResult = await _mediator.SendAsync<BMAP.Core.Result.Result<int>>(createCommand);

        // Assert
        Assert.True(createResult.IsSuccess, $"Create failed: {createResult.Error?.Code} - {createResult.Error?.Message}");
        Assert.True(createResult.Value > 0);
    }

    [Fact]
    public async Task SoftDelete_Should_Work_Independently()
    {
        // Arrange - Create a user first
        var user = new TestUser
        {
            Name = "Test User",
            Email = "test@example.com",
            CreatedBy = "system"
        };

        var createCommand = new CreateEntityCommand<TestUser>(user);
        var createResult = await _mediator.SendAsync<BMAP.Core.Result.Result<int>>(createCommand);
        Assert.True(createResult.IsSuccess);
        var userId = createResult.Value;

        // Act - Try soft delete
        var softDeleteCommand = new SoftDeleteEntityCommand(userId, "admin");
        var softDeleteResult = await _mediator.SendAsync<BMAP.Core.Result.Result>(softDeleteCommand);
        
        // Assert
        Assert.True(softDeleteResult.IsSuccess);
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _serviceProvider?.Dispose();
    }

    #region Test Helper Classes

    [Table("Users")]
    public class TestUser : IAuditableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("CreatedBy")]
        public string? CreatedBy { get; set; }

        [Column("LastModifiedAt")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("LastModifiedBy")]
        public string? LastModifiedBy { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; }

        [Column("DeletedAt")]
        public DateTime? DeletedAt { get; set; }

        [Column("DeletedBy")]
        public string? DeletedBy { get; set; }
    }

    #endregion
}