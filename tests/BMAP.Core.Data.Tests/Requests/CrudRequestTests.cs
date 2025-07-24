using BMAP.Core.Data.Commands;
using BMAP.Core.Data.Queries;
using BMAP.Core.Data.Requests;

namespace BMAP.Core.Data.Tests.Requests;

/// <summary>
/// Unit tests for CRUD request classes to ensure proper initialization and property handling.
/// </summary>
public class CrudRequestTests
{
    #region CreateEntityCommand Tests

    [Fact]
    public void CreateEntityCommand_Should_Initialize_With_Entity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };

        // Act
        var command = new CreateEntityCommand<TestEntity>(entity);

        // Assert
        Assert.Equal(entity, command.Entity);
        Assert.Same(entity, command.Entity);
    }

    [Fact]
    public void CreateEntityCommand_With_Null_Entity_Should_Throw()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateEntityCommand<TestEntity>(null!));
    }

    [Fact]
    public void CreateEntityCommand_Generic_Should_Work()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };

        // Act
        var command = new CreateEntityCommand<TestEntity, int>(entity);

        // Assert
        Assert.Equal(entity, command.Entity);
        Assert.IsAssignableFrom<ICreateCommand<TestEntity, int>>(command);
    }

    #endregion

    #region UpdateEntityCommand Tests

    [Fact]
    public void UpdateEntityCommand_Should_Initialize_With_Id_And_Entity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        const int id = 123;

        // Act
        var command = new UpdateEntityCommand<TestEntity>(id, entity);

        // Assert
        Assert.Equal(id, command.Id);
        Assert.Equal(entity, command.Entity);
        Assert.Same(entity, command.Entity);
    }

    [Fact]
    public void UpdateEntityCommand_With_Null_Entity_Should_Throw()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UpdateEntityCommand<TestEntity>(1, null!));
    }

    [Fact]
    public void UpdateEntityCommand_Generic_Should_Work()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };

        // Act
        var command = new UpdateEntityCommand<TestEntity, int>(123, entity);

        // Assert
        Assert.Equal(123, command.Id);
        Assert.Equal(entity, command.Entity);
        Assert.IsAssignableFrom<IUpdateCommand<TestEntity, int>>(command);
    }

    #endregion

    #region DeleteEntityCommand Tests

    [Fact]
    public void DeleteEntityCommand_Should_Initialize_With_Id()
    {
        // Arrange & Act
        var command = new DeleteEntityCommand(123);

        // Assert
        Assert.Equal(123, command.Id);
        Assert.IsAssignableFrom<IDeleteCommand>(command);
    }

    [Fact]
    public void DeleteEntityCommand_Generic_Should_Work()
    {
        // Arrange & Act
        var command = new DeleteEntityCommand<int>(123);

        // Assert
        Assert.Equal(123, command.Id);
        Assert.IsAssignableFrom<IDeleteCommand<int>>(command);
    }

    [Fact]
    public void DeleteEntityCommand_With_Different_Id_Types_Should_Work()
    {
        // Arrange & Act
        var guidCommand = new DeleteEntityCommand<Guid>(Guid.NewGuid());
        var stringCommand = new DeleteEntityCommand<string>("test-id");

        // Assert
        Assert.IsType<Guid>(guidCommand.Id);
        Assert.IsType<string>(stringCommand.Id);
        Assert.Equal("test-id", stringCommand.Id);
    }

    #endregion

    #region SoftDeleteEntityCommand Tests

    [Fact]
    public void SoftDeleteEntityCommand_Should_Initialize_With_Id()
    {
        // Arrange & Act
        var command = new SoftDeleteEntityCommand(123);

        // Assert
        Assert.Equal(123, command.Id);
        Assert.Null(command.DeletedBy);
        Assert.IsAssignableFrom<ISoftDeleteCommand>(command);
    }

    [Fact]
    public void SoftDeleteEntityCommand_Should_Initialize_With_Id_And_DeletedBy()
    {
        // Arrange & Act
        var command = new SoftDeleteEntityCommand(123, "admin");

        // Assert
        Assert.Equal(123, command.Id);
        Assert.Equal("admin", command.DeletedBy);
    }

    [Fact]
    public void SoftDeleteEntityCommand_Generic_Should_Work()
    {
        // Arrange & Act
        var command = new SoftDeleteEntityCommand<int>(123, "user");

        // Assert
        Assert.Equal(123, command.Id);
        Assert.Equal("user", command.DeletedBy);
        Assert.IsAssignableFrom<ISoftDeleteCommand<int>>(command);
    }

    #endregion

    #region GetEntityByIdQuery Tests

    [Fact]
    public void GetEntityByIdQuery_Should_Initialize_With_Id()
    {
        // Arrange & Act
        var query = new GetEntityByIdQuery<TestEntity>(123);

        // Assert
        Assert.Equal(123, query.Id);
        Assert.IsAssignableFrom<IGetByIdQuery<TestEntity>>(query);
    }

    [Fact]
    public void GetEntityByIdQuery_Generic_Should_Work()
    {
        // Arrange & Act
        var query = new GetEntityByIdQuery<TestEntity, int>(123);

        // Assert
        Assert.Equal(123, query.Id);
        Assert.IsAssignableFrom<IGetByIdQuery<TestEntity, int>>(query);
    }

    #endregion

    #region GetAllEntitiesQuery Tests

    [Fact]
    public void GetAllEntitiesQuery_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var query = new GetAllEntitiesQuery<TestEntity>();

        // Assert
        Assert.False(query.IncludeDeleted);
        Assert.IsAssignableFrom<IGetAllQuery<TestEntity>>(query);
    }

    [Fact]
    public void GetAllEntitiesQuery_Should_Initialize_With_IncludeDeleted()
    {
        // Arrange & Act
        var query = new GetAllEntitiesQuery<TestEntity>(includeDeleted: true);

        // Assert
        Assert.True(query.IncludeDeleted);
    }

    #endregion

    #region GetEntitiesPagedQuery Tests

    [Fact]
    public void GetEntitiesPagedQuery_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var query = new GetEntitiesPagedQuery<TestEntity>();

        // Assert
        Assert.Equal(1, query.PageNumber);
        Assert.Equal(10, query.PageSize);
        Assert.False(query.IncludeDeleted);
        Assert.IsAssignableFrom<IPagedQuery<TestEntity>>(query);
    }

    [Fact]
    public void GetEntitiesPagedQuery_Should_Initialize_With_Custom_Values()
    {
        // Arrange & Act
        var query = new GetEntitiesPagedQuery<TestEntity>(pageNumber: 3, pageSize: 25, includeDeleted: true);

        // Assert
        Assert.Equal(3, query.PageNumber);
        Assert.Equal(25, query.PageSize);
        Assert.True(query.IncludeDeleted);
    }

    [Theory]
    [InlineData(0, 1)]   // PageNumber 0 should become 1
    [InlineData(-1, 1)]  // Negative PageNumber should become 1
    [InlineData(5, 5)]   // Valid PageNumber should remain unchanged
    public void GetEntitiesPagedQuery_Should_Normalize_PageNumber(int inputPageNumber, int expectedPageNumber)
    {
        // Arrange & Act
        var query = new GetEntitiesPagedQuery<TestEntity>(pageNumber: inputPageNumber);

        // Assert
        Assert.Equal(expectedPageNumber, query.PageNumber);
    }

    [Theory]
    [InlineData(0, 10)]   // PageSize 0 should become 10
    [InlineData(-1, 10)]  // Negative PageSize should become 10
    [InlineData(25, 25)]  // Valid PageSize should remain unchanged
    public void GetEntitiesPagedQuery_Should_Normalize_PageSize(int inputPageSize, int expectedPageSize)
    {
        // Arrange & Act
        var query = new GetEntitiesPagedQuery<TestEntity>(pageSize: inputPageSize);

        // Assert
        Assert.Equal(expectedPageSize, query.PageSize);
    }

    #endregion

    #region Test Helper Classes

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}