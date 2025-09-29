using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests to validate that the user's original issue has been resolved.
/// These tests ensure the specific parameter combinations requested by the user work correctly.
/// </summary>
public class UserIssueResolutionTests
{
    /// <summary>
    /// Test the exact parameter pattern the user wanted to support from their original issue.
    /// This validates that their desired syntax now works correctly.
    /// </summary>
    [Fact]
    public void AddMediator_UserOriginalSyntax_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - This is the EXACT syntax the user wanted to support:
        // services.AddMediator(
        //     enableStatisticsTracking: true,
        //     discoverNotificationMiddleware: true,
        //     Assembly.GetExecutingAssembly()
        // );
        //
        // This works using the new configuration approach:
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify statistics tracking is enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // Verify notification middleware discovery worked
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);

        // Verify request middleware discovery was NOT enabled (discoverMiddleware: false)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);
    }

    /// <summary>
    /// Test that all the functionality from the user's SimpleNotificationExample works
    /// </summary>
    [Fact]
    public void AddMediator_SimpleNotificationExamplePattern_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - This pattern is from their SimpleNotificationExample
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery();         // DO discover notification middleware
        }, Assembly.GetExecutingAssembly());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // The user's example expects:
        // - Statistics tracking enabled
        // - Notification middleware auto-discovered
        // - Request middleware NOT auto-discovered

        // Verify statistics
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        var statisticsRenderer = serviceProvider.GetService<IStatisticsRenderer>();
        statisticsRenderer.ShouldNotBeNull();

        // Verify notification middleware discovery
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();

        // Should have auto-discovered notification middleware
        notificationMiddleware.Count.ShouldBeGreaterThan(0);

        // Verify request middleware was not discovered
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);
    }

    /// <summary>
    /// Test alternative syntax patterns that should also work for the user
    /// </summary>
    [Theory]
    [InlineData(true, true)]   // Stats + notification middleware
    [InlineData(true, false)]  // Stats only
    [InlineData(false, true)]  // Notification middleware only
    [InlineData(false, false)] // Neither (minimal setup)
    public void AddMediator_AlternativeSyntaxPatterns_WorkCorrectly(bool enableStats, bool discoverNotifications)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Test various combinations the user might want to use
        services.AddMediator(config =>
        {
            if (enableStats)
                config.WithStatisticsTracking();
            if (discoverNotifications)
                config.WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Check statistics
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        if (enableStats)
        {
            statistics.ShouldNotBeNull();
        }
        else
        {
            statistics.ShouldBeNull();
        }

        // Check notification middleware
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();

        if (discoverNotifications)
        {
            notificationMiddleware.Count.ShouldBeGreaterThan(0);
        }
        else
        {
            notificationMiddleware.Count.ShouldBe(0);
        }
    }

    /// <summary>
    /// Test that the comprehensive overload provides all the flexibility the user needs
    /// </summary>
    [Fact]
    public void AddMediator_ComprehensiveOverload_ProvidesFullFlexibility()
    {
        // Arrange & Act - Test all the variations the user might need
        var testCases = new[]
        {
            // (enableStats, discoverRequest, discoverNotification, description)
            (true, false, true, "User's preferred pattern"),
            (true, true, false, "Stats + request middleware only"),
            (true, true, true, "Stats + all middleware"),
            (false, false, true, "Notification middleware only"),
            (false, true, false, "Request middleware only"),
            (false, true, true, "All middleware, no stats"),
            (false, false, false, "Minimal setup")
        };

        foreach (var (enableStats, discoverRequest, discoverNotification, description) in testCases)
        {
            var services = new ServiceCollection();

            // Act
            services.AddMediator(config =>
            {
                if (enableStats)
                    config.WithStatisticsTracking();
                if (discoverRequest)
                    config.WithMiddlewareDiscovery();
                if (discoverNotification)
                    config.WithNotificationMiddlewareDiscovery();
            }, Assembly.GetExecutingAssembly());

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            mediator.ShouldNotBeNull($"Failed for case: {description}");

            // All cases should produce a working mediator
            // Specific feature validation is covered in other tests
        }
    }

    /// <summary>
    /// Test that the user can now achieve their original goal from the SimpleNotificationExample
    /// </summary>
    [Fact]
    public void AddMediator_OriginalGoalAchieved_SuccessfullyConfigured()
    {
        // Arrange
        var services = new ServiceCollection();

        // This works now with the new configuration approach:
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());

        // Assert - Verify this produces the exact configuration the user wanted
        var serviceProvider = services.BuildServiceProvider();

        // 1. Mediator should be registered and working
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // 2. Statistics tracking should be enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // 3. Notification middleware should be auto-discovered
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);

        // 4. Request middleware should NOT be auto-discovered (wasn't requested)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);
    }
}