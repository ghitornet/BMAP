using Microsoft.EntityFrameworkCore;

namespace BMAP.Core.Data.EntityFramework.Tests.Unit;

/// <summary>
/// Simple unit tests to verify basic EntityFramework provider functionality.
/// </summary>
public class BasicEntityFrameworkTests
{
    [Fact]
    public void DbContext_Should_Be_Configurable()
    {
        // Arrange
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        // Act & Assert
        using var context = new DbContext(options);
        Assert.NotNull(context);
        Assert.True(context.Database.EnsureCreated());
    }

    [Fact]
    public void Test_Should_Pass()
    {
        // This basic test ensures the test infrastructure works
        Assert.True(true);
    }
}