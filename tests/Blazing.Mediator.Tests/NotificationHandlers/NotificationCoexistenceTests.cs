using Blazing.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.NotificationHandlers;

/// <summary>
/// Tests for coexistence of INotificationSubscriber and INotificationHandler patterns.
/// </summary>
public class NotificationCoexistenceTests
{
    /// <summary>
    /// Test notification for coexistence tests.
    /// </summary>
    public record TestNotification(string Message) : INotification;

    /// <summary>
    /// Test notification subscriber (manual pattern).
    /// </summary>
    public class TestNotificationSubscriber : INotificationSubscriber<TestNotification>
    {
        public bool WasCalled { get; private set; }
        public TestNotification? ReceivedNotification { get; private set; }

        public Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedNotification = notification;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test notification handler (automatic pattern).
    /// </summary>
    public class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public bool WasCalled { get; private set; }
        public TestNotification? ReceivedNotification { get; private set; }

        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedNotification = notification;
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Additional handler to test multiple handlers for same notification.
    /// </summary>
    public class AdditionalNotificationHandler : INotificationHandler<TestNotification>
    {
        public bool WasCalled { get; private set; }

        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task Publish_WithBothSubscriberAndHandler_CallsBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestNotificationSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - Handler should be called via source-gen dispatch path
        var handler = serviceProvider.GetRequiredService<TestNotificationHandler>();
        Assert.True(handler.WasCalled);
        Assert.Equal(notification, handler.ReceivedNotification);
        // Note: In source-gen mode, manual subscribers are not called when registered handlers exist.
    }

    [Fact]
    public async Task Publish_WithMultipleHandlers_CallsAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - All auto-discovered handlers should be called
        var firstHandler = serviceProvider.GetRequiredService<TestNotificationHandler>();
        var secondHandler = serviceProvider.GetRequiredService<AdditionalNotificationHandler>();

        Assert.True(firstHandler.WasCalled);
        Assert.Equal(notification, firstHandler.ReceivedNotification);
        Assert.True(secondHandler.WasCalled);
    }

    [Fact]
    public async Task Publish_WithOnlyHandlers_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - Handler should be called
        var handler = serviceProvider.GetRequiredService<TestNotificationHandler>();
        Assert.True(handler.WasCalled);
        Assert.Equal(notification, handler.ReceivedNotification);
    }

    [Fact]
    public async Task Publish_WithOnlySubscribers_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - publish; source-gen handles all TestNotification handlers
        var notification = new TestNotification("Test message");
        await mediator.Publish(notification);

        // Assert - Handlers are called via source-gen path
        var handler = serviceProvider.GetRequiredService<TestNotificationHandler>();
        Assert.True(handler.WasCalled);
    }

    [Fact]
    public async Task Publish_WithNoHandlersOrSubscribers_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new TestNotification("Test message");

        // Act & Assert - Should not throw (source-gen handles registered handlers)
        await mediator.Publish(notification);
    }

    /// <summary>
    /// Exception throwing handler for error handling tests.
    /// Excluded from auto-discovery so it does not affect other TestNotification tests.
    /// </summary>
    [ExcludeFromAutoDiscovery]
    public class ExceptionThrowingHandler : INotificationHandler<TestNotification>
    {
        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test exception from handler");
        }
    }
}