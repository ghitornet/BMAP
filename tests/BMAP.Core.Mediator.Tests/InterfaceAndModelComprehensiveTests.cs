namespace BMAP.Core.Mediator.Tests;

/// <summary>
/// Unit tests for interface and model classes to maximize code coverage.
/// Tests cover default implementations, inheritance, and type constraints.
/// </summary>
public class InterfaceAndModelTests
{
    #region IRequest Tests

    [Fact]
    public void IRequest_Implementation_Should_Work()
    {
        // Arrange & Act
        var request = new TestSimpleRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest>(request);
    }

    [Fact]
    public void IRequestWithResponse_Implementation_Should_Work()
    {
        // Arrange & Act
        var request = new TestRequestWithResponse();

        // Assert
        Assert.IsAssignableFrom<IRequest<string>>(request);
        Assert.IsAssignableFrom<IRequest>(request); // Should also implement base interface
    }

    [Fact]
    public void IRequest_Multiple_Implementations_Should_Work()
    {
        // Arrange & Act
        var requests = new IRequest[]
        {
            new TestSimpleRequest(),
            new TestRequestWithResponse(),
            new TestAnotherRequest()
        };

        // Assert
        Assert.All(requests, r => Assert.IsAssignableFrom<IRequest>(r));
    }

    [Fact]
    public void IRequest_Generic_Types_Should_Work()
    {
        // Arrange & Act
        var stringRequest = new TestGenericRequest<string> { Value = "test" };
        var intRequest = new TestGenericRequest<int> { Value = 42 };
        var boolRequest = new TestGenericRequest<bool> { Value = true };

        // Assert
        Assert.IsAssignableFrom<IRequest<string>>(stringRequest);
        Assert.IsAssignableFrom<IRequest<int>>(intRequest);
        Assert.IsAssignableFrom<IRequest<bool>>(boolRequest);
        Assert.Equal("test", stringRequest.Value);
        Assert.Equal(42, intRequest.Value);
        Assert.True(boolRequest.Value);
    }

    #endregion

    #region ICommand Tests

    [Fact]
    public void ICommand_Implementation_Should_Work()
    {
        // Arrange & Act
        var command = new TestSimpleCommand();

        // Assert
        Assert.IsAssignableFrom<ICommand>(command);
        Assert.IsAssignableFrom<IRequest>(command); // Should inherit from IRequest
    }

    [Fact]
    public void ICommandWithResponse_Implementation_Should_Work()
    {
        // Arrange & Act
        var command = new TestCommandWithResponse();

        // Assert
        Assert.IsAssignableFrom<ICommand<string>>(command);
        Assert.IsAssignableFrom<IRequest<string>>(command); // Should inherit from IRequest<T>
        Assert.IsAssignableFrom<IRequest>(command); // Should also implement base interface
    }

    [Fact]
    public void ICommand_Properties_Should_Work()
    {
        // Arrange & Act
        var command = new TestCommandWithProperties
        {
            Name = "Test Command",
            Value = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal("Test Command", command.Name);
        Assert.Equal(100, command.Value);
        Assert.True(command.IsActive);
        Assert.True(command.CreatedAt <= DateTime.UtcNow);
    }

    #endregion

    #region IQuery Tests

    [Fact]
    public void IQuery_Implementation_Should_Work()
    {
        // Arrange & Act
        var query = new TestSimpleQuery();

        // Assert
        Assert.IsAssignableFrom<IQuery<string>>(query);
        Assert.IsAssignableFrom<IRequest<string>>(query); // Should inherit from IRequest<T>
        Assert.IsAssignableFrom<IRequest>(query); // Should also implement base interface
    }

    [Fact]
    public void IQuery_Properties_Should_Work()
    {
        // Arrange & Act
        var query = new TestQueryWithProperties
        {
            SearchTerm = "test search",
            PageSize = 25,
            PageNumber = 2,
            SortBy = "Name",
            SortDescending = true
        };

        // Assert
        Assert.Equal("test search", query.SearchTerm);
        Assert.Equal(25, query.PageSize);
        Assert.Equal(2, query.PageNumber);
        Assert.Equal("Name", query.SortBy);
        Assert.True(query.SortDescending);
    }

    [Fact]
    public void IQuery_Different_Response_Types_Should_Work()
    {
        // Arrange & Act
        var stringQuery = new TestStringQuery { Value = "string query" };
        var intQuery = new TestIntQuery { Value = 42 };
        var listQuery = new TestListQuery { Filter = "list filter" };
        var boolQuery = new TestBoolQuery { Condition = "test condition" };

        // Assert
        Assert.IsAssignableFrom<IQuery<string>>(stringQuery);
        Assert.IsAssignableFrom<IQuery<int>>(intQuery);
        Assert.IsAssignableFrom<IQuery<List<string>>>(listQuery);
        Assert.IsAssignableFrom<IQuery<bool>>(boolQuery);
    }

    #endregion

    #region INotification Tests

    [Fact]
    public void INotification_Implementation_Should_Work()
    {
        // Arrange & Act
        var notification = new TestSimpleNotification();

        // Assert
        Assert.IsAssignableFrom<INotification>(notification);
    }

    [Fact]
    public void INotification_Properties_Should_Work()
    {
        // Arrange & Act
        var notification = new TestNotificationWithProperties
        {
            EventId = Guid.NewGuid(),
            EventType = "UserCreated",
            OccurredAt = DateTime.UtcNow,
            Data = "notification data",
            Source = "UserService"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, notification.EventId);
        Assert.Equal("UserCreated", notification.EventType);
        Assert.True(notification.OccurredAt <= DateTime.UtcNow);
        Assert.Equal("notification data", notification.Data);
        Assert.Equal("UserService", notification.Source);
    }

    [Fact]
    public void INotification_Multiple_Implementations_Should_Work()
    {
        // Arrange & Act
        var notifications = new INotification[]
        {
            new TestSimpleNotification(),
            new TestNotificationWithProperties(),
            new TestAnotherNotification()
        };

        // Assert
        Assert.All(notifications, n => Assert.IsAssignableFrom<INotification>(n));
    }

    #endregion

    #region Handler Interface Tests

    [Fact]
    public void IRequestHandler_Implementation_Should_Work()
    {
        // Arrange & Act
        var handler = new TestSimpleRequestHandler();

        // Assert
        Assert.IsAssignableFrom<IRequestHandler<TestSimpleRequest>>(handler);
    }

    [Fact]
    public void IRequestHandlerWithResponse_Implementation_Should_Work()
    {
        // Arrange & Act
        var handler = new TestRequestWithResponseHandler();

        // Assert
        Assert.IsAssignableFrom<IRequestHandler<TestRequestWithResponse, string>>(handler);
    }

    [Fact]
    public void ICommandHandler_Implementation_Should_Work()
    {
        // Arrange & Act
        var handler = new TestSimpleCommandHandler();

        // Assert
        Assert.IsAssignableFrom<ICommandHandler<TestSimpleCommand>>(handler);
        Assert.IsAssignableFrom<IRequestHandler<TestSimpleCommand>>(handler); // Should inherit from IRequestHandler
    }

    [Fact]
    public void ICommandHandlerWithResponse_Implementation_Should_Work()
    {
        // Arrange & Act
        var handler = new TestCommandWithResponseHandler();

        // Assert
        Assert.IsAssignableFrom<ICommandHandler<TestCommandWithResponse, string>>(handler);
        Assert.IsAssignableFrom<IRequestHandler<TestCommandWithResponse, string>>(handler); // Should inherit from IRequestHandler
    }

    [Fact]
    public void IQueryHandler_Implementation_Should_Work()
    {
        // Arrange & Act
        var handler = new TestSimpleQueryHandler();

        // Assert
        Assert.IsAssignableFrom<IQueryHandler<TestSimpleQuery, string>>(handler);
        Assert.IsAssignableFrom<IRequestHandler<TestSimpleQuery, string>>(handler); // Should inherit from IRequestHandler
    }

    [Fact]
    public void INotificationHandler_Implementation_Should_Work()
    {
        // Arrange & Act
        var handler = new TestSimpleNotificationHandler();

        // Assert
        Assert.IsAssignableFrom<INotificationHandler<TestSimpleNotification>>(handler);
    }

    #endregion

    #region Type Constraint Tests

    [Fact]
    public void ICommand_Should_Inherit_From_IRequest()
    {
        // Assert
        Assert.True(typeof(IRequest).IsAssignableFrom(typeof(ICommand)));
    }

    [Fact]
    public void ICommandWithResponse_Should_Inherit_From_IRequest()
    {
        // Assert
        Assert.True(typeof(IRequest<string>).IsAssignableFrom(typeof(ICommand<string>)));
        Assert.True(typeof(IRequest).IsAssignableFrom(typeof(ICommand<string>)));
    }

    [Fact]
    public void IQuery_Should_Inherit_From_IRequest()
    {
        // Assert
        Assert.True(typeof(IRequest<string>).IsAssignableFrom(typeof(IQuery<string>)));
        Assert.True(typeof(IRequest).IsAssignableFrom(typeof(IQuery<string>)));
    }

    [Fact]
    public void ICommandHandler_Should_Inherit_From_IRequestHandler()
    {
        // Assert
        Assert.True(typeof(IRequestHandler<TestSimpleCommand>).IsAssignableFrom(typeof(ICommandHandler<TestSimpleCommand>)));
        Assert.True(typeof(IRequestHandler<TestCommandWithResponse, string>).IsAssignableFrom(typeof(ICommandHandler<TestCommandWithResponse, string>)));
    }

    [Fact]
    public void IQueryHandler_Should_Inherit_From_IRequestHandler()
    {
        // Assert
        Assert.True(typeof(IRequestHandler<TestSimpleQuery, string>).IsAssignableFrom(typeof(IQueryHandler<TestSimpleQuery, string>)));
    }

    #endregion

    #region Test Helper Classes

    // Test requests
    public class TestSimpleRequest : IRequest;
    public class TestRequestWithResponse : IRequest<string>;
    public class TestAnotherRequest : IRequest;
    public class TestGenericRequest<T> : IRequest<T> { public T? Value { get; set; } }

    // Test commands
    public class TestSimpleCommand : ICommand;
    public class TestCommandWithResponse : ICommand<string>;
    public class TestCommandWithProperties : ICommand
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Test queries
    public class TestSimpleQuery : IQuery<string>;
    public class TestQueryWithProperties : IQuery<List<string>>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
    public class TestStringQuery : IQuery<string> { public string Value { get; set; } = string.Empty; }
    public class TestIntQuery : IQuery<int> { public int Value { get; set; } }
    public class TestListQuery : IQuery<List<string>> { public string Filter { get; set; } = string.Empty; }
    public class TestBoolQuery : IQuery<bool> { public string Condition { get; set; } = string.Empty; }

    // Test notifications
    public class TestSimpleNotification : INotification;
    public class TestNotificationWithProperties : INotification
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string Data { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
    public class TestAnotherNotification : INotification;

    // Test handlers
    public class TestSimpleRequestHandler : IRequestHandler<TestSimpleRequest>
    {
        public Task HandleAsync(TestSimpleRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public Task<string> HandleAsync(TestRequestWithResponse request, CancellationToken cancellationToken = default) => Task.FromResult("Response");
    }

    public class TestSimpleCommandHandler : ICommandHandler<TestSimpleCommand>
    {
        public Task HandleAsync(TestSimpleCommand request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<string> HandleAsync(TestCommandWithResponse request, CancellationToken cancellationToken = default) => Task.FromResult("Command Response");
    }

    public class TestSimpleQueryHandler : IQueryHandler<TestSimpleQuery, string>
    {
        public Task<string> HandleAsync(TestSimpleQuery request, CancellationToken cancellationToken = default) => Task.FromResult("Query Response");
    }

    public class TestSimpleNotificationHandler : INotificationHandler<TestSimpleNotification>
    {
        public Task HandleAsync(TestSimpleNotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    #endregion
}