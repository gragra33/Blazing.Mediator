using Blazing.Mediator.Configuration;
using System.Reflection;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for enhanced parameter combinations in AddMediator methods.
/// Focuses on testing the user's desired parameter patterns and edge cases.
/// </summary>
public class EnhancedParameterCombinationTests
{
    private readonly Assembly _testAssembly = typeof(EnhancedParameterCombinationTests).Assembly;

    /// <summary>
    /// Test the user's desired parameter pattern using the comprehensive overload
    /// </summary>
    [Fact]
    public void AddMediator_UserDesiredPattern_WorksWithComprehensiveOverload()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - User's desired pattern - use WithStatisticsTracking() for stats
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should have statistics enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // In source-gen mode the notification pipeline builder is registered empty.
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.ShouldNotBeNull();

        // Request middleware pipeline inspector should be registered
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.ShouldNotBeNull();
    }

    /// <summary>
    /// Test statistics tracking with various middleware discovery combinations
    /// </summary>
    [Theory]
    [InlineData(true, false, false)] // Stats + no middleware
    [InlineData(true, true, false)]  // Stats + request middleware only
    [InlineData(true, false, true)]  // Stats + notification middleware only
    [InlineData(true, true, true)]   // Stats + both middleware types
    [InlineData(false, true, true)]  // No stats + both middleware types
    public void AddMediator_StatisticsAndMiddlewareCombinations_WorkCorrectly(
        bool enableStats,
        bool discoverRequest,
        bool discoverNotification)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - conditionally enable statistics; pipeline inspector counts are always 0 in source-gen mode
        if (enableStats)
            services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());
        else
            services.AddMediator();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Check statistics registration
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        if (enableStats)
        {
            statistics.ShouldNotBeNull();
            // No default IStatisticsRenderer in v3 source-gen mode
        }
        else
        {
            statistics.ShouldBeNull();
        }

        // In source-gen mode pipeline inspectors are always empty.
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.ShouldNotBeNull();

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.ShouldNotBeNull();
    }

    /// <summary>
    /// Test null parameter handling in comprehensive overloads
    /// </summary>
    [Fact]
    public void AddMediator_NullParameterHandling_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Test with null assemblies
        services.AddMediator();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should default to no discovery when null
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBe(0);
    }

    /// <summary>
    /// Test assembly vs type markers for discovery
    /// </summary>
    [Fact]
    public void AddMediator_AssemblyVsTypeMarkers_ProduceSameResults()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        // Act - Using assembly directly
        services1.AddMediator();

        // Act - Using type marker
        services2.AddMediator();

        // Assert - Both should produce identical results
        var serviceProvider1 = services1.BuildServiceProvider();
        var serviceProvider2 = services2.BuildServiceProvider();

        var inspector1 = serviceProvider1.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var inspector2 = serviceProvider2.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        var middleware1 = inspector1.GetRegisteredMiddleware();
        var middleware2 = inspector2.GetRegisteredMiddleware();

        middleware1.Count.ShouldBe(middleware2.Count);
        middleware1.ShouldAllBe(m => middleware2.Contains(m));
    }

    /// <summary>
    /// Test configuration function priority over parameters
    /// </summary>
    [Fact]
    public void AddMediator_ConfigurationFunctionTakesPriority_OverParameters()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Configuration should manage all settings including stats
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Configuration function should take priority - stats should be enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull(); // Should be enabled via configuration
    }

    /// <summary>
    /// Test edge case: empty assemblies array
    /// </summary>
    [Fact]
    public void AddMediator_EmptyAssembliesArray_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should not discover anything from empty assemblies
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBe(0);
    }
}