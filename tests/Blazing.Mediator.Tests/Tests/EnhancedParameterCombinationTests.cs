using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

        // Act - User's desired pattern using comprehensive overload
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should have statistics enabled
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        statistics.ShouldNotBeNull();

        // Should have notification middleware discovered
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);

        // Should NOT have request middleware discovered
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);
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

        // Act
        services.AddMediator(config =>
        {
            if (enableStats)
                config.WithStatisticsTracking();
            if (discoverRequest)
                config.WithMiddlewareDiscovery();
            if (discoverNotification)
                config.WithNotificationMiddlewareDiscovery();
        }, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Check statistics registration
        var statistics = serviceProvider.GetService<MediatorStatistics>();
        if (enableStats)
        {
            statistics.ShouldNotBeNull();
            var statsRenderer = serviceProvider.GetService<IStatisticsRenderer>();
            statsRenderer.ShouldNotBeNull();
        }
        else
        {
            statistics.ShouldBeNull();
        }

        // Check request middleware discovery
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        if (discoverRequest)
        {
            requestMiddleware.Count.ShouldBeGreaterThan(0);
        }
        else
        {
            requestMiddleware.Count.ShouldBe(0);
        }

        // Check notification middleware discovery
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        if (discoverNotification)
        {
            notificationMiddleware.Count.ShouldBeGreaterThan(0);
        }
        else
        {
            notificationMiddleware.Count.ShouldBe(0);
        }
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
        services.AddMediator(config =>
        {
            // No configuration needed - default behavior
        }, Array.Empty<Assembly>());

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
        services1.AddMediator(config =>
        {
            config.WithNotificationMiddlewareDiscovery();
        }, _testAssembly);

        // Act - Using type marker
        services2.AddMediator(config =>
        {
            config.WithNotificationMiddlewareDiscovery();
        }, typeof(EnhancedParameterCombinationTests).Assembly);

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

        // Act - Configuration function should manage all settings
        services.AddMediator(config =>
        {
            // Test configuration - statistics enabled via configuration
            config.WithStatisticsTracking();
        }, _testAssembly);

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
        services.AddMediator(config =>
        {
            config.WithMiddlewareDiscovery()
                  .WithNotificationMiddlewareDiscovery();
        }, Array.Empty<Assembly>());

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