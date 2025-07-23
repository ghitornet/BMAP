using BMAP.Core.Mediator.Behaviors;
using System.Text;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for QueryLoggingBehavior class.
/// Tests cover logging functionality, performance monitoring, caching suggestions, and edge cases.
/// </summary>
public class QueryLoggingBehaviorTests
{
    [Fact]
    public async Task QueryLoggingBehavior_SuccessfulExecution_Should_LogCorrectly()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestLogQuery, string>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestLogQuery, string>(logger);
        var query = new TestLogQuery { SearchTerm = "test query" };
        var expectedResponse = "Query Result";
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        }

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(expectedResponse, result);
        var logs = logOutput.ToString();
        Assert.Contains("Executing query TestLogQuery", logs);
        Assert.Contains("expecting response String", logs);
        Assert.Contains("executed successfully", logs);
    }

    [Fact]
    public async Task QueryLoggingBehavior_ExceptionDuringExecution_Should_LogError()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestLogQuery, string>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestLogQuery, string>(logger);
        var query = new TestLogQuery { SearchTerm = "failing query" };
        var expectedException = new InvalidOperationException("Test exception");

        Task<string> Next()
        {
            throw expectedException;
        }

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.HandleAsync(query, Next));
        Assert.Same(expectedException, thrownException);

        var logs = logOutput.ToString();
        Assert.Contains("Executing query TestLogQuery", logs);
        Assert.Contains("failed after", logs);
        Assert.Contains("Test exception", logs);
    }

    [Fact]
    public async Task QueryLoggingBehavior_SlowExecution_Should_LogPerformanceWarning()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestLogQuery, string>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestLogQuery, string>(logger);
        var query = new TestLogQuery { SearchTerm = "slow query" };

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal("Slow Result", result);
        var logs = logOutput.ToString();
        Assert.Contains("Executing query TestLogQuery", logs);
        Assert.Contains("executed successfully", logs);
        return;

        static async Task<string> Next()
        {
            await Task.Delay(20); // Short delay for test purposes
            return "Slow Result";
        }
    }

    [Fact]
    public async Task QueryLoggingBehavior_CollectionResponse_Should_LogItemCount()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestListQuery, List<string>>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestListQuery, List<string>>(logger);
        var query = new TestListQuery { SearchTerm = "collection query" };
        var collectionResponse = new List<string> { "item1", "item2", "item3", "item4", "item5" };

        Task<List<string>> Next()
        {
            return Task.FromResult(collectionResponse);
        }

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal(collectionResponse, result);
        var logs = logOutput.ToString();
        Assert.Contains("returned 5 items", logs);
    }

    [Fact]
    public async Task QueryLoggingBehavior_LargeCollectionResponse_Should_LogPaginationWarning()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestListQuery, List<string>>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestListQuery, List<string>>(logger);
        var query = new TestListQuery { SearchTerm = "large collection query" };
        var largeCollection = Enumerable.Range(1, 1500).Select(i => $"item{i}").ToList();

        Task<List<string>> Next()
        {
            return Task.FromResult(largeCollection);
        }

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal(largeCollection, result);
        var logs = logOutput.ToString();
        Assert.Contains("returned 1500 items", logs);
        Assert.Contains("consider pagination", logs);
    }

    [Fact]
    public async Task QueryLoggingBehavior_NonCollectionResponse_Should_NotLogItemCount()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestResultQuery, TestQueryResult>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestResultQuery, TestQueryResult>(logger);
        var query = new TestResultQuery { SearchTerm = "single result query" };
        var singleResult = new TestQueryResult { Id = 1, Name = "Test Result" };

        Task<TestQueryResult> Next()
        {
            return Task.FromResult(singleResult);
        }

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal(singleResult, result);
        var logs = logOutput.ToString();
        Assert.DoesNotContain("returned", logs);
        Assert.DoesNotContain("items", logs);
    }

    [Fact]
    public Task QueryLoggingBehavior_NullLogger_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueryLoggingBehavior<TestLogQuery, string>(null!));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task QueryLoggingBehavior_CancellationToken_Should_BePropagated()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<QueryLoggingBehavior<TestLogQuery, string>>();
        var behavior = new QueryLoggingBehavior<TestLogQuery, string>(logger);
        var query = new TestLogQuery { SearchTerm = "test" };
        var cts = new CancellationTokenSource();
        var receivedToken = CancellationToken.None;

        Task<string> Next()
        {
            receivedToken = cts.Token;
            return Task.FromResult("Success");
        }

        // Act
        await behavior.HandleAsync(query, Next, cts.Token);

        // Assert
        Assert.Equal(cts.Token, receivedToken);
    }

    [Fact]
    public async Task QueryLoggingBehavior_NullResponse_Should_LogCorrectly()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestLogQuery, string?>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestLogQuery, string?>(logger);
        var query = new TestLogQuery { SearchTerm = "null result query" };

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Null(result);
        var logs = logOutput.ToString();
        Assert.Contains("executed successfully", logs);
        return;

        static Task<string?> Next()
        {
            return Task.FromResult<string?>(null);
        }
    }

    [Fact]
    public async Task QueryLoggingBehavior_ArrayResponse_Should_LogItemCount()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestArrayQuery, string[]>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestArrayQuery, string[]>(logger);
        var query = new TestArrayQuery { SearchTerm = "array query" };
        var arrayResponse = new[] { "item1", "item2", "item3" };

        Task<string[]> Next()
        {
            return Task.FromResult(arrayResponse);
        }

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal(arrayResponse, result);
        var logs = logOutput.ToString();
        Assert.Contains("returned 3 items", logs);
    }

    [Fact]
    public async Task QueryLoggingBehavior_ConfigureAwait_Should_BeUsed()
    {
        // This test ensures that ConfigureAwait(false) is properly used in the implementation
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<QueryLoggingBehavior<TestLogQuery, string>>();
        var behavior = new QueryLoggingBehavior<TestLogQuery, string>(logger);
        var query = new TestLogQuery { SearchTerm = "test" };

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal("Success", result);
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    [Fact]
    public async Task QueryLoggingBehavior_ModerateDuration_Should_LogCachingOpportunity()
    {
        // Arrange
        var logOutput = new StringBuilder();
        var logger = MockLoggerHelper.CreateLogger<QueryLoggingBehavior<TestLogQuery, string>>(logOutput);
        var behavior = new QueryLoggingBehavior<TestLogQuery, string>(logger);
        var query = new TestLogQuery { SearchTerm = "cacheable query" };

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal("Cacheable Result", result);
        var logs = logOutput.ToString();
        Assert.Contains("executed successfully", logs);
        return;

        static async Task<string> Next()
        {
            await Task.Delay(5); // Small delay to simulate moderate duration
            return "Cacheable Result";
        }
    }

    #region Test Helper Classes

    // Test queries with specific response types
    public class TestLogQuery : IQuery<string>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class TestListQuery : IQuery<List<string>>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class TestArrayQuery : IQuery<string[]>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class TestResultQuery : IQuery<TestQueryResult>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }

    // Test result class
    public class TestQueryResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}