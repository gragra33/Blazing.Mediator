using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Reflection;

namespace Blazing.Mediator.Tests.NotificationTests;

public class NotificationMiddlewareTests
{
    // Test notification
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    // Test middleware
    public class LoggingNotificationMiddleware : INotificationMiddleware
    {
        public int Order => 10;
        public List<string> LoggedMessages { get; } = new();

        public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
        {
            LoggedMessages.Add($"Before: {typeof(TNotification).Name}");
            await next(notification, cancellationToken);
            LoggedMessages.Add($"After: {typeof(TNotification).Name}");
        }
    }

    // Test conditional middleware
    public class ConditionalNotificationMiddleware : IConditionalNotificationMiddleware
    {
        public int Order => 5;
        public List<string> ProcessedMessages { get; } = new();

        public bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification
        {
            return notification is TestNotification testNotification && testNotification.Message.StartsWith("IMPORTANT");
        }

        public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
        {
            ProcessedMessages.Add($"Processing: {typeof(TNotification).Name}");
            await next(notification, cancellationToken);
        }
    }

    // Test subscriber
    public class TestSubscriber : INotificationSubscriber<TestNotification>
    {
        public List<TestNotification> ReceivedNotifications { get; } = new();

        public Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
        {
            ReceivedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Notification_Should_Pass_Through_Middleware()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggingMiddleware = new LoggingNotificationMiddleware();
        
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
        }, Array.Empty<Assembly>());
        
        services.AddSingleton(loggingMiddleware);
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe<TestNotification>(subscriber);

        var notification = new TestNotification { Message = "Hello" };

        // Act
        await mediator.Publish(notification);

        // Assert
        subscriber.ReceivedNotifications.Count.ShouldBe(1);
        loggingMiddleware.LoggedMessages.Count.ShouldBe(2);
        loggingMiddleware.LoggedMessages[0].ShouldBe("Before: TestNotification");
        loggingMiddleware.LoggedMessages[1].ShouldBe("After: TestNotification");
    }

    [Fact]
    public async Task Conditional_Middleware_Should_Only_Execute_When_Condition_Met()
    {
        // Arrange
        var services = new ServiceCollection();
        var conditionalMiddleware = new ConditionalNotificationMiddleware();
        
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, Array.Empty<Assembly>());
        
        services.AddSingleton(conditionalMiddleware);
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe<TestNotification>(subscriber);

        // Act & Assert - should NOT execute for regular message
        var normalNotification = new TestNotification { Message = "Hello" };
        await mediator.Publish(normalNotification);

        conditionalMiddleware.ProcessedMessages.Count.ShouldBe(0);
        subscriber.ReceivedNotifications.Count.ShouldBe(1);

        // Act & Assert - SHOULD execute for important message
        var importantNotification = new TestNotification { Message = "IMPORTANT: System down" };
        await mediator.Publish(importantNotification);

        conditionalMiddleware.ProcessedMessages.Count.ShouldBe(1);
        conditionalMiddleware.ProcessedMessages[0].ShouldBe("Processing: TestNotification");
        subscriber.ReceivedNotifications.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Middleware_Should_Execute_In_Order()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggingMiddleware = new LoggingNotificationMiddleware();
        var conditionalMiddleware = new ConditionalNotificationMiddleware();
        
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>(); // Order 5
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>(); // Order 10
        }, Array.Empty<Assembly>());
        
        services.AddSingleton(loggingMiddleware);
        services.AddSingleton(conditionalMiddleware);
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe<TestNotification>(subscriber);

        // Act
        var notification = new TestNotification { Message = "IMPORTANT: Test" };
        await mediator.Publish(notification);

        // Assert
        // Conditional middleware (Order 5) should execute first, then logging middleware (Order 10)
        conditionalMiddleware.ProcessedMessages.Count.ShouldBe(1);
        loggingMiddleware.LoggedMessages.Count.ShouldBe(2);
        
        // The logging middleware should log around the conditional middleware
        loggingMiddleware.LoggedMessages[0].ShouldBe("Before: TestNotification");
        loggingMiddleware.LoggedMessages[1].ShouldBe("After: TestNotification");
    }
}
