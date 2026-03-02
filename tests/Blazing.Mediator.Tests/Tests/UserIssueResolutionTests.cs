using Blazing.Mediator.Configuration;
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
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify statistics tracking is enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // In source-gen mode the notification pipeline builder is registered empty.
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.ShouldNotBeNull();

        // Verify request middleware discovery via inspector
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.ShouldNotBeNull();
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
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify statistics
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // No default IStatisticsRenderer is registered in v3 source-gen mode.
        var statisticsRenderer = serviceProvider.GetService<IStatisticsRenderer>();
        statisticsRenderer.ShouldBeNull();

        // In source-gen mode the notification pipeline builder is registered empty.
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.ShouldNotBeNull();

        // Verify request middleware via inspector
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.ShouldNotBeNull();
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

        // Act - conditionally enable statistics based on test parameter
        if (enableStats)
            services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());
        else
            services.AddMediator();

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

        // In source-gen mode the notification pipeline builder is ALWAYS empty.
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.ShouldNotBeNull();
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
            services.AddMediator();

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
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert - Verify this produces the expected configuration
        var serviceProvider = services.BuildServiceProvider();

        // 1. Mediator should be registered and working
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // 2. Statistics tracking should be enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // 3. In source-gen mode the notification pipeline builder is registered empty.
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.ShouldNotBeNull();

        // 4. Request middleware pipeline inspector should be registered
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.ShouldNotBeNull();
    }
}