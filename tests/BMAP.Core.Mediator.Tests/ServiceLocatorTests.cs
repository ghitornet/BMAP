using Microsoft.Extensions.DependencyInjection;

namespace BMAP.Core.Mediator.Tests;

/// <summary>
///     Test cases for ServiceLocator implementation.
/// </summary>
public class ServiceLocatorTests
{
    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_WhenServiceProviderIsNull()
    {
        // Arrange
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ServiceLocator(null!, logger));
        Assert.Equal("serviceProvider", exception.ParamName);
    }

    [Fact]
    public void GetService_Generic_Should_ReturnService_WhenServiceIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetService<ITestService>();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestService>(result);
    }

    [Fact]
    public void GetService_Generic_Should_ThrowInvalidOperationException_WhenServiceIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => serviceLocator.GetService<ITestService>());
        Assert.Equal("Service of type ITestService is not registered.", exception.Message);
    }

    [Fact]
    public void GetService_Type_Should_ReturnService_WhenServiceIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetService(typeof(ITestService));

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestService>(result);
    }

    [Fact]
    public void GetService_Type_Should_ThrowInvalidOperationException_WhenServiceIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => serviceLocator.GetService(typeof(ITestService)));
        Assert.Equal("Service of type ITestService is not registered.", exception.Message);
    }

    [Fact]
    public void GetService_Type_Should_ThrowArgumentNullException_WhenServiceTypeIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => serviceLocator.GetService(null!));
        Assert.Equal("serviceType", exception.ParamName);
    }

    [Fact]
    public void GetServices_Generic_Should_ReturnAllServices_WhenMultipleServicesAreRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        services.AddTransient<ITestService, AnotherTestService>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetServices<ITestService>();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, service => Assert.IsAssignableFrom<ITestService>(service));
    }

    [Fact]
    public void GetServices_Type_Should_ReturnAllServices_WhenMultipleServicesAreRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        services.AddTransient<ITestService, AnotherTestService>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetServices(typeof(ITestService));

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, service => Assert.IsAssignableFrom<ITestService>(service));
    }

    [Fact]
    public void GetServices_Type_Should_ThrowArgumentNullException_WhenServiceTypeIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => serviceLocator.GetServices(null!));
        Assert.Equal("serviceType", exception.ParamName);
    }

    [Fact]
    public void GetServiceOrDefault_Generic_Should_ReturnService_WhenServiceIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetServiceOrDefault<ITestService>();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestService>(result);
    }

    [Fact]
    public void GetServiceOrDefault_Generic_Should_ReturnNull_WhenServiceIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetServiceOrDefault<ITestService>();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetServiceOrDefault_Type_Should_ReturnService_WhenServiceIsRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetServiceOrDefault(typeof(ITestService));

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TestService>(result);
    }

    [Fact]
    public void GetServiceOrDefault_Type_Should_ReturnNull_WhenServiceIsNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act
        var result = serviceLocator.GetServiceOrDefault(typeof(ITestService));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetServiceOrDefault_Type_Should_ThrowArgumentNullException_WhenServiceTypeIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var logger = MockLoggerHelper.CreateNullLogger<ServiceLocator>();
        var serviceLocator = new ServiceLocator(serviceProvider, logger);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => serviceLocator.GetServiceOrDefault(null!));
        Assert.Equal("serviceType", exception.ParamName);
    }

    // Test interfaces and classes
    private interface ITestService
    {
    }

    private class TestService : ITestService
    {
    }

    private class AnotherTestService : ITestService
    {
    }
}