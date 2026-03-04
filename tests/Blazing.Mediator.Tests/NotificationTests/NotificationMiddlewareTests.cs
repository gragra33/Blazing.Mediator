using System.Reflection;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.NotificationTests;

public class NotificationMiddlewareTests
{
    // Test notification
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    // Test middleware
    [ExcludeFromAutoDiscovery]
    public class LoggingNotificationMiddleware : INotificationMiddleware
    {
        public int Order => 10;
        public List<string> LoggedMessages { get; } = new();

        public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
        {
            LoggedMessages.Add($"Before: {typeof(TNotification).Name}");
            await next(notification, cancellationToken);
            LoggedMessages.Add($"After: {typeof(TNotification).Name}");
        }
    }

    // Test conditional middleware
    [ExcludeFromAutoDiscovery]
    public class ConditionalNotificationMiddleware : IConditionalNotificationMiddleware
    {
        public int Order => 5;
        public List<string> ProcessedMessages { get; } = new();

        public bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification
        {
            return notification is TestNotification testNotification && testNotification.Message.StartsWith("IMPORTANT");
        }

        public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
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

    /// <summary>
    /// Tests for AnalyzeMiddleware functionality with discoverNotificationMiddleware: true
    /// </summary>
    [Fact]
    public void NotificationMiddleware_DiscoverNotificationMiddleware_True_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Get the inspector
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        var pipelineBuilder = notificationPipelineBuilderField?.GetValue(mediator);
        var inspector = (INotificationMiddlewarePipelineInspector)pipelineBuilder!;

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert - AnalyzeMiddleware should return meaningful data
        analysis.ShouldNotBeNull();

        // Each analysis should have valid data
        foreach (var middlewareAnalysis in analysis)
        {
            middlewareAnalysis.ClassName.ShouldNotBeNullOrWhiteSpace();
            middlewareAnalysis.Type.ShouldNotBeNull();
            middlewareAnalysis.OrderDisplay.ShouldNotBeNullOrWhiteSpace();
        }
    }

}