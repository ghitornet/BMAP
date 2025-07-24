using BMAP.Core.Data.Entities;

namespace BMAP.Core.Data.Tests.Entities;

/// <summary>
/// Unit tests for entity interfaces to ensure proper implementation and inheritance.
/// </summary>
public class EntityInterfaceTests
{
    [Fact]
    public void IEntity_Should_Have_Id_Property()
    {
        // Arrange & Act
        var entity = new TestEntity();
        entity.Id = 123;

        // Assert
        Assert.Equal(123, entity.Id);
        Assert.IsAssignableFrom<IEntity<int>>(entity);
        Assert.IsAssignableFrom<IEntity>(entity);
    }

    [Fact]
    public void IEntityGeneric_Should_Support_Different_Id_Types()
    {
        // Arrange & Act
        var guidEntity = new TestGuidEntity();
        var stringEntity = new TestStringEntity();
        var longEntity = new TestLongEntity();

        var testGuid = Guid.NewGuid();
        guidEntity.Id = testGuid;
        stringEntity.Id = "test-id";
        longEntity.Id = 999L;

        // Assert
        Assert.Equal(testGuid, guidEntity.Id);
        Assert.Equal("test-id", stringEntity.Id);
        Assert.Equal(999L, longEntity.Id);

        Assert.IsAssignableFrom<IEntity<Guid>>(guidEntity);
        Assert.IsAssignableFrom<IEntity<string>>(stringEntity);
        Assert.IsAssignableFrom<IEntity<long>>(longEntity);
    }

    [Fact]
    public void ICreatable_Should_Track_Creation_Info()
    {
        // Arrange
        var entity = new TestCreatableEntity();
        var now = DateTime.UtcNow;

        // Act
        entity.CreatedAt = now;
        entity.CreatedBy = "test-user";

        // Assert
        Assert.Equal(now, entity.CreatedAt);
        Assert.Equal("test-user", entity.CreatedBy);
        Assert.IsAssignableFrom<ICreatable>(entity);
    }

    [Fact]
    public void IModifiable_Should_Track_Modification_Info()
    {
        // Arrange
        var entity = new TestModifiableEntity();
        var now = DateTime.UtcNow;

        // Act
        entity.LastModifiedAt = now;
        entity.LastModifiedBy = "modifier-user";

        // Assert
        Assert.Equal(now, entity.LastModifiedAt);
        Assert.Equal("modifier-user", entity.LastModifiedBy);
        Assert.IsAssignableFrom<IModifiable>(entity);
    }

    [Fact]
    public void ISoftDeletable_Should_Track_Deletion_Info()
    {
        // Arrange
        var entity = new TestSoftDeletableEntity();
        var now = DateTime.UtcNow;

        // Act
        entity.IsDeleted = true;
        entity.DeletedAt = now;
        entity.DeletedBy = "deleter-user";

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.Equal(now, entity.DeletedAt);
        Assert.Equal("deleter-user", entity.DeletedBy);
        Assert.IsAssignableFrom<ISoftDeletable>(entity);
    }

    [Fact]
    public void IAuditableEntity_Should_Implement_All_Interfaces()
    {
        // Arrange & Act
        var entity = new TestAuditableEntity();

        // Assert
        Assert.IsAssignableFrom<IEntity<int>>(entity);
        Assert.IsAssignableFrom<IEntity>(entity);
        Assert.IsAssignableFrom<ICreatable>(entity);
        Assert.IsAssignableFrom<IModifiable>(entity);
        Assert.IsAssignableFrom<ISoftDeletable>(entity);
        Assert.IsAssignableFrom<IAuditableEntity<int>>(entity);
        Assert.IsAssignableFrom<IAuditableEntity>(entity);
    }

    [Fact]
    public void IAuditableEntity_Should_Support_Complete_Lifecycle()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var modifiedAt = DateTime.UtcNow.AddHours(-1);
        var deletedAt = DateTime.UtcNow;

        // Act - Creation
        entity.Id = 1;
        entity.CreatedAt = createdAt;
        entity.CreatedBy = "creator";

        // Act - Modification
        entity.LastModifiedAt = modifiedAt;
        entity.LastModifiedBy = "modifier";

        // Act - Soft Deletion
        entity.IsDeleted = true;
        entity.DeletedAt = deletedAt;
        entity.DeletedBy = "deleter";

        // Assert
        Assert.Equal(1, entity.Id);
        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal(modifiedAt, entity.LastModifiedAt);
        Assert.Equal("modifier", entity.LastModifiedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedAt, entity.DeletedAt);
        Assert.Equal("deleter", entity.DeletedBy);
    }

    #region Test Helper Classes

    public class TestEntity : IEntity
    {
        public int Id { get; set; }
    }

    public class TestGuidEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }
    }

    public class TestStringEntity : IEntity<string>
    {
        public string Id { get; set; } = string.Empty;
    }

    public class TestLongEntity : IEntity<long>
    {
        public long Id { get; set; }
    }

    public class TestCreatableEntity : ICreatable
    {
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class TestModifiableEntity : IModifiable
    {
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    public class TestSoftDeletableEntity : ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }

    public class TestAuditableEntity : IAuditableEntity
    {
        public int Id { get; set; }
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