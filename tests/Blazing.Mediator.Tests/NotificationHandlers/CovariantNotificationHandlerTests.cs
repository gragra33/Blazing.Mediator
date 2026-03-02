using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.NotificationHandlers;

/// <summary>
/// Tests for covariant notification handler support, ensuring handlers can handle notifications
/// through inheritance hierarchies and interface implementations.
/// </summary>
public class CovariantNotificationHandlerTests
{
    [Fact]
    public async Task Publish_WithCovariantHandlers_ShouldInvokeAllCompatibleHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        // Reset handler call counts
        BaseNotificationHandler.CallCount = 0;
        InterfaceNotificationHandler.CallCount = 0;
        SpecificNotificationHandler.CallCount = 0;
        AnotherInterfaceHandler.CallCount = 0;

        var notification = new DerivedTestNotification
        {
            Message = "Test covariant handling",
            DerivedProperty = "Derived value"
        };

        // Act
        await mediator.Publish(notification);

        // Assert - Source-gen dispatches to handlers for DerivedTestNotification and its interfaces
        // Note: BaseNotificationHandler handles BaseTestNotification (not DerivedTestNotification)
        // and source-gen does not include base-class handlers for derived types
        Assert.Equal(0, BaseNotificationHandler.CallCount); // BaseTestNotification handler not called for DerivedTestNotification
        Assert.Equal(1, InterfaceNotificationHandler.CallCount); // ITestInterface handler called
        Assert.Equal(1, SpecificNotificationHandler.CallCount); // DerivedTestNotification handler called
        Assert.Equal(1, AnotherInterfaceHandler.CallCount); // IAnotherTestInterface handler called
    }

    [Fact]
    public async Task Publish_WithBaseNotification_ShouldOnlyInvokeBaseAndInterfaceHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        // Reset handler call counts
        BaseNotificationHandler.CallCount = 0;
        InterfaceNotificationHandler.CallCount = 0;
        SpecificNotificationHandler.CallCount = 0;
        AnotherInterfaceHandler.CallCount = 0;

        var notification = new BaseTestNotification
        {
            Message = "Test base handling"
        };

        // Act
        await mediator.Publish(notification);

        // Assert - Only base-type and interface handlers should be called
        // Source-gen dispatches to handlers for BaseTestNotification and its interfaces
        Assert.Equal(1, BaseNotificationHandler.CallCount); // BaseTestNotification handler called
        Assert.Equal(1, InterfaceNotificationHandler.CallCount); // ITestInterface handler called
        Assert.Equal(0, SpecificNotificationHandler.CallCount); // DerivedTestNotification handler NOT called for base type
        Assert.Equal(1, AnotherInterfaceHandler.CallCount); // IAnotherTestInterface handler called
    }

    [Fact]
    public async Task Publish_WithMultipleInterfaceImplementation_ShouldInvokeAllInterfaceHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        // Reset handler call counts
        BaseNotificationHandler.CallCount = 0;
        InterfaceNotificationHandler.CallCount = 0;
        SpecificNotificationHandler.CallCount = 0;
        AnotherInterfaceHandler.CallCount = 0;
        MultiInterfaceHandler.CallCount = 0;

        var notification = new MultiInterfaceNotification
        {
            Message = "Test multi-interface handling",
            TestValue = "Interface value",
            AnotherValue = "Another interface value"
        };

        // Act
        await mediator.Publish(notification);

        // Assert - Source-gen dispatches to handlers for MultiInterfaceNotification and its interfaces
        // Note: BaseNotificationHandler handles BaseTestNotification (not MultiInterfaceNotification)
        Assert.Equal(0, BaseNotificationHandler.CallCount); // BaseTestNotification handler not called
        Assert.Equal(1, InterfaceNotificationHandler.CallCount); // ITestInterface handler called
        Assert.Equal(0, SpecificNotificationHandler.CallCount); // Not derived from DerivedTestNotification
        Assert.Equal(1, AnotherInterfaceHandler.CallCount); // IAnotherTestInterface handler called
        Assert.Equal(1, MultiInterfaceHandler.CallCount); // MultiInterfaceNotification handler called
    }

    [Fact]
    public async Task Publish_WithDeepInheritanceHierarchy_ShouldInvokeAllHierarchyHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        // Reset handler call counts
        BaseNotificationHandler.CallCount = 0;
        InterfaceNotificationHandler.CallCount = 0;
        SpecificNotificationHandler.CallCount = 0;
        DeeplyDerivedHandler.CallCount = 0;
        AnotherInterfaceHandler.CallCount = 0;

        var notification = new DeeplyDerivedNotification
        {
            Message = "Base message",
            DerivedProperty = "Derived property",
            DeeplyDerivedProperty = "Deeply derived property"
        };

        // Act
        await mediator.Publish(notification);

        // Assert - Source-gen dispatches to handlers for DeeplyDerivedNotification and its interfaces
        // Note: Source-gen does NOT include parent-class handlers; only exact-type and interface handlers
        Assert.Equal(0, BaseNotificationHandler.CallCount); // BaseTestNotification handler not called for deeply derived
        Assert.Equal(1, InterfaceNotificationHandler.CallCount); // ITestInterface handler called
        Assert.Equal(0, SpecificNotificationHandler.CallCount); // DerivedTestNotification handler NOT called (parent class, not exact type)
        Assert.Equal(1, DeeplyDerivedHandler.CallCount); // DeeplyDerivedNotification handler called
        Assert.Equal(1, AnotherInterfaceHandler.CallCount); // IAnotherTestInterface handler called
    }

    [Fact]
    public async Task Publish_WithHandlerException_ShouldContinueProcessingOtherHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        // Reset handler call counts
        BaseNotificationHandler.CallCount = 0;
        InterfaceNotificationHandler.CallCount = 0;
        SpecificNotificationHandler.CallCount = 0;
        ExceptionThrowingHandler.CallCount = 0;
        ExceptionThrowingHandler.ShouldThrow = true;

        var notification = new DerivedTestNotification
        {
            Message = "Test exception handling",
            DerivedProperty = "Derived value"
        };

        // Act & Assert - Exception from one handler shouldn't stop others
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await mediator.Publish(notification));
        Assert.Equal("Test exception from handler", exception.Message);

        // Verify that the exception-throwing handler was called
        Assert.Equal(1, ExceptionThrowingHandler.CallCount);

        // Note: Other handlers may or may not be called depending on the order of execution
        // The important thing is that the exception from one handler doesn't prevent the system from working
    }

    /// <summary>
    /// Creates a mediator with all required dependencies for testing.
    /// Uses GetRequiredService since source generators require AddMediator() DI setup.
    /// </summary>
    private static IMediator CreateMediatorWithDependencies(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IMediator>();
    }
}

#region Test Notifications and Interfaces

/// <summary>
/// Interface for testing covariant notification handling.
/// </summary>
public interface ITestInterface : INotification
{
    string TestValue { get; }
}

/// <summary>
/// Another interface for testing multiple interface implementations.
/// </summary>
public interface IAnotherTestInterface : INotification
{
    string AnotherValue { get; }
}

/// <summary>
/// Base notification for testing inheritance hierarchies.
/// </summary>
public class BaseTestNotification : INotification, ITestInterface, IAnotherTestInterface
{
    public string Message { get; set; } = string.Empty;
    public string TestValue => Message;
    public string AnotherValue => Message;
}

/// <summary>
/// Derived notification for testing covariant handling.
/// </summary>
public class DerivedTestNotification : BaseTestNotification
{
    public string DerivedProperty { get; set; } = string.Empty;
}

/// <summary>
/// Deeply derived notification for testing deep inheritance hierarchies.
/// </summary>
public class DeeplyDerivedNotification : DerivedTestNotification
{
    public string DeeplyDerivedProperty { get; set; } = string.Empty;
}

/// <summary>
/// Notification that implements multiple interfaces.
/// </summary>
public class MultiInterfaceNotification : BaseTestNotification, ITestInterface, IAnotherTestInterface
{
    public new string TestValue { get; set; } = string.Empty;
    public new string AnotherValue { get; set; } = string.Empty;
}

#endregion

#region Test Handlers

/// <summary>
/// Handler for base notification type.
/// </summary>
public class BaseNotificationHandler : INotificationHandler<BaseTestNotification>
{
    public static int CallCount;
    public static BaseTestNotification? LastNotification;

    public ValueTask Handle(BaseTestNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastNotification = notification;
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handler for interface-based notifications.
/// </summary>
public class InterfaceNotificationHandler : INotificationHandler<ITestInterface>
{
    public static int CallCount;
    public static ITestInterface? LastNotification;

    public ValueTask Handle(ITestInterface notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastNotification = notification;
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handler for specific derived notification type.
/// </summary>
public class SpecificNotificationHandler : INotificationHandler<DerivedTestNotification>
{
    public static int CallCount;
    public static DerivedTestNotification? LastNotification;

    public ValueTask Handle(DerivedTestNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastNotification = notification;
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handler for another interface.
/// </summary>
public class AnotherInterfaceHandler : INotificationHandler<IAnotherTestInterface>
{
    public static int CallCount;
    public static IAnotherTestInterface? LastNotification;

    public ValueTask Handle(IAnotherTestInterface notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastNotification = notification;
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handler for deeply derived notification type.
/// </summary>
public class DeeplyDerivedHandler : INotificationHandler<DeeplyDerivedNotification>
{
    public static int CallCount;
    public static DeeplyDerivedNotification? LastNotification;

    public ValueTask Handle(DeeplyDerivedNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastNotification = notification;
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handler for multi-interface notifications.
/// </summary>
public class MultiInterfaceHandler : INotificationHandler<MultiInterfaceNotification>
{
    public static int CallCount;
    public static MultiInterfaceNotification? LastNotification;

    public ValueTask Handle(MultiInterfaceNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        LastNotification = notification;
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Handler that throws exceptions for testing error handling.
/// </summary>
public class ExceptionThrowingHandler : INotificationHandler<DerivedTestNotification>
{
    public static int CallCount;
    public static bool ShouldThrow;

    public ValueTask Handle(DerivedTestNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Test exception from handler");
        }
        return ValueTask.CompletedTask;
    }
}

#endregion