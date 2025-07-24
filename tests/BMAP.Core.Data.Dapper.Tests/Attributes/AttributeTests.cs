using BMAP.Core.Data.Dapper.Attributes;

namespace BMAP.Core.Data.Dapper.Tests.Attributes;

/// <summary>
/// Unit tests for Dapper attributes to ensure proper functionality and validation.
/// </summary>
public class AttributeTests
{
    #region TableAttribute Tests

    [Fact]
    public void TableAttribute_Should_Initialize_With_Name()
    {
        // Arrange & Act
        var attribute = new TableAttribute("Users");

        // Assert
        Assert.Equal("Users", attribute.Name);
        Assert.Null(attribute.Schema);
        Assert.Equal("Users", attribute.FullName);
    }

    [Fact]
    public void TableAttribute_Should_Support_Schema()
    {
        // Arrange & Act
        var attribute = new TableAttribute("Users") { Schema = "dbo" };

        // Assert
        Assert.Equal("Users", attribute.Name);
        Assert.Equal("dbo", attribute.Schema);
        Assert.Equal("dbo.Users", attribute.FullName);
    }

    [Fact]
    public void TableAttribute_With_Null_Name_Should_Throw()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TableAttribute(null!));
    }

    [Fact]
    public void TableAttribute_FullName_Without_Schema_Should_Return_Name_Only()
    {
        // Arrange & Act
        var attribute = new TableAttribute("Products");

        // Assert
        Assert.Equal("Products", attribute.FullName);
    }

    [Fact]
    public void TableAttribute_FullName_With_Empty_Schema_Should_Return_Name_Only()
    {
        // Arrange & Act
        var attribute = new TableAttribute("Products") { Schema = "" };

        // Assert
        Assert.Equal("Products", attribute.FullName);
    }

    #endregion

    #region ColumnAttribute Tests

    [Fact]
    public void ColumnAttribute_Should_Initialize_With_Name()
    {
        // Arrange & Act
        var attribute = new ColumnAttribute("Id");

        // Assert
        Assert.Equal("Id", attribute.Name);
        Assert.False(attribute.IsPrimaryKey);
        Assert.False(attribute.IsIdentity);
        Assert.False(attribute.IgnoreOnInsert);
        Assert.False(attribute.IgnoreOnUpdate);
    }

    [Fact]
    public void ColumnAttribute_Should_Support_PrimaryKey()
    {
        // Arrange & Act
        var attribute = new ColumnAttribute("Id") { IsPrimaryKey = true };

        // Assert
        Assert.Equal("Id", attribute.Name);
        Assert.True(attribute.IsPrimaryKey);
    }

    [Fact]
    public void ColumnAttribute_Should_Support_Identity()
    {
        // Arrange & Act
        var attribute = new ColumnAttribute("Id") { IsIdentity = true };

        // Assert
        Assert.Equal("Id", attribute.Name);
        Assert.True(attribute.IsIdentity);
    }

    [Fact]
    public void ColumnAttribute_Should_Support_IgnoreOnInsert()
    {
        // Arrange & Act
        var attribute = new ColumnAttribute("UpdatedAt") { IgnoreOnInsert = true };

        // Assert
        Assert.Equal("UpdatedAt", attribute.Name);
        Assert.True(attribute.IgnoreOnInsert);
    }

    [Fact]
    public void ColumnAttribute_Should_Support_IgnoreOnUpdate()
    {
        // Arrange & Act
        var attribute = new ColumnAttribute("CreatedAt") { IgnoreOnUpdate = true };

        // Assert
        Assert.Equal("CreatedAt", attribute.Name);
        Assert.True(attribute.IgnoreOnUpdate);
    }

    [Fact]
    public void ColumnAttribute_Should_Support_All_Properties()
    {
        // Arrange & Act
        var attribute = new ColumnAttribute("Id")
        {
            IsPrimaryKey = true,
            IsIdentity = true,
            IgnoreOnInsert = false,
            IgnoreOnUpdate = true
        };

        // Assert
        Assert.Equal("Id", attribute.Name);
        Assert.True(attribute.IsPrimaryKey);
        Assert.True(attribute.IsIdentity);
        Assert.False(attribute.IgnoreOnInsert);
        Assert.True(attribute.IgnoreOnUpdate);
    }

    [Fact]
    public void ColumnAttribute_With_Null_Name_Should_Throw()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ColumnAttribute(null!));
    }

    #endregion

    #region IgnoreAttribute Tests

    [Fact]
    public void IgnoreAttribute_Should_Be_Creatable()
    {
        // Arrange & Act
        var attribute = new IgnoreAttribute();

        // Assert
        Assert.NotNull(attribute);
    }

    #endregion

    #region Attribute Usage Tests

    [Fact]
    public void Attributes_Should_Be_Applicable_To_Correct_Targets()
    {
        // Arrange & Act
        var testEntity = new TestEntityWithAttributes();

        // Assert - Check that the class can be decorated with TableAttribute
        var tableAttribute = testEntity.GetType().GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() as TableAttribute;
        Assert.NotNull(tableAttribute);
        Assert.Equal("TestEntities", tableAttribute.Name);

        // Assert - Check that properties can be decorated with ColumnAttribute
        var idProperty = testEntity.GetType().GetProperty(nameof(TestEntityWithAttributes.Id));
        var idColumnAttribute = idProperty?.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
        Assert.NotNull(idColumnAttribute);
        Assert.Equal("Id", idColumnAttribute.Name);
        Assert.True(idColumnAttribute.IsPrimaryKey);

        // Assert - Check that properties can be decorated with IgnoreAttribute
        var ignoredProperty = testEntity.GetType().GetProperty(nameof(TestEntityWithAttributes.IgnoredProperty));
        var ignoreAttribute = ignoredProperty?.GetCustomAttributes(typeof(IgnoreAttribute), false).FirstOrDefault();
        Assert.NotNull(ignoreAttribute);
    }

    #endregion

    #region Test Helper Classes

    [Table("TestEntities")]
    public class TestEntityWithAttributes
    {
        [Column("Id", IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("CreatedAt", IgnoreOnUpdate = true)]
        public DateTime CreatedAt { get; set; }

        [Column("UpdatedAt", IgnoreOnInsert = true)]
        public DateTime? UpdatedAt { get; set; }

        [Ignore]
        public string IgnoredProperty { get; set; } = string.Empty;
    }

    #endregion
}