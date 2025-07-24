using BMAP.Core.Data.Queries;

namespace BMAP.Core.Data.Tests.Queries;

/// <summary>
/// Unit tests for PagedResult class to ensure proper pagination functionality.
/// </summary>
public class PagedResultTests
{
    [Fact]
    public void PagedResult_Constructor_Should_Set_Properties_Correctly()
    {
        // Arrange
        var items = new[] { "item1", "item2", "item3" };
        const int totalCount = 25;
        const int pageNumber = 2;
        const int pageSize = 10;

        // Act
        var result = new PagedResult<string>(items, totalCount, pageNumber, pageSize);

        // Assert
        Assert.Equal(items, result.Items);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
    }

    [Fact]
    public void PagedResult_Should_Calculate_TotalPages_Correctly()
    {
        // Arrange & Act
        var result1 = new PagedResult<string>([], 25, 1, 10); // 3 pages
        var result2 = new PagedResult<string>([], 30, 1, 10); // 3 pages  
        var result3 = new PagedResult<string>([], 31, 1, 10); // 4 pages
        var result4 = new PagedResult<string>([], 0, 1, 10);  // 0 pages

        // Assert
        Assert.Equal(3, result1.TotalPages);
        Assert.Equal(3, result2.TotalPages);
        Assert.Equal(4, result3.TotalPages);
        Assert.Equal(0, result4.TotalPages);
    }

    [Fact]
    public void PagedResult_Should_Calculate_HasPreviousPage_Correctly()
    {
        // Arrange & Act
        var result1 = new PagedResult<string>([], 25, 1, 10); // First page
        var result2 = new PagedResult<string>([], 25, 2, 10); // Second page
        var result3 = new PagedResult<string>([], 25, 3, 10); // Third page

        // Assert
        Assert.False(result1.HasPreviousPage);
        Assert.True(result2.HasPreviousPage);
        Assert.True(result3.HasPreviousPage);
    }

    [Fact]
    public void PagedResult_Should_Calculate_HasNextPage_Correctly()
    {
        // Arrange & Act
        var result1 = new PagedResult<string>([], 25, 1, 10); // Page 1 of 3
        var result2 = new PagedResult<string>([], 25, 2, 10); // Page 2 of 3
        var result3 = new PagedResult<string>([], 25, 3, 10); // Page 3 of 3

        // Assert
        Assert.True(result1.HasNextPage);
        Assert.True(result2.HasNextPage);
        Assert.False(result3.HasNextPage);
    }

    [Fact]
    public void PagedResult_With_Single_Page_Should_Have_No_Navigation()
    {
        // Arrange & Act
        var result = new PagedResult<string>(["item1"], 1, 1, 10);

        // Assert
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_With_Empty_Items_Should_Work()
    {
        // Arrange & Act
        var result = new PagedResult<string>([], 0, 1, 10);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void PagedResult_Constructor_With_Null_Items_Should_Throw()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new PagedResult<string>(null!, 10, 1, 10));
    }

    [Theory]
    [InlineData(100, 10, 1, 10, true)]   // First page of 10 pages
    [InlineData(100, 10, 5, 10, true)]   // Middle page of 10 pages
    [InlineData(100, 10, 10, 10, false)] // Last page of 10 pages
    [InlineData(5, 10, 1, 1, false)]     // Single page - total count is less than page size
    public void PagedResult_HasNextPage_Should_Be_Correct(int totalCount, int pageSize, int pageNumber, int totalPages, bool expectedHasNext)
    {
        // Arrange & Act
        var result = new PagedResult<string>([], totalCount, pageNumber, pageSize);

        // Assert
        Assert.Equal(totalPages, result.TotalPages);
        Assert.Equal(expectedHasNext, result.HasNextPage);
    }

    [Theory]
    [InlineData(100, 10, 1, false)]  // First page
    [InlineData(100, 10, 5, true)]   // Middle page
    [InlineData(100, 10, 10, true)]  // Last page
    [InlineData(5, 10, 1, false)]    // Single page
    public void PagedResult_HasPreviousPage_Should_Be_Correct(int totalCount, int pageSize, int pageNumber, bool expectedHasPrevious)
    {
        // Arrange & Act
        var result = new PagedResult<string>([], totalCount, pageNumber, pageSize);

        // Assert
        Assert.Equal(expectedHasPrevious, result.HasPreviousPage);
    }
}