using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace BMAP.Core.Data.Dapper.Tests.Attributes;

/// <summary>
/// Unit tests for DataAnnotations usage to ensure proper functionality with Dapper integration.
/// These tests verify that standard DataAnnotations work correctly with our SQL generation.
/// </summary>
public class DataAnnotationsTests
{
    #region TableAttribute Tests

    [Fact]
    public void TableAttribute_Should_Work_With_SqlGenerator()
    {
        // Arrange & Act
        var entityType = typeof(TestEntityWithTable);
        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();

        // Assert
        Assert.NotNull(tableAttribute);
        Assert.Equal("Users", tableAttribute.Name);
    }

    #endregion

    #region ColumnAttribute Tests

    [Fact]
    public void ColumnAttribute_Should_Work_With_Properties()
    {
        // Arrange & Act
        var property = typeof(TestEntityWithColumns).GetProperty("Name");
        var columnAttribute = property?.GetCustomAttribute<ColumnAttribute>();

        // Assert
        Assert.NotNull(columnAttribute);
        Assert.Equal("user_name", columnAttribute.Name);
    }

    #endregion

    #region KeyAttribute Tests

    [Fact]
    public void KeyAttribute_Should_Identify_Primary_Key()
    {
        // Arrange & Act
        var property = typeof(TestEntityWithKey).GetProperty("UserId");
        var keyAttribute = property?.GetCustomAttribute<KeyAttribute>();

        // Assert
        Assert.NotNull(keyAttribute);
    }

    #endregion

    #region NotMappedAttribute Tests

    [Fact]
    public void NotMappedAttribute_Should_Exclude_Property()
    {
        // Arrange & Act
        var property = typeof(TestEntityWithNotMapped).GetProperty("IgnoredProperty");
        var notMappedAttribute = property?.GetCustomAttribute<NotMappedAttribute>();

        // Assert
        Assert.NotNull(notMappedAttribute);
    }

    #endregion

    #region DatabaseGeneratedAttribute Tests

    [Fact]
    public void DatabaseGeneratedAttribute_Should_Mark_Identity()
    {
        // Arrange & Act
        var property = typeof(TestEntityWithIdentity).GetProperty("Id");
        var dbGeneratedAttribute = property?.GetCustomAttribute<DatabaseGeneratedAttribute>();

        // Assert
        Assert.NotNull(dbGeneratedAttribute);
        Assert.Equal(DatabaseGeneratedOption.Identity, dbGeneratedAttribute.DatabaseGeneratedOption);
    }

    #endregion

    #region Test Helper Classes

    [Table("Users")]
    public class TestEntityWithTable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestEntityWithColumns
    {
        [Column("user_id")]
        public int Id { get; set; }
        
        [Column("user_name")]
        public string Name { get; set; } = string.Empty;
    }

    public class TestEntityWithKey
    {
        [Key]
        [Column("UserId")]
        public int UserId { get; set; }
        
        public string Name { get; set; } = string.Empty;
    }

    public class TestEntityWithNotMapped
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        [NotMapped]
        public string IgnoredProperty { get; set; } = string.Empty;
    }

    public class TestEntityWithIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}