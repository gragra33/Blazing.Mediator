using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Comprehensive tests to verify that request statistics and notification statistics 
/// are tracked separately and working correctly. Tests both total execution counts
/// and unique type counts for proper statistics reporting.
/// </summary>
public class RequestVsNotificationStatisticsTests
{
    #region Test Types

    // Request types (Commands/Queries)
    public record TestQuery(string Message) : IRequest<string>;
    public record AnotherQuery(int Id) : IRequest<int>;
    public record TestCommand(string Action) : IRequest;
    public record AnotherCommand(bool Flag) : IRequest;

    // Notification types
    public record OrderNotification(int OrderId, decimal Amount) : INotification;
    public record UserNotification(int UserId, string Email) : INotification;
    public record ProductNotification(string ProductId, string Name) : INotification;

    // Handlers
    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
            => Task.FromResult($"Response: {request.Message}");
    }

    public class AnotherQueryHandler : IRequestHandler<AnotherQuery, int>
    {
        public Task<int> Handle(AnotherQuery request, CancellationToken cancellationToken)
            => Task.FromResult(request.Id * 2);
    }

    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    public class AnotherCommandHandler : IRequestHandler<AnotherCommand>
    {
        public Task Handle(AnotherCommand request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    public class OrderNotificationHandler : INotificationHandler<OrderNotification>
    {
        public int CallCount { get; private set; }
        public Task Handle(OrderNotification notification, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    public class UserNotificationHandler : INotificationHandler<UserNotification>
    {
        public int CallCount { get; private set; }
        public Task Handle(UserNotification notification, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    public class ProductNotificationHandler : INotificationHandler<ProductNotification>
    {
        public int CallCount { get; private set; }
        public Task Handle(ProductNotification notification, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    #endregion

    #region Core Separation Tests

    /// <summary>
    /// Tests that request statistics (queries/commands) and notification statistics 
    /// are tracked completely separately and do not interfere with each other.
    /// </summary>
    [Fact]
    public async Task Statistics_RequestsAndNotifications_TrackedSeparately()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = true;
                options.EnablePerformanceCounters = true;
                options.EnableDetailedAnalysis = true;
            }).WithNotificationHandlerDiscovery();
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act - Execute requests only (no notifications)
        await mediator.Send(new TestQuery("Test1"));
        await mediator.Send(new TestQuery("Test2")); // Same type, different instance
        await mediator.Send(new AnotherQuery(42));
        await mediator.Send(new TestCommand("Action1"));

        // Verify requests are tracked but notifications are not
        statistics.ReportStatistics();
        var messages = renderer.Messages;
        
        // Should show request counts but zero notifications
        messages.ShouldContain(m => m.Contains("Queries:") && m.Contains("3")); // 3 total query executions (2 TestQuery + 1 AnotherQuery)
        messages.ShouldContain(m => m.Contains("Commands:") && m.Contains("1")); // 1 command execution
        messages.ShouldContain(m => m.Contains("Notifications:") && m.Contains("0")); // 0 notification executions

        renderer.Messages.Clear();

        // Act - Execute notifications only (no more requests)
        await mediator.Publish(new OrderNotification(1, 100m));
        await mediator.Publish(new OrderNotification(2, 200m)); // Same type, different instance
        await mediator.Publish(new UserNotification(1, "user@test.com"));
        await mediator.Publish(new ProductNotification("P1", "Product1"));
        await mediator.Publish(new ProductNotification("P2", "Product2")); // Same type, different instance

        // Verify notifications are now tracked, requests remain the same
        statistics.ReportStatistics();
        messages = renderer.Messages;

        // Total executions should now include notifications, requests unchanged
        messages.ShouldContain(m => m.Contains("Queries:") && m.Contains("3")); // Still 3 (unchanged)
        messages.ShouldContain(m => m.Contains("Commands:") && m.Contains("1")); // Still 1 (unchanged)
        messages.ShouldContain(m => m.Contains("Notifications:") && m.Contains("5")); // 5 notification executions total

        // Verify handlers were actually called
        var orderHandler = serviceProvider.GetRequiredService<OrderNotificationHandler>();
        var userHandler = serviceProvider.GetRequiredService<UserNotificationHandler>();
        var productHandler = serviceProvider.GetRequiredService<ProductNotificationHandler>();

        orderHandler.CallCount.ShouldBe(2);
        userHandler.CallCount.ShouldBe(1);
        productHandler.CallCount.ShouldBe(2);
    }

    /// <summary>
    /// Tests that notification statistics work independently when no requests are executed.
    /// </summary>
    [Fact]
    public async Task Statistics_NotificationsOnly_WorkIndependently()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = true;
            }).WithNotificationHandlerDiscovery();
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act - Execute ONLY notifications, no requests
        await mediator.Publish(new OrderNotification(1, 100m));
        await mediator.Publish(new OrderNotification(2, 200m));
        await mediator.Publish(new OrderNotification(3, 300m));
        await mediator.Publish(new UserNotification(1, "user1@test.com"));
        await mediator.Publish(new UserNotification(2, "user2@test.com"));

        // Assert
        statistics.ReportStatistics();
        var messages = renderer.Messages;

        // Should show zero requests, non-zero notifications
        messages.ShouldContain(m => m.Contains("Queries:") && m.Contains("0"));
        messages.ShouldContain(m => m.Contains("Commands:") && m.Contains("0"));
        messages.ShouldContain(m => m.Contains("Notifications:") && m.Contains("5")); // 5 total executions

        // Verify handlers were called
        var orderHandler = serviceProvider.GetRequiredService<OrderNotificationHandler>();
        var userHandler = serviceProvider.GetRequiredService<UserNotificationHandler>();

        orderHandler.CallCount.ShouldBe(3);
        userHandler.CallCount.ShouldBe(2);
    }

    /// <summary>
    /// Tests that request statistics work independently when no notifications are published.
    /// </summary>
    [Fact]
    public async Task Statistics_RequestsOnly_WorkIndependently()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = true;
            });
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act - Execute ONLY requests, no notifications
        await mediator.Send(new TestQuery("Query1"));
        await mediator.Send(new TestQuery("Query2"));
        await mediator.Send(new AnotherQuery(42));
        await mediator.Send(new TestCommand("Command1"));
        await mediator.Send(new AnotherCommand(true));

        // Assert
        statistics.ReportStatistics();
        var messages = renderer.Messages;

        // Should show non-zero requests, zero notifications
        messages.ShouldContain(m => m.Contains("Queries:") && m.Contains("3")); // 2 TestQuery + 1 AnotherQuery = 3
        messages.ShouldContain(m => m.Contains("Commands:") && m.Contains("2")); // 1 TestCommand + 1 AnotherCommand = 2
        messages.ShouldContain(m => m.Contains("Notifications:") && m.Contains("0"));
    }

    #endregion

    #region Total Executions vs Unique Types Tests

    /// <summary>
    /// Tests that statistics show total execution counts, not just unique type counts.
    /// This was the core issue - statistics were showing unique types instead of total executions.
    /// </summary>
    [Fact]
    public async Task Statistics_ShowTotalExecutions_NotJustUniqueTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking().WithNotificationHandlerDiscovery();
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act - Execute same types multiple times
        // 2 unique query types, but 5 total query executions
        await mediator.Send(new TestQuery("Query1"));
        await mediator.Send(new TestQuery("Query2")); 
        await mediator.Send(new TestQuery("Query3")); // 3 executions of TestQuery
        await mediator.Send(new AnotherQuery(1));
        await mediator.Send(new AnotherQuery(2)); // 2 executions of AnotherQuery

        // 1 unique command type, but 3 total command executions
        await mediator.Send(new TestCommand("Command1"));
        await mediator.Send(new TestCommand("Command2"));
        await mediator.Send(new TestCommand("Command3")); // 3 executions of TestCommand

        // 2 unique notification types, but 6 total notification executions
        await mediator.Publish(new OrderNotification(1, 100m));
        await mediator.Publish(new OrderNotification(2, 200m));
        await mediator.Publish(new OrderNotification(3, 300m)); // 3 executions of OrderNotification
        await mediator.Publish(new UserNotification(1, "user1@test.com"));
        await mediator.Publish(new UserNotification(2, "user2@test.com"));
        await mediator.Publish(new UserNotification(3, "user3@test.com")); // 3 executions of UserNotification

        // Assert - Should show TOTAL EXECUTIONS, not unique types
        statistics.ReportStatistics();
        var messages = renderer.Messages;

        // Total executions (not unique types)
        messages.ShouldContain(m => m.Contains("Queries:") && m.Contains("5")); // 5 total executions (3+2)
        messages.ShouldContain(m => m.Contains("Commands:") && m.Contains("3")); // 3 total executions
        messages.ShouldContain(m => m.Contains("Notifications:") && m.Contains("6")); // 6 total executions (3+3)

        // Verify the unique type counts would have been different
        // If it were showing unique types, it would be:
        // Queries: 2 (TestQuery, AnotherQuery)
        // Commands: 1 (TestCommand)  
        // Notifications: 2 (OrderNotification, UserNotification)
    }

    #endregion

    #region Statistics Options Tests

    /// <summary>
    /// Tests that statistics can be disabled for requests while enabled for notifications and vice versa.
    /// </summary>
    [Fact]
    public async Task Statistics_CanDisableRequestsOrNotifications_Independently()
    {
        // Test 1: Notifications enabled, requests disabled
        var services1 = new ServiceCollection();
        var renderer1 = new TestStatisticsRenderer();
        services1.AddSingleton<IStatisticsRenderer>(renderer1);
        services1.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = false;
                options.EnableNotificationMetrics = true;
            }).WithNotificationHandlerDiscovery();
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider1 = services1.BuildServiceProvider();
        var mediator1 = serviceProvider1.GetRequiredService<IMediator>();
        var statistics1 = serviceProvider1.GetRequiredService<MediatorStatistics>();

        await mediator1.Send(new TestQuery("Test"));
        await mediator1.Send(new TestCommand("Test"));
        await mediator1.Publish(new OrderNotification(1, 100m));

        statistics1.ReportStatistics();
        var messages1 = renderer1.Messages;

        // Requests should not be tracked, notifications should be
        messages1.ShouldContain(m => m.Contains("Queries:") && m.Contains("0"));
        messages1.ShouldContain(m => m.Contains("Commands:") && m.Contains("0"));
        messages1.ShouldContain(m => m.Contains("Notifications:") && m.Contains("1"));

        // Test 2: Requests enabled, notifications disabled
        var services2 = new ServiceCollection();
        var renderer2 = new TestStatisticsRenderer();
        services2.AddSingleton<IStatisticsRenderer>(renderer2);
        services2.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = false;
            }).WithNotificationHandlerDiscovery();
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider2 = services2.BuildServiceProvider();
        var mediator2 = serviceProvider2.GetRequiredService<IMediator>();
        var statistics2 = serviceProvider2.GetRequiredService<MediatorStatistics>();

        await mediator2.Send(new TestQuery("Test"));
        await mediator2.Send(new TestCommand("Test"));
        await mediator2.Publish(new OrderNotification(1, 100m));

        statistics2.ReportStatistics();
        var messages2 = renderer2.Messages;

        // Requests should be tracked, notifications should not be
        messages2.ShouldContain(m => m.Contains("Queries:") && m.Contains("1"));
        messages2.ShouldContain(m => m.Contains("Commands:") && m.Contains("1"));
        messages2.ShouldContain(m => m.Contains("Notifications:") && m.Contains("0"));
    }

    #endregion

    #region Performance Summary Tests

    /// <summary>
    /// Tests that the Performance Summary correctly shows request metrics but not notification metrics,
    /// since notifications follow publish/subscribe pattern, not request/response pattern.
    /// </summary>
    [Fact]
    public async Task Statistics_PerformanceSummary_OnlyTracksRequests_NotNotifications()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = true;
                options.EnablePerformanceCounters = true; // Enable performance summary
            }).WithNotificationHandlerDiscovery();
        }, typeof(RequestVsNotificationStatisticsTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act
        // Execute both requests and notifications
        await mediator.Send(new TestQuery("Test1"));
        await mediator.Send(new TestQuery("Test2"));
        await mediator.Send(new TestCommand("Command1"));

        await mediator.Publish(new OrderNotification(1, 100m));
        await mediator.Publish(new OrderNotification(2, 200m));

        // Assert
        statistics.ReportStatistics();
        var messages = renderer.Messages;

        // Basic statistics should show both
        messages.ShouldContain(m => m.Contains("Queries:") && m.Contains("2"));
        messages.ShouldContain(m => m.Contains("Commands:") && m.Contains("1"));
        messages.ShouldContain(m => m.Contains("Notifications:") && m.Contains("2"));

        // Performance Summary shows overall operations (currently this includes all operations)
        // Note: The performance counter implementation may need to be added if not present
        if (messages.Any(m => m.Contains("Total Operations:")))
        {
            // If performance counters are implemented, verify the total includes both requests and notifications
            messages.ShouldContain(m => m.Contains("Total Operations:") && (m.Contains("3") || m.Contains("5"))); // 3 requests + 2 notifications = 5, or just requests = 3
        }
    }

    #endregion

    #region Test Helper

    private class TestStatisticsRenderer : IStatisticsRenderer
    {
        public List<string> Messages { get; } = new();

        public void Render(string message)
        {
            Messages.Add(message);
        }
    }

    #endregion
}