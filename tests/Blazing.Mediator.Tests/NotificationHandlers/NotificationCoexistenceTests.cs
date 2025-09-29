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

        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedNotification = notification;
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Additional handler to test multiple handlers for same notification.
    /// </summary>
    public class AdditionalNotificationHandler : INotificationHandler<TestNotification>
    {
        public bool WasCalled { get; private set; }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Publish_WithBothSubscriberAndHandler_CallsBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        var subscriber = new TestNotificationSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - Both subscriber and handler should be called
        Assert.True(subscriber.WasCalled);
        Assert.Equal(notification, subscriber.ReceivedNotification);

        var handler = serviceProvider.GetRequiredService<INotificationHandler<TestNotification>>();
        var typedHandler = (TestNotificationHandler)handler;
        Assert.True(typedHandler.WasCalled);
        Assert.Equal(notification, typedHandler.ReceivedNotification);
    }

    [Fact]
    public async Task Publish_WithMultipleHandlers_CallsAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddScoped<INotificationHandler<TestNotification>, AdditionalNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - All handlers should be called
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>().ToList();
        Assert.Equal(2, handlers.Count);

        var firstHandler = (TestNotificationHandler)handlers.First(h => h is TestNotificationHandler);
        var secondHandler = (AdditionalNotificationHandler)handlers.First(h => h is AdditionalNotificationHandler);

        Assert.True(firstHandler.WasCalled);
        Assert.Equal(notification, firstHandler.ReceivedNotification);
        Assert.True(secondHandler.WasCalled);
    }

    [Fact]
    public async Task Publish_WithOnlyHandlers_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - Handler should be called
        var handler = serviceProvider.GetRequiredService<INotificationHandler<TestNotification>>();
        var typedHandler = (TestNotificationHandler)handler;
        Assert.True(typedHandler.WasCalled);
        Assert.Equal(notification, typedHandler.ReceivedNotification);
    }

    [Fact]
    public async Task Publish_WithOnlySubscribers_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        var subscriber = new TestNotificationSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification("Test message");

        // Act
        await mediator.Publish(notification);

        // Assert - Subscriber should be called
        Assert.True(subscriber.WasCalled);
        Assert.Equal(notification, subscriber.ReceivedNotification);
    }

    [Fact]
    public async Task Publish_WithNoHandlersOrSubscribers_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        var notification = new TestNotification("Test message");

        // Act & Assert - Should not throw
        await mediator.Publish(notification);
    }

    [Fact]
    public async Task Publish_HandlerException_DoesNotPreventOtherProcessors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<INotificationHandler<TestNotification>, ExceptionThrowingHandler>();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);

        var subscriber = new TestNotificationSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification("Test message");

        // Act & Assert - Should throw (first exception is propagated)
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Publish(notification));

        // Assert - Other processors should still have been called
        Assert.True(subscriber.WasCalled);

        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>().ToList();
        var workingHandler = (TestNotificationHandler)handlers.First(h => h is TestNotificationHandler);
        Assert.True(workingHandler.WasCalled);
    }

    /// <summary>
    /// Exception throwing handler for error handling tests.
    /// </summary>
    public class ExceptionThrowingHandler : INotificationHandler<TestNotification>
    {
        public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Test exception from handler");
        }
    }

    /// <summary>
    /// Creates a mediator with all required dependencies for testing.
    /// </summary>
    private static IMediator CreateMediatorWithDependencies(IServiceProvider serviceProvider)
    {
        // Create minimal dependencies for mediator
        var pipelineBuilder = new Blazing.Mediator.Pipeline.MiddlewarePipelineBuilder();
        var notificationPipelineBuilder = new Blazing.Mediator.Pipeline.NotificationPipelineBuilder();

        return new Mediator(serviceProvider, pipelineBuilder, notificationPipelineBuilder, null);
    }
}