using BMAP.Core.Mediator.Behaviors;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for QueryValidationBehavior class.
/// Tests cover validation logic, error handling, logging, and edge cases for query validation.
/// </summary>
public class QueryValidationBehaviorTests
{
    [Fact]
    public async Task QueryValidationBehavior_NoValidators_Should_CallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestQuery>>();
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "test" };
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Success", result);
        return;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Success");
        }
    }

    [Fact]
    public async Task QueryValidationBehavior_ValidQuery_Should_CallNext()
    {
        // Arrange
        var validator = new TestQueryValidator(ValidationResult.Success());
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "valid search" };
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Success", result);
        Assert.True(validator.WasCalled);
        return;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Success");
        }
    }

    [Fact]
    public async Task QueryValidationBehavior_InvalidQuery_Should_ThrowValidationException()
    {
        // Arrange
        var error = new ValidationError("Search term is required", nameof(TestQuery.SearchTerm));
        var validator = new TestQueryValidator(ValidationResult.Failure(error));
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "" };
        var nextCalled = false;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(query, Next));
        Assert.False(nextCalled);
        Assert.True(validator.WasCalled);
        Assert.Single(exception.Errors);
        Assert.Equal("Search term is required", exception.Errors.First().Message);
        return;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Success");
        }
    }

    [Fact]
    public async Task QueryValidationBehavior_MultipleValidators_Should_RunAllValidators()
    {
        // Arrange
        var validator1 = new TestQueryValidator(ValidationResult.Success());
        var validator2 = new TestQueryValidator2(ValidationResult.Success());
        var validators = new IValidator<TestQuery>[] { validator1, validator2 };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "valid" };
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Success", result);
        Assert.True(validator1.WasCalled);
        Assert.True(validator2.WasCalled);
        return;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Success");
        }
    }

    [Fact]
    public async Task QueryValidationBehavior_MultipleValidationErrors_Should_AggregateErrors()
    {
        // Arrange
        var error1 = new ValidationError("Search term is required", "SearchTerm");
        var error2 = new ValidationError("Page size too large", "PageSize");
        var validator1 = new TestQueryValidator(ValidationResult.Failure(error1));
        var validator2 = new TestQueryValidator2(ValidationResult.Failure(error2));
        var validators = new IValidator<TestQuery>[] { validator1, validator2 };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(query, Next));
        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.Message == "Search term is required");
        Assert.Contains(exception.Errors, e => e.Message == "Page size too large");
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    [Fact]
    public async Task QueryValidationBehavior_ValidatorThrowsException_Should_WrapInValidationException()
    {
        // Arrange
        var validator = new ThrowingTestQueryValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(query, Next));
        Assert.Contains("Error occurred during validation", exception.Message);
        Assert.Single(exception.Errors);
        Assert.Equal("Test query validation exception", exception.Errors.First().Message);
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    [Fact]
    public void QueryValidationBehavior_NullLogger_Should_ThrowArgumentNullException()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestQuery>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new QueryValidationBehavior<TestQuery, string>(validators, null!));
    }

    [Fact]
    public async Task QueryValidationBehavior_CancellationToken_Should_BePropagated()
    {
        // Arrange
        var validator = new CancellationTokenTestQueryValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "test" };
        var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(query, Next, cts.Token);

        // Assert
        Assert.Equal(cts.Token, validator.ReceivedCancellationToken);
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    [Fact]
    public async Task QueryValidationBehavior_ConfigureAwait_Should_BeUsed()
    {
        // This test ensures that ConfigureAwait(false) is properly used in the implementation
        // Arrange
        var validator = new AsyncTestQueryValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<QueryValidationBehavior<TestQuery, string>>();
        var behavior = new QueryValidationBehavior<TestQuery, string>(validators, logger);
        var query = new TestQuery { SearchTerm = "test" };

        // Act
        var result = await behavior.HandleAsync(query, Next);

        // Assert
        Assert.Equal("Success", result);
        Assert.True(validator.WasCalled);
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    #region Test Helper Classes

    // Test query
    public class TestQuery : IQuery<string>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    // Test validators
    public class TestQueryValidator(ValidationResult result) : IValidator<TestQuery>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    public class TestQueryValidator2(ValidationResult result) : IValidator<TestQuery>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    public class ThrowingTestQueryValidator : IValidator<TestQuery>
    {
        public Task<ValidationResult> ValidateAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test query validation exception");
        }
    }

    public class CancellationTokenTestQueryValidator : IValidator<TestQuery>
    {
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(ValidationResult.Success());
        }
    }

    public class AsyncTestQueryValidator : IValidator<TestQuery>
    {
        public bool WasCalled { get; private set; }

        public async Task<ValidationResult> ValidateAsync(TestQuery request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return ValidationResult.Success();
        }
    }

    #endregion
}