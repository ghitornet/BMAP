using BMAP.Core.Mediator.Behaviors;
using Microsoft.Extensions.Logging;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for CommandValidationBehavior classes.
/// Tests cover validation logic, error handling, logging, and edge cases for both command variants.
/// </summary>
public class CommandValidationBehaviorTests
{
    #region CommandValidationBehavior (without response) Tests

    [Fact]
    public async Task CommandValidationBehavior_NoValidators_Should_CallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "Test" };
        var nextCalled = false;

        // Act
        await behavior.HandleAsync(command, Next);

        // Assert
        Assert.True(nextCalled);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CommandValidationBehavior_ValidCommand_Should_CallNext()
    {
        // Arrange
        var validator = new TestCommandValidator(ValidationResult.Success());
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "Valid Name" };
        var nextCalled = false;

        // Act
        await behavior.HandleAsync(command, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.True(validator.WasCalled);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CommandValidationBehavior_InvalidCommand_Should_ThrowValidationException()
    {
        // Arrange
        var error = new ValidationError("Name is required", nameof(TestCommand.Name));
        var validator = new TestCommandValidator(ValidationResult.Failure(error));
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "" };
        var nextCalled = false;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(command, Next));
        Assert.False(nextCalled);
        Assert.True(validator.WasCalled);
        Assert.Single(exception.Errors);
        Assert.Equal("Name is required", exception.Errors.First().Message);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CommandValidationBehavior_MultipleValidators_Should_RunAllValidators()
    {
        // Arrange
        var validator1 = new TestCommandValidator(ValidationResult.Success());
        var validator2 = new TestCommandValidator2(ValidationResult.Success());
        var validators = new IValidator<TestCommand>[] { validator1, validator2 };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "Valid" };
        var nextCalled = false;

        // Act
        await behavior.HandleAsync(command, Next);

        // Assert
        Assert.True(nextCalled);
        Assert.True(validator1.WasCalled);
        Assert.True(validator2.WasCalled);
        return;

        Task Next()
        {
            nextCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task CommandValidationBehavior_MultipleValidationErrors_Should_AggregateErrors()
    {
        // Arrange
        var error1 = new ValidationError("Name is required", "Name");
        var error2 = new ValidationError("Description is required", "Description");
        var validator1 = new TestCommandValidator(ValidationResult.Failure(error1));
        var validator2 = new TestCommandValidator2(ValidationResult.Failure(error2));
        var validators = new IValidator<TestCommand>[] { validator1, validator2 };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(command, Next));
        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.Message == "Name is required");
        Assert.Contains(exception.Errors, e => e.Message == "Description is required");
        return;

        static Task Next() => Task.CompletedTask;
    }

    [Fact]
    public async Task CommandValidationBehavior_ValidatorThrowsException_Should_WrapInValidationException()
    {
        // Arrange
        var validator = new ThrowingTestCommandValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(command, Next));
        Assert.Contains("Error occurred during validation", exception.Message);
        Assert.Single(exception.Errors);
        Assert.Equal("Test validation exception", exception.Errors.First().Message);
        return;

        static Task Next() => Task.CompletedTask;
    }

    [Fact]
    public void CommandValidationBehavior_NullLogger_Should_ThrowArgumentNullException()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommand>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandValidationBehavior<TestCommand>(validators, null!));
    }

    #endregion

    #region CommandValidationBehavior (with response) Tests

    [Fact]
    public async Task CommandValidationBehaviorWithResponse_NoValidators_Should_CallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommandWithResponse>>();
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "Test" };
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(command, Next);

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
    public async Task CommandValidationBehaviorWithResponse_ValidCommand_Should_CallNext()
    {
        // Arrange
        var validator = new TestCommandWithResponseValidator(ValidationResult.Success());
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "Valid Name" };
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(command, Next);

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
    public async Task CommandValidationBehaviorWithResponse_InvalidCommand_Should_ThrowValidationException()
    {
        // Arrange
        var error = new ValidationError("Name is required", nameof(TestCommandWithResponse.Name));
        var validator = new TestCommandWithResponseValidator(ValidationResult.Failure(error));
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "" };
        var nextCalled = false;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(command, Next));
        Assert.False(nextCalled);
        Assert.True(validator.WasCalled);
        Assert.Single(exception.Errors);
        Assert.Equal("Name is required", exception.Errors.First().Message);
        return;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("Success");
        }
    }

    [Fact]
    public async Task CommandValidationBehaviorWithResponse_MultipleValidators_Should_RunAllValidators()
    {
        // Arrange
        var validator1 = new TestCommandWithResponseValidator(ValidationResult.Success());
        var validator2 = new TestCommandWithResponseValidator2(ValidationResult.Success());
        var validators = new IValidator<TestCommandWithResponse>[] { validator1, validator2 };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "Valid" };
        var nextCalled = false;

        // Act
        var result = await behavior.HandleAsync(command, Next);

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
    public async Task CommandValidationBehaviorWithResponse_MultipleValidationErrors_Should_AggregateErrors()
    {
        // Arrange
        var error1 = new ValidationError("Name is required", "Name");
        var error2 = new ValidationError("Description is required", "Description");
        var validator1 = new TestCommandWithResponseValidator(ValidationResult.Failure(error1));
        var validator2 = new TestCommandWithResponseValidator2(ValidationResult.Failure(error2));
        var validators = new IValidator<TestCommandWithResponse>[] { validator1, validator2 };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(command, Next));
        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, e => e.Message == "Name is required");
        Assert.Contains(exception.Errors, e => e.Message == "Description is required");
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    [Fact]
    public async Task CommandValidationBehaviorWithResponse_ValidatorThrowsException_Should_WrapInValidationException()
    {
        // Arrange
        var validator = new ThrowingTestCommandWithResponseValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "Test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.HandleAsync(command, Next));
        Assert.Contains("Error occurred during validation", exception.Message);
        Assert.Single(exception.Errors);
        Assert.Equal("Test validation exception", exception.Errors.First().Message);
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    [Fact]
    public void CommandValidationBehaviorWithResponse_NullLogger_Should_ThrowArgumentNullException()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestCommandWithResponse>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommandValidationBehavior<TestCommandWithResponse, string>(validators, null!));
    }

    [Fact]
    public async Task CommandValidationBehavior_CancellationToken_Should_BePropagated()
    {
        // Arrange
        var validator = new CancellationTokenTestCommandValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommand>>();
        var behavior = new CommandValidationBehavior<TestCommand>(validators, logger);
        var command = new TestCommand { Name = "Test" };
        var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(command, Next, cts.Token);

        // Assert
        Assert.Equal(cts.Token, validator.ReceivedCancellationToken);
        return;

        static Task Next() => Task.CompletedTask;
    }

    [Fact]
    public async Task CommandValidationBehaviorWithResponse_CancellationToken_Should_BePropagated()
    {
        // Arrange
        var validator = new CancellationTokenTestCommandWithResponseValidator();
        var validators = new[] { validator };
        var logger = MockLoggerHelper.CreateNullLogger<CommandValidationBehavior<TestCommandWithResponse, string>>();
        var behavior = new CommandValidationBehavior<TestCommandWithResponse, string>(validators, logger);
        var command = new TestCommandWithResponse { Name = "Test" };
        var cts = new CancellationTokenSource();

        // Act
        await behavior.HandleAsync(command, Next, cts.Token);

        // Assert
        Assert.Equal(cts.Token, validator.ReceivedCancellationToken);
        return;

        static Task<string> Next() => Task.FromResult("Success");
    }

    #endregion

    #region Test Helper Classes

    // Test commands
    public class TestCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestCommandWithResponse : ICommand<string>
    {
        public string Name { get; set; } = string.Empty;
    }

    // Test validators for command without response
    public class TestCommandValidator(ValidationResult result) : IValidator<TestCommand>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestCommand request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    public class TestCommandValidator2(ValidationResult result) : IValidator<TestCommand>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestCommand request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    public class ThrowingTestCommandValidator : IValidator<TestCommand>
    {
        public Task<ValidationResult> ValidateAsync(TestCommand request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test validation exception");
        }
    }

    public class CancellationTokenTestCommandValidator : IValidator<TestCommand>
    {
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestCommand request, CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(ValidationResult.Success());
        }
    }

    // Test validators for command with response
    public class TestCommandWithResponseValidator(ValidationResult result) : IValidator<TestCommandWithResponse>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestCommandWithResponse request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    public class TestCommandWithResponseValidator2(ValidationResult result) : IValidator<TestCommandWithResponse>
    {
        public bool WasCalled { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestCommandWithResponse request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(result);
        }
    }

    public class ThrowingTestCommandWithResponseValidator : IValidator<TestCommandWithResponse>
    {
        public Task<ValidationResult> ValidateAsync(TestCommandWithResponse request, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test validation exception");
        }
    }

    public class CancellationTokenTestCommandWithResponseValidator : IValidator<TestCommandWithResponse>
    {
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<ValidationResult> ValidateAsync(TestCommandWithResponse request, CancellationToken cancellationToken = default)
        {
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(ValidationResult.Success());
        }
    }

    #endregion
}