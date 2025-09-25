using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.NotificationHandlers;

/// <summary>
/// Tests for automatic discovery and registration of INotificationHandler implementations.
/// </summary>
public class NotificationHandlerDiscoveryTests
{
    [Fact]
    public void AddMediator_WithAssembly_ShouldDiscoverAndRegisterNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(NotificationHandlerDiscoveryTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>();
        Assert.NotNull(handlers);
        Assert.Equal(2, handlers.Count());

        // Verify specific handler types are registered
        var handlerTypes = handlers.Select(h => h.GetType()).ToList();
        Assert.Contains(typeof(FirstTestNotificationHandler), handlerTypes);
        Assert.Contains(typeof(SecondTestNotificationHandler), handlerTypes);
    }

    [Fact]
    public void AddMediator_WithoutNotificationHandlerDiscovery_ShouldNotRegisterNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config => config.WithoutNotificationHandlerDiscovery(), typeof(NotificationHandlerDiscoveryTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>();
        Assert.NotNull(handlers);
        Assert.Empty(handlers);
    }

    [Fact]
    public void AddMediator_WithNotificationHandlerDiscovery_ShouldAllowMultipleHandlersPerNotification()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(config => config.WithNotificationHandlerDiscovery(), typeof(NotificationHandlerDiscoveryTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Multiple handlers should be registered for the same notification
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>();
        Assert.True(handlers.Count() >= 2);
    }

    [Fact]
    public void AddMediator_DefaultConfiguration_ShouldDiscoverNotificationHandlersByDefault()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Default configuration should have notification handler discovery enabled
        services.AddMediator(typeof(NotificationHandlerDiscoveryTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handlers = serviceProvider.GetServices<INotificationHandler<TestNotification>>();
        Assert.True(handlers.Count() > 0);
    }

    [Fact]
    public async Task Mediator_WithDiscoveredHandlers_ShouldCallAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(typeof(NotificationHandlerDiscoveryTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = CreateMediatorWithDependencies(serviceProvider);
        
        FirstTestNotificationHandler.CallCount = 0;
        SecondTestNotificationHandler.CallCount = 0;

        var notification = new TestNotification { Message = "Test message" };

        // Act
        await mediator.Publish(notification);

        // Assert
        Assert.Equal(1, FirstTestNotificationHandler.CallCount);
        Assert.Equal(1, SecondTestNotificationHandler.CallCount);
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

/// <summary>
/// Test notification for handler discovery tests.
/// </summary>
public class TestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// First test notification handler for testing automatic discovery.
/// </summary>
public class FirstTestNotificationHandler : INotificationHandler<TestNotification>
{
    public static int CallCount = 0;

    public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Second test notification handler for testing automatic discovery and multiple handlers per notification.
/// </summary>
public class SecondTestNotificationHandler : INotificationHandler<TestNotification>
{
    public static int CallCount = 0;

    public Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}