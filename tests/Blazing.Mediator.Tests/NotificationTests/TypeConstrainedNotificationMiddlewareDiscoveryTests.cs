using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static Blazing.Mediator.Tests.NotificationTests.NotificationMiddlewareTests;

namespace Blazing.Mediator.Tests.NotificationTests;

/// <summary>
/// Tests for type-constrained notification middleware auto-discovery functionality.
/// </summary>
public class TypeConstrainedNotificationMiddlewareDiscoveryTests
{
    private readonly Assembly _testAssembly = typeof(TypeConstrainedNotificationMiddlewareDiscoveryTests).Assembly;

    /// <summary>
    /// Test that auto-discovery finds INotificationMiddleware&lt;T&gt; implementations
    /// </summary>
    [Fact]
    public void AutoDiscovery_FindsConstrainedNotificationMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Enable auto-discovery of notification middleware
        services.AddMediator(config =>
        {
            config.WithNotificationMiddlewareDiscovery();
        }, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var middlewareTypes = inspector.GetRegisteredMiddleware();

        // Should find the test constrained middleware
        middlewareTypes.ShouldContain(typeof(TestOrderConstrainedMiddleware));
        middlewareTypes.ShouldContain(typeof(TestCustomerConstrainedMiddleware));
        middlewareTypes.ShouldContain(typeof(TestAuditConstrainedMiddleware));
        
        // Should also find regular notification middleware
        middlewareTypes.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test that constrained middleware is properly registered with correct ordering
    /// </summary>
    [Fact]
    public void ConstrainedMiddleware_HasCorrectOrdering()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            config.WithNotificationMiddlewareDiscovery();
        }, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Find our test middleware and verify ordering
        var orderMiddleware = analysis.FirstOrDefault(m => m.Type == typeof(TestOrderConstrainedMiddleware));
        var customerMiddleware = analysis.FirstOrDefault(m => m.Type == typeof(TestCustomerConstrainedMiddleware));
        var auditMiddleware = analysis.FirstOrDefault(m => m.Type == typeof(TestAuditConstrainedMiddleware));

        orderMiddleware?.Order.ShouldBe(50);
        customerMiddleware?.Order.ShouldBe(60);
        auditMiddleware?.Order.ShouldBe(100);
    }

    /// <summary>
    /// Test that constrained and non-constrained middleware can coexist
    /// </summary>
    [Fact]
    public void ConstrainedAndNonConstrainedMiddleware_CanCoexist()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config =>
        {
            // Manually add regular middleware
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
            // Auto-discover constrained middleware
            config.WithNotificationMiddlewareDiscovery();
        }, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var middlewareTypes = inspector.GetRegisteredMiddleware();

        // Should have both types
        middlewareTypes.ShouldContain(typeof(LoggingNotificationMiddleware)); // Regular (may appear twice - manual + auto)
        middlewareTypes.ShouldContain(typeof(TestOrderConstrainedMiddleware)); // Constrained
        middlewareTypes.ShouldContain(typeof(TestCustomerConstrainedMiddleware)); // Constrained
    }
}

// Test constraint interfaces
public interface ITestOrderNotification : INotification
{
    int OrderId { get; }
}

public interface ITestCustomerNotification : INotification  
{
    int CustomerId { get; }
}

public interface ITestAuditableNotification : INotification
{
    string EntityId { get; }
    string Action { get; }
    DateTime Timestamp { get; }
}

// Test constrained middleware implementations for auto-discovery
public class TestOrderConstrainedMiddleware : INotificationMiddleware<ITestOrderNotification>
{
    public int Order => 50;

    public Task InvokeAsync(ITestOrderNotification notification, NotificationDelegate<ITestOrderNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    Task INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class TestCustomerConstrainedMiddleware : INotificationMiddleware<ITestCustomerNotification>
{
    public int Order => 60;

    public Task InvokeAsync(ITestCustomerNotification notification, NotificationDelegate<ITestCustomerNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    Task INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class TestAuditConstrainedMiddleware : INotificationMiddleware<ITestAuditableNotification>
{
    public int Order => 100;

    public Task InvokeAsync(ITestAuditableNotification notification, NotificationDelegate<ITestAuditableNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    Task INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

// Test notification implementations
public class TestOrderCreatedNotification : ITestOrderNotification, ITestAuditableNotification
{
    public int OrderId { get; set; }
    public string EntityId => OrderId.ToString();
    public string Action => "OrderCreated";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class TestCustomerRegisteredNotification : ITestCustomerNotification, ITestAuditableNotification
{
    public int CustomerId { get; set; }
    public string EntityId => CustomerId.ToString();
    public string Action => "CustomerRegistered";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}