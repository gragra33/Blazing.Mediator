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

    public ValueTask InvokeAsync(ITestOrderNotification notification, NotificationDelegate<ITestOrderNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    ValueTask INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class TestCustomerConstrainedMiddleware : INotificationMiddleware<ITestCustomerNotification>
{
    public int Order => 60;

    public ValueTask InvokeAsync(ITestCustomerNotification notification, NotificationDelegate<ITestCustomerNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    ValueTask INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class TestAuditConstrainedMiddleware : INotificationMiddleware<ITestAuditableNotification>
{
    public int Order => 100;

    public ValueTask InvokeAsync(ITestAuditableNotification notification, NotificationDelegate<ITestAuditableNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    ValueTask INotificationMiddleware.InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
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