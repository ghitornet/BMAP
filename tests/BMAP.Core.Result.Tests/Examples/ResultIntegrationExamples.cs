using BMAP.Core.Result;
using BMAP.Core.Result.Extensions;
using BMAP.Core.Result.Utilities;

namespace BMAP.Core.Result.Tests.Examples;

/// <summary>
/// Integration examples demonstrating the Result pattern usage in real-world scenarios.
/// These examples show how to use the Result pattern for error handling, validation, and functional composition.
/// </summary>
public class ResultIntegrationExamples
{
    [Fact]
    public void UserRegistration_Example_Should_Demonstrate_Complete_Workflow()
    {
        // This example demonstrates a complete user registration workflow
        // using the Result pattern for validation and error handling

        // Arrange
        var validUser = new UserRegistrationRequest("john.doe@example.com", "SecurePassword123!", "John", "Doe");
        var invalidUser = new UserRegistrationRequest("invalid-email", "weak", "", "");

        // Act & Assert - Valid user should succeed
        var validResult = RegisterUser(validUser);
        Assert.True(validResult.IsSuccess);
        Assert.Equal("john.doe@example.com", validResult.Value.Email);

        // Act & Assert - Invalid user should fail with detailed validation errors
        var invalidResult = RegisterUser(invalidUser);
        Assert.True(invalidResult.IsFailure);
        Assert.Equal(ErrorType.Validation, invalidResult.Error!.Type);
        Assert.Contains("Multiple validation errors occurred", invalidResult.Error.Message);
    }

    [Fact]
    public void FileProcessing_Example_Should_Demonstrate_Exception_Handling()
    {
        // This example demonstrates how to use Result pattern for file operations
        // with proper exception handling

        // Act & Assert - Non-existent file should return failure
        var result = ProcessFile("non-existent-file.txt");
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error!.Type);
        Assert.Equal("Exception.Caught", result.Error.Code);
    }

    [Fact]
    public void CalculationChain_Example_Should_Demonstrate_Functional_Composition()
    {
        // This example demonstrates chaining operations with Result pattern

        // Act & Assert - Valid calculation chain
        var validResult = PerformCalculationChain(10, 2);
        Assert.True(validResult.IsSuccess);
        Assert.Equal("Final result: 10", validResult.Value);

        // Act & Assert - Division by zero should fail
        var invalidResult = PerformCalculationChain(10, 0);
        Assert.True(invalidResult.IsFailure);
        Assert.Contains("Division by zero", invalidResult.Error!.Message);
    }

    [Fact]
    public async Task AsyncWorkflow_Example_Should_Demonstrate_Async_Operations()
    {
        // This example demonstrates async Result operations

        // Act & Assert
        var result = await ProcessAsyncWorkflow("valid-data");
        Assert.True(result.IsSuccess);
        Assert.Contains("Processed", result.Value);
    }

    [Fact]
    public void ApiResponse_Example_Should_Demonstrate_Api_Error_Mapping()
    {
        // This example demonstrates how to map different error types
        // to appropriate HTTP responses (simulated)

        var notFoundResult = GetUser(-1);
        var unauthorizedResult = GetUser(999);
        var validResult = GetUser(1);

        // Simulate API response mapping
        var notFoundResponse = MapToHttpResponse(notFoundResult);
        var unauthorizedResponse = MapToHttpResponse(unauthorizedResult);
        var validResponse = MapToHttpResponse(validResult);

        Assert.Equal(404, notFoundResponse.StatusCode);
        Assert.Equal(401, unauthorizedResponse.StatusCode);
        Assert.Equal(200, validResponse.StatusCode);
        Assert.NotNull(validResponse.Data);
    }

    #region Helper Methods for Examples

    private static Result<User> RegisterUser(UserRegistrationRequest request)
    {
        return ValidateUserRegistration(request)
            .Bind(() => CheckEmailAvailability(request.Email))
            .Bind(() => CreateUser(request))
            .Tap(user => SendWelcomeEmail(user.Email));
    }

    private static Result ValidateUserRegistration(UserRegistrationRequest request)
    {
        var validations = new[]
        {
            ResultUtilities.Validate(!string.IsNullOrWhiteSpace(request.Email), "Email", request.Email, "Email is required"),
            ResultUtilities.Validate(request.Email.Contains("@"), "Email", request.Email, "Email must be valid"),
            ResultUtilities.Validate(request.Password.Length >= 8, "Password", request.Password.Length, "Password must be at least 8 characters"),
            ResultUtilities.Validate(!string.IsNullOrWhiteSpace(request.FirstName), "FirstName", request.FirstName, "First name is required"),
            ResultUtilities.Validate(!string.IsNullOrWhiteSpace(request.LastName), "LastName", request.LastName, "Last name is required")
        };

        return ResultUtilities.AggregateValidations(validations);
    }

    private static Result CheckEmailAvailability(string email)
    {
        // Simulate email availability check
        if (email == "taken@example.com")
            return Result.Failure(Error.Conflict("USER.EMAIL_TAKEN", "Email address is already in use"));

        return Result.Success();
    }

    private static Result<User> CreateUser(UserRegistrationRequest request)
    {
        // Simulate user creation
        var user = new User(
            Id: Random.Shared.Next(1, 1000),
            Email: request.Email,
            FirstName: request.FirstName,
            LastName: request.LastName,
            CreatedAt: DateTime.UtcNow
        );

        return Result<User>.Success(user);
    }

    private static void SendWelcomeEmail(string email)
    {
        // Simulate sending welcome email
        Console.WriteLine($"Welcome email sent to {email}");
    }

    private static Result<string> ProcessFile(string filePath)
    {
        return ResultUtilities.Try(() =>
        {
            // Simulate file processing that might throw exceptions
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return File.ReadAllText(filePath).ToUpperInvariant();
        });
    }

    private static Result<string> PerformCalculationChain(int x, int y)
    {
        return Result<int>.Success(x)
            .Ensure(value => value > 0, Error.Validation("CALC.NEGATIVE", "Value must be positive"))
            .Bind(value => Divide(value, y))
            .Map(result => result * 2)
            .Ensure(result => result < 100, Error.Validation("CALC.TOO_LARGE", "Result too large"))
            .Map(result => $"Final result: {result}");
    }

    private static Result<int> Divide(int dividend, int divisor)
    {
        if (divisor == 0)
            return Result<int>.Failure(Error.Validation("CALC.DIVISION_BY_ZERO", "Division by zero is not allowed"));

        return Result<int>.Success(dividend / divisor);
    }

    private static async Task<Result<string>> ProcessAsyncWorkflow(string data)
    {
        var result = Result<string>.Success(data);
        result = await result.TapAsync(async d => await SimulateAsyncValidation(d));
        
        var processedResult = await result.BindAsync(async d => await SimulateAsyncProcessing(d));
        if (processedResult.IsFailure)
            return processedResult;
            
        return await processedResult.MapAsync(async d => await SimulateAsyncFormatting(d));
    }

    private static Task SimulateAsyncValidation(string data)
    {
        return Task.Delay(10); // Simulate async validation
    }

    private static Task<Result<string>> SimulateAsyncProcessing(string data)
    {
        return Task.FromResult(Result<string>.Success($"Processed: {data}"));
    }

    private static Task<string> SimulateAsyncFormatting(string data)
    {
        return Task.FromResult($"Formatted: {data}");
    }

    private static Result<User> GetUser(int userId)
    {
        return userId switch
        {
            -1 => Result<User>.Failure(Error.NotFound("USER.NOT_FOUND", $"User with ID {userId} was not found")),
            999 => Result<User>.Failure(Error.Unauthorized("USER.UNAUTHORIZED", "Authentication required to access user data")),
            _ => Result<User>.Success(new User(userId, "test@example.com", "Test", "User", DateTime.UtcNow))
        };
    }

    private static HttpSimulatedResponse MapToHttpResponse<T>(Result<T> result)
    {
        return result.Match(
            value => new HttpSimulatedResponse(200, "OK", value),
            error => error.Type switch
            {
                ErrorType.NotFound => new HttpSimulatedResponse(404, "Not Found", error.Message),
                ErrorType.Unauthorized => new HttpSimulatedResponse(401, "Unauthorized", error.Message),
                ErrorType.Forbidden => new HttpSimulatedResponse(403, "Forbidden", error.Message),
                ErrorType.Validation => new HttpSimulatedResponse(400, "Bad Request", error.Message),
                ErrorType.Conflict => new HttpSimulatedResponse(409, "Conflict", error.Message),
                _ => new HttpSimulatedResponse(500, "Internal Server Error", "An unexpected error occurred")
            }
        );
    }

    #endregion

    #region Test Data Models

    private record UserRegistrationRequest(string Email, string Password, string FirstName, string LastName);

    private record User(int Id, string Email, string FirstName, string LastName, DateTime CreatedAt);

    private record HttpSimulatedResponse(int StatusCode, string StatusMessage, object? Data);

    #endregion
}