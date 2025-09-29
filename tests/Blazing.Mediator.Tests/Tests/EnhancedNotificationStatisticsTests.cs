using Blazing.Mediator.Statistics;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Enhanced
{
    /// <summary>
    /// Tests for the enhanced notification pattern detection and subscriber tracking functionality.
    /// Validates the fix for the issue where NotificationSubscriberExample reported "0 handlers" when subscribers were active.
    /// </summary>
    public class EnhancedNotificationStatisticsTests
    {
        /// <summary>
        /// Tests that subscriber tracking correctly detects active manual subscribers.
        /// This addresses the core issue where the NotificationSubscriberExample showed "0 handlers"
        /// even when subscribers were registered and working.
        /// </summary>
        [Fact]
        public void NotificationAnalysis_WithActiveSubscribers_DetectsManualSubscribersPattern()
        {
            // Arrange
            var services = new ServiceCollection();
            // Don't use assembly scanning to avoid auto-registering handlers
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
            services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
            services.AddSingleton<MediatorStatistics>();

            services.AddScoped<TestTypes.EnhancedTestSubscriber>();
            var serviceProvider = services.BuildServiceProvider();
            
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            var subscriber = serviceProvider.GetRequiredService<TestTypes.EnhancedTestSubscriber>();

            // Act - Subscribe to notifications
            mediator.Subscribe(subscriber);

            // Analyze notifications after subscription
            var notifications = statistics.AnalyzeNotifications(serviceProvider);
            var testNotification = notifications.FirstOrDefault(n => n.ClassName == nameof(TestTypes.EnhancedTestNotification));

            // Assert
            testNotification.ShouldNotBeNull("EnhancedTestNotification should be found in analysis");
            testNotification.Pattern.ShouldBe(NotificationPattern.ManualSubscribers, "Should detect Manual Subscribers pattern");
            testNotification.SubscriberStatus.ShouldBe(SubscriberStatus.Present, "Should show subscribers as present");
            testNotification.ActiveSubscriberCount.ShouldBe(1, "Should show 1 active subscriber");
            testNotification.SupportsBroadcast.ShouldBeFalse("Single subscriber should not support broadcast");
            testNotification.HandlerStatus.ShouldBe(HandlerStatus.Missing, "Should show no handlers (manual subscribers only)");
        }

        /// <summary>
        /// Tests that multiple subscribers are correctly detected and counted.
        /// </summary>
        [Fact]
        public void NotificationAnalysis_WithMultipleSubscribers_DetectsBroadcastCapability()
        {
            // Arrange
            var services = new ServiceCollection();
            // Don't use assembly scanning to avoid auto-registering handlers
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
            services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
            services.AddSingleton<MediatorStatistics>();

            services.AddScoped<TestTypes.EnhancedTestSubscriber>();
            services.AddScoped<TestTypes.SecondEnhancedTestSubscriber>();
            var serviceProvider = services.BuildServiceProvider();
            
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            var subscriber1 = serviceProvider.GetRequiredService<TestTypes.EnhancedTestSubscriber>();
            var subscriber2 = serviceProvider.GetRequiredService<TestTypes.SecondEnhancedTestSubscriber>();

            // Act - Subscribe multiple subscribers
            mediator.Subscribe(subscriber1);
            mediator.Subscribe(subscriber2);

            // Analyze notifications after subscription - IMPORTANT: Use detailed mode!
            var notifications = statistics.AnalyzeNotifications(serviceProvider, isDetailed: true);
            var testNotification = notifications.FirstOrDefault(n => n.ClassName == nameof(TestTypes.EnhancedTestNotification));

            // Assert
            testNotification.ShouldNotBeNull("EnhancedTestNotification should be found in analysis");
            testNotification.Pattern.ShouldBe(NotificationPattern.ManualSubscribers, "Should detect Manual Subscribers pattern");
            testNotification.SubscriberStatus.ShouldBe(SubscriberStatus.Present, "Should show subscribers as present");
            testNotification.ActiveSubscriberCount.ShouldBe(2, "Should show 2 active subscribers");
            testNotification.SupportsBroadcast.ShouldBeTrue("Multiple subscribers should support broadcast");
            testNotification.SubscriberTypes.Count.ShouldBe(2, "Should show 2 different subscriber types");
        }

        /// <summary>
        /// Tests hybrid pattern detection when both handlers and subscribers are present.
        /// </summary>
        [Fact]
        public void NotificationAnalysis_WithHandlersAndSubscribers_DetectsHybridPattern()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Manual registration of mediator services (no assembly scanning to avoid conflicts)
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
            services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
            services.AddSingleton<MediatorStatistics>();
            
            // Register both handler and subscriber
            services.AddScoped<INotificationHandler<TestTypes.EnhancedTestNotification>, TestTypes.EnhancedTestNotificationHandler>();
            services.AddScoped<TestTypes.EnhancedTestSubscriber>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            var subscriber = serviceProvider.GetRequiredService<TestTypes.EnhancedTestSubscriber>();

            // Act - Both handler and subscriber present
            mediator.Subscribe(subscriber);

            // Analyze notifications
            var notifications = statistics.AnalyzeNotifications(serviceProvider, isDetailed: true);
            var testNotification = notifications.FirstOrDefault(n => n.ClassName == nameof(TestTypes.EnhancedTestNotification));

            // Assert
            testNotification.ShouldNotBeNull("EnhancedTestNotification should be found in analysis");
            testNotification.Pattern.ShouldBe(NotificationPattern.Hybrid, "Should detect Hybrid pattern");
            testNotification.HandlerStatus.ShouldBe(HandlerStatus.Single, "Should show single handler");
            testNotification.SubscriberStatus.ShouldBe(SubscriberStatus.Present, "Should show subscribers as present");
            testNotification.HandlerCount.ShouldBe(1, "Should show 1 handler");
            testNotification.ActiveSubscriberCount.ShouldBe(1, "Should show 1 subscriber");
            testNotification.SupportsBroadcast.ShouldBeTrue("Hybrid with multiple processors should support broadcast");
        }

        /// <summary>
        /// Tests that unsubscription correctly updates the subscriber count.
        /// </summary>
        [Fact]
        public void NotificationAnalysis_AfterUnsubscription_UpdatesSubscriberCount()
        {
            // Arrange
            var services = new ServiceCollection();
            // Don't use assembly scanning to avoid auto-registering handlers
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
            services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
            services.AddSingleton<MediatorStatistics>();

            services.AddScoped<TestTypes.EnhancedTestSubscriber>();
            var serviceProvider = services.BuildServiceProvider();
            
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            var subscriber = serviceProvider.GetRequiredService<TestTypes.EnhancedTestSubscriber>();

            // Act - Subscribe then unsubscribe
            mediator.Subscribe(subscriber);
            
            // Verify subscription worked
            var notificationsAfterSub = statistics.AnalyzeNotifications(serviceProvider);
            var testNotificationAfterSub = notificationsAfterSub.FirstOrDefault(n => n.ClassName == nameof(TestTypes.EnhancedTestNotification));
            testNotificationAfterSub!.ActiveSubscriberCount.ShouldBe(1, "Should have 1 subscriber after subscription");

            // Unsubscribe
            mediator.Unsubscribe(subscriber);

            // Analyze after unsubscription
            var notificationsAfterUnsub = statistics.AnalyzeNotifications(serviceProvider);
            var testNotificationAfterUnsub = notificationsAfterUnsub.FirstOrDefault(n => n.ClassName == nameof(TestTypes.EnhancedTestNotification));

            // Assert
            testNotificationAfterUnsub.ShouldNotBeNull("EnhancedTestNotification should still be found in analysis");
            testNotificationAfterUnsub.Pattern.ShouldBe(NotificationPattern.None, "Should detect no pattern after unsubscription");
            testNotificationAfterUnsub.SubscriberStatus.ShouldBe(SubscriberStatus.None, "Should show no subscribers");
            testNotificationAfterUnsub.ActiveSubscriberCount.ShouldBe(0, "Should show 0 active subscribers");
            testNotificationAfterUnsub.SupportsBroadcast.ShouldBeFalse("No subscribers should not support broadcast");
        }

        /// <summary>
        /// Tests pattern detection for notifications with only automatic handlers (no subscribers).
        /// </summary>
        [Fact]
        public void NotificationAnalysis_WithOnlyHandlers_DetectsAutomaticHandlersPattern()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
            services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
            services.AddSingleton<MediatorStatistics>();
            
            // Explicitly register the handler for this test
            services.AddScoped<INotificationHandler<TestTypes.EnhancedTestNotification>, TestTypes.EnhancedTestNotificationHandler>();

            var serviceProvider = services.BuildServiceProvider();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

            // Act - Only handlers, no subscribers
            var notifications = statistics.AnalyzeNotifications(serviceProvider);
            var testNotification = notifications.FirstOrDefault(n => n.ClassName == nameof(TestTypes.EnhancedTestNotification));

            // Assert
            testNotification.ShouldNotBeNull("EnhancedTestNotification should be found in analysis");
            testNotification.Pattern.ShouldBe(NotificationPattern.AutomaticHandlers, "Should detect Automatic Handlers pattern");
            testNotification.HandlerStatus.ShouldBe(HandlerStatus.Single, "Should show single handler");
            testNotification.SubscriberStatus.ShouldBe(SubscriberStatus.None, "Should show no subscribers");
            testNotification.HandlerCount.ShouldBe(1, "Should show 1 handler");
            testNotification.ActiveSubscriberCount.ShouldBe(0, "Should show 0 subscribers");
            testNotification.SupportsBroadcast.ShouldBeFalse("Single handler should not support broadcast");
        }

        /// <summary>
        /// Tests rendering of enhanced notification analysis output.
        /// </summary>
        [Fact]
        public void NotificationStatisticsRenderer_WithSubscribers_RendersCorrectOutput()
        {
            // Arrange
            var services = new ServiceCollection();
            // Don't use assembly scanning to avoid auto-registering handlers
            services.AddSingleton<IMediator, Mediator>();
            services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
            services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
            services.AddSingleton<MediatorStatistics>();

            services.AddScoped<TestTypes.EnhancedTestSubscriber>();
            var serviceProvider = services.BuildServiceProvider();
            
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();
            var subscriber = serviceProvider.GetRequiredService<TestTypes.EnhancedTestSubscriber>();

            // Capture console output
            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);

            try
            {
                // Act
                mediator.Subscribe(subscriber);
                statistics.RenderNotificationAnalysis(serviceProvider, isDetailed: false);

                var output = sw.ToString();

                // Assert - Key indicators that fix is working
                output.ShouldContain("@ EnhancedTestNotification");
                output.ShouldContain("Manual Subscribers");
                output.ShouldNotContain("(0 handlers)");
                output.ShouldNotContain("! EnhancedTestNotification");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}

namespace Blazing.Mediator.Tests.Enhanced.TestTypes
{
    public class EnhancedTestNotification : INotification
    {
        public string Message { get; init; } = string.Empty;
    }

    public class EnhancedTestSubscriber : INotificationSubscriber<EnhancedTestNotification>
    {
        public Task OnNotification(EnhancedTestNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class SecondEnhancedTestSubscriber : INotificationSubscriber<EnhancedTestNotification>
    {
        public Task OnNotification(EnhancedTestNotification notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class EnhancedTestNotificationHandler : INotificationHandler<EnhancedTestNotification>
    {
        public Task Handle(EnhancedTestNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}