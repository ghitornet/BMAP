using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BMAP.Core.Data.Dapper.Helpers;
using BMAP.Core.Data.Entities;

namespace BMAP.Core.Data.Dapper.Tests.Helpers;

/// <summary>
/// Unit tests for SqlGenerator to ensure proper SQL statement generation with DataAnnotations.
/// </summary>
public class SqlGeneratorTests
{
    #region GenerateSelectById Tests

    [Fact]
    public void GenerateSelectById_Should_Generate_Basic_Select()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectById<TestEntity>();

        // Assert
        Assert.Contains("SELECT", sql);
        Assert.Contains("FROM TestEntity", sql);
        Assert.Contains("WHERE Id = @Id", sql);
    }

    [Fact]
    public void GenerateSelectById_With_Table_Attribute_Should_Use_Table_Name()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectById<TestEntityWithTable>();

        // Assert
        Assert.Contains("FROM Users", sql);
    }

    [Fact]
    public void GenerateSelectById_With_Custom_Primary_Key_Should_Use_Custom_Key()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectById<TestEntityWithCustomPrimaryKey>();

        // Assert
        Assert.Contains("WHERE UserId = @Id", sql);
    }

    [Fact]
    public void GenerateSelectById_With_Column_Attributes_Should_Use_Column_Names()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectById<TestEntityWithColumns>();

        // Assert
        Assert.Contains("user_id", sql);
        Assert.Contains("user_name", sql);
        Assert.Contains("email_address", sql);
    }

    [Fact]
    public void GenerateSelectById_With_NotMapped_Properties_Should_Exclude_Them()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectById<TestEntityWithNotMapped>();

        // Assert
        Assert.Contains("Id", sql);
        Assert.Contains("Name", sql);
        Assert.DoesNotContain("NotMappedProperty", sql);
    }

    #endregion

    #region GenerateSelectAll Tests

    [Fact]
    public void GenerateSelectAll_Should_Generate_Basic_Select_All()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectAll<TestEntity>();

        // Assert
        Assert.Contains("SELECT", sql);
        Assert.Contains("FROM TestEntity", sql);
        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void GenerateSelectAll_With_SoftDeletable_Should_Exclude_Deleted()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectAll<TestSoftDeletableEntity>(includeDeleted: false);

        // Assert
        Assert.Contains("WHERE IsDeleted = 0", sql);
    }

    [Fact]
    public void GenerateSelectAll_With_SoftDeletable_IncludeDeleted_Should_Not_Filter()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectAll<TestSoftDeletableEntity>(includeDeleted: true);

        // Assert
        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void GenerateSelectAll_With_Additional_Where_Should_Add_Condition()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectAll<TestEntity>(whereClause: "Status = 'Active'");

        // Assert
        Assert.Contains("WHERE (Status = 'Active')", sql);
    }

    [Fact]
    public void GenerateSelectAll_With_SoftDeletable_And_Additional_Where_Should_Combine_Conditions()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectAll<TestSoftDeletableEntity>(includeDeleted: false, whereClause: "Status = 'Active'");

        // Assert
        Assert.Contains("WHERE IsDeleted = 0 AND (Status = 'Active')", sql);
    }

    #endregion

    #region GenerateInsert Tests

    [Fact]
    public void GenerateInsert_Should_Generate_Basic_Insert()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateInsert<TestEntity>();

        // Assert
        Assert.Contains("INSERT INTO TestEntity", sql);
        Assert.Contains("(Id, Name)", sql);
        Assert.Contains("VALUES (@Id, @Name)", sql);
    }

    [Fact]
    public void GenerateInsert_With_Identity_Should_Exclude_Identity_Column_And_Return_Id()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateInsert<TestEntityWithIdentity>();

        // Assert
        Assert.Contains("INSERT INTO", sql);
        Assert.DoesNotContain("@Id", sql); // Identity column should be excluded
        Assert.Contains("SELECT last_insert_rowid();", sql); // SQLite syntax for identity retrieval
    }

    #endregion

    #region GenerateUpdate Tests

    [Fact]
    public void GenerateUpdate_Should_Generate_Basic_Update()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateUpdate<TestEntity>();

        // Assert
        Assert.Contains("UPDATE TestEntity", sql);
        Assert.Contains("SET", sql);
        Assert.Contains("WHERE Id = @Id", sql);
    }

    [Fact]
    public void GenerateUpdate_Should_Exclude_Primary_Key_From_Set_Clause()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateUpdate<TestEntity>();

        // Assert
        var setClauseMatch = System.Text.RegularExpressions.Regex.Match(sql, @"SET\s+(.+?)\s+WHERE");
        Assert.True(setClauseMatch.Success);
        var setClause = setClauseMatch.Groups[1].Value;
        Assert.DoesNotContain("Id =", setClause); // Primary key should not be in SET clause
    }

    #endregion

    #region GenerateDelete Tests

    [Fact]
    public void GenerateDelete_Should_Generate_Basic_Delete()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateDelete<TestEntity>();

        // Assert
        Assert.Contains("DELETE FROM TestEntity", sql);
        Assert.Contains("WHERE Id = @Id", sql);
    }

    #endregion

    #region GenerateSoftDelete Tests

    [Fact]
    public void GenerateSoftDelete_Should_Generate_Update_For_Soft_Delete()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSoftDelete<TestSoftDeletableEntity>();

        // Assert
        Assert.Contains("UPDATE TestSoftDeletableEntity", sql);
        Assert.Contains("SET IsDeleted = 1", sql);
        Assert.Contains("DeletedAt = @DeletedAt", sql);
        Assert.Contains("DeletedBy = @DeletedBy", sql);
        Assert.Contains("WHERE Id = @Id", sql);
    }

    [Fact]
    public void GenerateSoftDelete_With_Non_SoftDeletable_Should_Throw()
    {
        // Arrange, Act & Assert
        Assert.Throws<InvalidOperationException>(() => SqlGenerator.GenerateSoftDelete<TestEntity>());
    }

    #endregion

    #region GenerateCount Tests

    [Fact]
    public void GenerateCount_Should_Generate_Count_Query()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateCount<TestEntity>();

        // Assert
        Assert.Contains("SELECT COUNT(*)", sql);
        Assert.Contains("FROM TestEntity", sql);
    }

    [Fact]
    public void GenerateCount_With_SoftDeletable_Should_Exclude_Deleted()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateCount<TestSoftDeletableEntity>(includeDeleted: false);

        // Assert
        Assert.Contains("WHERE IsDeleted = 0", sql);
    }

    #endregion

    #region GenerateSelectPaged Tests

    [Fact]
    public void GenerateSelectPaged_Should_Generate_Paged_Query()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectPaged<TestEntity>();

        // Assert
        Assert.Contains("SELECT", sql);
        Assert.Contains("FROM TestEntity", sql);
        Assert.Contains("ORDER BY Id", sql);
        Assert.Contains("LIMIT @PageSize OFFSET @Offset", sql); // SQLite syntax for pagination
    }

    [Fact]
    public void GenerateSelectPaged_With_Custom_OrderBy_Should_Use_Custom_Order()
    {
        // Arrange & Act
        var sql = SqlGenerator.GenerateSelectPaged<TestEntity>(orderBy: "Name DESC");

        // Assert
        Assert.Contains("ORDER BY Name DESC", sql);
    }

    #endregion

    #region Test Helper Classes

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Table("Users")]
    public class TestEntityWithTable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestEntityWithCustomPrimaryKey
    {
        [Key]
        [Column("UserId")]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestEntityWithColumns
    {
        [Column("user_id")]
        public int Id { get; set; }
        
        [Column("user_name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("email_address")]
        public string Email { get; set; } = string.Empty;
    }

    public class TestEntityWithNotMapped
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        [NotMapped]
        public string NotMappedProperty { get; set; } = string.Empty;
    }

    public class TestSoftDeletableEntity : ISoftDeletable
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
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