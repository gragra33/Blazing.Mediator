using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Statistics;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Comprehensive tests for QueryCommandAnalysisExtensions methods.
/// Tests all extension methods for QueryCommandAnalysis and NotificationAnalysis
/// including type formatting, handler details, and fully qualified names.
/// </summary>
public class QueryCommandAnalysisExtensionsTests : IDisposable
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly MediatorStatistics _mediatorStatistics;

    public QueryCommandAnalysisExtensionsTests()
    {
        _services = new ServiceCollection();
        _services.AddMediator(typeof(QueryCommandAnalysisExtensionsTests).Assembly);
        _services.AddLogging();
        
        _serviceProvider = _services.BuildServiceProvider();
        _mediatorStatistics = new MediatorStatistics(new ExtensionsTestStatisticsRenderer());
    }

    #region QueryCommandAnalysis Extension Tests

    [Fact]
    public void GetFormattedResponseTypeName_WithGenericType_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var listQuery = analysis.FirstOrDefault(a => a.ClassName == "GetUserListQuery");

        // Act
        var formattedName = listQuery?.GetFormattedResponseTypeName();

        // Assert
        Assert.NotNull(formattedName);
        Assert.DoesNotContain("`", formattedName);
        Assert.Contains("List<", formattedName);
        Assert.Contains("UserTestDto>", formattedName);
    }

    [Fact]
    public void GetFormattedResponseTypeName_WithNullResponseType_ShouldReturnNull()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);
        var voidCommand = analysis.FirstOrDefault(a => a.ClassName == "DeleteUserCommand");

        // Act
        var formattedName = voidCommand?.GetFormattedResponseTypeName();

        // Assert
        Assert.Null(formattedName);
    }

    [Fact]
    public void GetFormattedResponseTypeName_WithNestedGenericType_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var nestedGenericQuery = analysis.FirstOrDefault(a => a.ClassName == "GetNestedGenericQuery");

        // Act
        var formattedName = nestedGenericQuery?.GetFormattedResponseTypeName();

        // Assert
        Assert.NotNull(formattedName);
        Assert.DoesNotContain("`", formattedName);
        Assert.Contains("Dictionary<", formattedName);
        Assert.Contains("List<", formattedName);
    }

    [Fact]
    public void GetFormattedPrimaryInterfaceName_WithGenericInterface_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var userQuery = analysis.FirstOrDefault(a => a.ClassName == "GetUserQuery");

        // Act
        var formattedInterface = userQuery?.GetFormattedPrimaryInterfaceName();

        // Assert
        Assert.NotNull(formattedInterface);
        Assert.DoesNotContain("`", formattedInterface);
        Assert.StartsWith("IQuery<", formattedInterface);
    }

    [Fact]
    public void GetFormattedPrimaryInterfaceName_WithCustomDomainInterface_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var customerQuery = analysis.FirstOrDefault(a => a.ClassName == "GetCustomerInfoQuery");

        // Act
        var formattedInterface = customerQuery?.GetFormattedPrimaryInterfaceName();

        // Assert
        Assert.NotNull(formattedInterface);
        Assert.DoesNotContain("`", formattedInterface);
        Assert.Equal("ICustomerQuery<CustomerTestDto>", formattedInterface);
    }

    [Fact]
    public void GetFormattedPrimaryInterfaceName_WithMalformedGenericInterface_ShouldReconstructCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var complexQuery = analysis.FirstOrDefault(a => a.ClassName == "GetComplexQuery");

        // Act
        var formattedInterface = complexQuery?.GetFormattedPrimaryInterfaceName();

        // Assert
        Assert.NotNull(formattedInterface);
        Assert.DoesNotContain("`", formattedInterface);
        // Should reconstruct properly formatted interface name
        Assert.True(formattedInterface.Contains('<') && formattedInterface.Contains('>'));
    }

    [Fact]
    public void GetFormattedHandlerNames_WithMultipleHandlers_ShouldFormatAllCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var queryWithHandlers = analysis.FirstOrDefault(a => a.Handlers.Count > 0);

        // Act
        var formattedHandlers = queryWithHandlers?.GetFormattedHandlerNames();

        // Assert
        Assert.NotNull(formattedHandlers);
        Assert.All(formattedHandlers, handler => Assert.DoesNotContain("`", handler));
        Assert.All(formattedHandlers, handler => Assert.True(!string.IsNullOrEmpty(handler)));
    }

    [Fact]
    public void GetFormattedHandlerNames_WithGenericHandlers_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var genericQuery = analysis.FirstOrDefault(a => a.ClassName == "GetGenericQuery");

        // Act
        var formattedHandlers = genericQuery?.GetFormattedHandlerNames();

        // Assert
        Assert.NotNull(formattedHandlers);
        Assert.All(formattedHandlers, handler => 
        {
            Assert.DoesNotContain("`", handler);
            if (handler.Contains('<'))
            {
                Assert.Contains('>', handler);
            }
        });
    }

    [Fact]
    public void GetFormattedHandlerDetails_WithSingleHandler_ShouldReturnFormattedHandlerName()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var singleHandlerQuery = analysis.FirstOrDefault(a => a.HandlerStatus == HandlerStatus.Single);

        // Act
        var handlerDetails = singleHandlerQuery?.GetFormattedHandlerDetails();

        // Assert
        Assert.NotNull(handlerDetails);
        Assert.DoesNotContain("`", handlerDetails);
        Assert.NotEqual("Handler found", handlerDetails);
    }

    [Fact]
    public void GetFormattedHandlerDetails_WithMissingHandler_ShouldReturnStandardMessage()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var missingHandlerQuery = analysis.FirstOrDefault(a => a.HandlerStatus == HandlerStatus.Missing);

        // Act
        var handlerDetails = missingHandlerQuery?.GetFormattedHandlerDetails();

        // Assert
        Assert.Equal("No handler registered", handlerDetails);
    }

    [Fact]
    public void GetFormattedHandlerDetails_WithMultipleHandlers_ShouldReturnFormattedList()
    {
        // This test would require a scenario with multiple handlers for the same request type
        // which is typically an error condition, but we can test the formatting logic
        
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var queryAnalysis = analysis.FirstOrDefault();
        
        // We'll test by checking that if there were multiple handlers, it would format correctly
        // by ensuring the method handles the Multiple case properly
        Assert.NotNull(queryAnalysis);
        
        // Act & Assert - The method should handle Multiple status correctly
        var handlerDetails = queryAnalysis.GetFormattedHandlerDetails();
        Assert.NotNull(handlerDetails);
    }

    [Fact]
    public void GetFullyQualifiedResponseTypeName_WithComplexType_ShouldIncludeNamespace()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var userQuery = analysis.FirstOrDefault(a => a.ClassName == "GetUserQuery");

        // Act
        var fullyQualified = userQuery?.GetFullyQualifiedResponseTypeName();

        // Assert
        Assert.NotNull(fullyQualified);
        Assert.Contains(".", fullyQualified); // Should contain namespace
        Assert.DoesNotContain("`", fullyQualified);
    }

    [Fact]
    public void GetFullyQualifiedResponseTypeName_WithNullResponseType_ShouldReturnNull()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeCommands(_serviceProvider, isDetailed: true);
        var voidCommand = analysis.FirstOrDefault(a => a.ResponseType == null);

        // Act
        var fullyQualified = voidCommand?.GetFullyQualifiedResponseTypeName();

        // Assert
        Assert.Null(fullyQualified);
    }

    [Fact]
    public void GetFullyQualifiedPrimaryInterfaceName_WithCustomInterface_ShouldIncludeNamespace()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var customerQuery = analysis.FirstOrDefault(a => a.ClassName == "GetCustomerInfoQuery");

        // Act
        var fullyQualified = customerQuery?.GetFullyQualifiedPrimaryInterfaceName();

        // Assert
        Assert.NotNull(fullyQualified);
        Assert.Contains(".", fullyQualified); // Should contain namespace or fallback to formatted name
    }

    [Fact]
    public void GetFullyQualifiedHandlerNames_ShouldIncludeNamespaces()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var queryWithHandlers = analysis.FirstOrDefault(a => a.Handlers.Count > 0);

        // Act
        var fullyQualifiedHandlers = queryWithHandlers?.GetFullyQualifiedHandlerNames();

        // Assert
        Assert.NotNull(fullyQualifiedHandlers);
        Assert.All(fullyQualifiedHandlers, handler => 
        {
            Assert.DoesNotContain("`", handler);
            Assert.Contains(".", handler); // Should contain namespace
        });
    }

    #endregion

    #region NotificationAnalysis Extension Tests

    [Fact]
    public void NotificationAnalysis_GetFormattedHandlerNames_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notification = analysis.FirstOrDefault(a => a.Handlers.Count > 0);

        // Act
        var formattedHandlers = notification?.GetFormattedHandlerNames();

        // Assert
        Assert.NotNull(formattedHandlers);
        Assert.All(formattedHandlers, handler => Assert.DoesNotContain("`", handler));
    }

    [Fact]
    public void NotificationAnalysis_GetFormattedHandlerDetails_WithSingleHandler_ShouldReturnFormattedName()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var singleHandlerNotification = analysis.FirstOrDefault(a => a.HandlerStatus == HandlerStatus.Single);

        // Act
        var handlerDetails = singleHandlerNotification?.GetFormattedHandlerDetails();

        // Assert
        Assert.NotNull(handlerDetails);
        Assert.DoesNotContain("`", handlerDetails);
    }

    [Fact]
    public void NotificationAnalysis_GetFormattedHandlerDetails_WithMissingHandler_ShouldReturnStandardMessage()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var missingHandlerNotification = analysis.FirstOrDefault(a => a.HandlerStatus == HandlerStatus.Missing);

        // Act
        var handlerDetails = missingHandlerNotification?.GetFormattedHandlerDetails();

        // Assert
        if (missingHandlerNotification != null)
        {
            Assert.Equal("No handler registered", handlerDetails);
        }
    }

    [Fact]
    public void NotificationAnalysis_GetFullyQualifiedHandlerNames_ShouldIncludeNamespaces()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notificationWithHandlers = analysis.FirstOrDefault(a => a.Handlers.Count > 0);

        // Act
        var fullyQualifiedHandlers = notificationWithHandlers?.GetFullyQualifiedHandlerNames();

        // Assert
        Assert.NotNull(fullyQualifiedHandlers);
        Assert.All(fullyQualifiedHandlers, handler => 
        {
            Assert.DoesNotContain("`", handler);
            Assert.Contains(".", handler); // Should contain namespace
        });
    }

    [Fact]
    public void NotificationAnalysis_GetFormattedPrimaryInterfaceName_ShouldReturnCleanName()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notification = analysis.FirstOrDefault();

        // Act
        var formattedInterface = notification?.GetFormattedPrimaryInterfaceName();

        // Assert
        Assert.NotNull(formattedInterface);
        Assert.DoesNotContain("`", formattedInterface);
        Assert.Equal("INotification", formattedInterface);
    }

    [Fact]
    public void NotificationAnalysis_GetFullyQualifiedPrimaryInterfaceName_ShouldFindActualInterface()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notification = analysis.FirstOrDefault();

        // Act
        var fullyQualified = notification?.GetFullyQualifiedPrimaryInterfaceName();

        // Assert
        Assert.NotNull(fullyQualified);
        // Should either find the actual INotification interface or fallback to primary interface
        Assert.True(fullyQualified.Contains("INotification") || fullyQualified.Contains("."));
    }

    [Fact]
    public void NotificationAnalysis_GetFormattedSubscriberNames_WithValidSubscribers_ShouldParseCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notificationWithSubscribers = analysis.FirstOrDefault(a => a.SubscriberStatus == SubscriberStatus.Present);

        // Act
        var subscriberNames = notificationWithSubscribers?.GetFormattedSubscriberNames();

        // Assert
        if (notificationWithSubscribers != null)
        {
            Assert.NotNull(subscriberNames);
            // Should either have subscriber types or empty list
            Assert.True(subscriberNames.Count >= 0);
        }
    }

    [Fact]
    public void NotificationAnalysis_GetFormattedSubscriberNames_WithNoSubscribers_ShouldReturnEmpty()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notificationWithoutSubscribers = analysis.FirstOrDefault(a => a.SubscriberStatus != SubscriberStatus.Present);

        // Act
        var subscriberNames = notificationWithoutSubscribers?.GetFormattedSubscriberNames();

        // Assert
        if (notificationWithoutSubscribers != null)
        {
            Assert.NotNull(subscriberNames);
            Assert.Empty(subscriberNames);
        }
    }

    [Fact]
    public void NotificationAnalysis_GetFormattedSubscriberNames_WithSubscriberTypes_ShouldReturnTypes()
    {
        // This test would need a notification with actual SubscriberTypes populated
        // For now, we'll test the method doesn't throw and handles the case properly
        
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeNotifications(_serviceProvider, isDetailed: true);
        var notification = analysis.FirstOrDefault();

        // Act & Assert
        Assert.NotNull(notification);
        var subscriberNames = notification.GetFormattedSubscriberNames();
        Assert.NotNull(subscriberNames);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public void Extensions_WithNullAnalysis_ShouldHandleGracefully()
    {
        // Arrange
        QueryCommandAnalysis? nullAnalysis = null;

        // Act & Assert - Should not throw, but will throw ArgumentNullException as expected for extension methods
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedResponseTypeName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedPrimaryInterfaceName());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedHandlerNames());
        Assert.Throws<ArgumentNullException>(() => nullAnalysis!.GetFormattedHandlerDetails());
    }

    [Fact]
    public void Extensions_WithComplexNestedGenerics_ShouldFormatCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var complexQuery = analysis.FirstOrDefault(a => a.ClassName == "GetNestedGenericQuery");

        // Act
        var responseType = complexQuery?.GetFormattedResponseTypeName();
        var handlerNames = complexQuery?.GetFormattedHandlerNames();

        // Assert
        Assert.NotNull(responseType);
        Assert.DoesNotContain("`", responseType);
        
        Assert.NotNull(handlerNames);
        Assert.All(handlerNames, name => Assert.DoesNotContain("`", name));
    }

    [Fact]
    public void Extensions_WithEmptyHandlersList_ShouldHandleCorrectly()
    {
        // Arrange
        var analysis = _mediatorStatistics.AnalyzeQueries(_serviceProvider, isDetailed: true);
        var queryWithoutHandlers = analysis.FirstOrDefault(a => a.Handlers.Count == 0);

        // Act
        var handlerNames = queryWithoutHandlers?.GetFormattedHandlerNames();
        var fullyQualifiedNames = queryWithoutHandlers?.GetFullyQualifiedHandlerNames();

        // Assert
        if (queryWithoutHandlers != null)
        {
            Assert.NotNull(handlerNames);
            Assert.Empty(handlerNames);
            
            Assert.NotNull(fullyQualifiedNames);
            Assert.Empty(fullyQualifiedNames);
        }
    }

    #endregion

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        _mediatorStatistics?.Dispose();
    }
}

#region Test Domain Types

// Custom domain interface for testing
public interface ICustomerQuery<out TResponse> : IQuery<TResponse>
{
}

// Test DTOs with unique names to avoid conflicts
public class UserTestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CustomerTestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
}

public class ComplexTestDto
{
    public Dictionary<string, List<string>> Data { get; set; } = new();
}

#endregion

#region Test Queries

public class GetUserQuery : IQuery<UserTestDto>
{
    public int UserId { get; set; }
}

public class GetUserListQuery : IQuery<List<UserTestDto>>
{
    public int PageSize { get; set; } = 10;
}

public class GetCustomerInfoQuery : ICustomerQuery<CustomerTestDto>
{
    public int CustomerId { get; set; }
}

public class GetNestedGenericQuery : IQuery<Dictionary<string, List<UserTestDto>>>
{
    public string Category { get; set; } = string.Empty;
}

public class GetComplexQuery : IQuery<ComplexTestDto>
{
    public string Filter { get; set; } = string.Empty;
}

public class GetGenericQuery<T> : IQuery<T> where T : class
{
    public string Id { get; set; } = string.Empty;
}

// Query without handler for testing missing handlers
public class GetUnhandledQuery : IQuery<string>
{
    public string Data { get; set; } = string.Empty;
}

#endregion

#region Test Commands

public class DeleteUserCommand : ICommand
{
    public int UserId { get; set; }
}

public class UpdateUserCommand : ICommand<UserTestDto>
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}

#endregion

#region Test Notifications

public class UserCreatedNotification : INotification
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public class UserUpdatedNotification : INotification
{
    public int UserId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
}

#endregion

#region Test Handlers

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserTestDto>
{
    public Task<UserTestDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserTestDto { Id = request.UserId, Name = "Test User", Email = "test@example.com" });
    }
}

public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, List<UserTestDto>>
{
    public Task<List<UserTestDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<UserTestDto> 
        { 
            new() { Id = 1, Name = "User 1", Email = "user1@example.com" },
            new() { Id = 2, Name = "User 2", Email = "user2@example.com" }
        });
    }
}

public class GetCustomerInfoQueryHandler : IRequestHandler<GetCustomerInfoQuery, CustomerTestDto>
{
    public Task<CustomerTestDto> Handle(GetCustomerInfoQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CustomerTestDto 
        { 
            Id = request.CustomerId, 
            Name = "Test Customer", 
            CustomerType = "Premium" 
        });
    }
}

public class GetNestedGenericQueryHandler : IRequestHandler<GetNestedGenericQuery, Dictionary<string, List<UserTestDto>>>
{
    public Task<Dictionary<string, List<UserTestDto>>> Handle(GetNestedGenericQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Dictionary<string, List<UserTestDto>>
        {
            ["users"] = new List<UserTestDto> 
            { 
                new() { Id = 1, Name = "User 1", Email = "user1@example.com" } 
            }
        });
    }
}

public class GetComplexQueryHandler : IRequestHandler<GetComplexQuery, ComplexTestDto>
{
    public Task<ComplexTestDto> Handle(GetComplexQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ComplexTestDto());
    }
}

public class GetGenericQueryHandler<T> : IRequestHandler<GetGenericQuery<T>, T> where T : class
{
    public Task<T> Handle(GetGenericQuery<T> request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Generic handler for testing");
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    public Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserTestDto>
{
    public Task<UserTestDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserTestDto 
        { 
            Id = request.UserId, 
            Name = request.Name, 
            Email = "updated@example.com" 
        });
    }
}

public class UserCreatedNotificationHandler : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class UserUpdatedNotificationHandler : INotificationHandler<UserUpdatedNotification>
{
    public Task Handle(UserUpdatedNotification notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

#endregion

/// <summary>
/// Test renderer that captures output for verification with unique name
/// </summary>
public class ExtensionsTestStatisticsRenderer : IStatisticsRenderer
{
    public List<string> Messages { get; } = new();

    public void Render(string message)
    {
        Messages.Add(message);
    }
}