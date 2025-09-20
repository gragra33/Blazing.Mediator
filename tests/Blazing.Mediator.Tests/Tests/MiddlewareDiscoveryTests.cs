using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static Blazing.Mediator.Tests.NotificationTests.NotificationMiddlewareTests;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for all middleware discovery combinations and AnalyzeMiddleware functionality.
/// Tests all four DI registration patterns and inspector functionality.
/// </summary>
public class MiddlewareDiscoveryTests
{
    private readonly Assembly _testAssembly = typeof(MiddlewareDiscoveryTests).Assembly;

    /// <summary>
    /// Test Pattern 1: Manual Middleware and Manual NotificationMiddleware (Fully Manual)
    /// </summary>
    [Fact]
    public void AddMediator_FullyManual_RegistersMiddlewareCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Manual registration only
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify request middleware inspector
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(2);
        requestMiddleware.ShouldContain(typeof(FirstQueryMiddleware));
        requestMiddleware.ShouldContain(typeof(AutoDiscoveryStaticOrderMiddleware));

        // Verify notification middleware inspector
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBe(2);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test Pattern 2: Auto Middleware and Manual NotificationMiddleware (Partially Auto)
    /// </summary>
    [Fact]
    public void AddMediator_AutoRequest_ManualNotification_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Auto request middleware, manual notification middleware
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>()
                  .WithMiddlewareDiscovery();
        }, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify request middleware (should be auto-discovered)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(0); // Some request middleware should be auto-discovered

        // Verify notification middleware (should be manually registered only)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBe(1);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test Pattern 3: Manual Middleware and Auto NotificationMiddleware (Partially Auto)
    /// </summary>
    [Fact]
    public void AddMediator_ManualRequest_AutoNotification_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Manual request middleware, auto notification middleware
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>()
                  .WithNotificationMiddlewareDiscovery();
        }, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify request middleware (should be manually registered only)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(1);
        requestMiddleware.ShouldContain(typeof(FirstQueryMiddleware));

        // Verify notification middleware (should be auto-discovered)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0); // Some notification middleware should be auto-discovered
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test Pattern 4: Auto Middleware and Auto NotificationMiddleware (Fully Auto)
    /// </summary>
    [Fact]
    public void AddMediator_FullyAuto_RegistersAllMiddlewareCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Auto-discover both types
        services.AddMediator(
            config => 
            {
                config.WithMiddlewareDiscovery();
                config.WithNotificationMiddlewareDiscovery();
            },
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify request middleware (should be auto-discovered)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(0);

        // Verify notification middleware (should be auto-discovered)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test nullable parameter overloads with defaults
    /// </summary>
    [Fact]
    public void AddMediator_NullableParameters_DefaultsToFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Manual registration only
        services.AddMediator(
            config => config.AddMiddleware<FirstQueryMiddleware>(),
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Both discovery flags should default to false
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(1); // Only manually registered middleware
        requestMiddleware.ShouldContain(typeof(FirstQueryMiddleware));

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBe(0); // No middleware should be registered
    }

    /// <summary>
    /// Test that auto-discovery works with explicit test assembly
    /// </summary>
    [Fact]
    public void AddMediator_ExplicitTestAssembly_DiscoversMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Use auto-discovery with the test assembly explicitly
        services.AddMediator(
            config => 
            {
                config.WithMiddlewareDiscovery();
                config.WithNotificationMiddlewareDiscovery();
            },
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should find middleware in the test assembly
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(0);
        // Should include auto-discovery middleware
        requestMiddleware.ShouldContain(typeof(AutoDiscoveryStaticOrderMiddleware));

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        // Should include the notification middleware from NotificationMiddlewareTests
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    #region Default Parameter Tests

    /// <summary>
    /// Test AddMediator with only discoverMiddleware=true (other params default)
    /// </summary>
    [Fact]
    public void AddMediator_OnlyDiscoverMiddleware_DefaultsOtherParameters()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Only specify discoverMiddleware, others should default
        services.AddMediator(discoverMiddleware: true, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should discover request middleware (discoverMiddleware=true)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(0);
        requestMiddleware.ShouldContain(typeof(AutoDiscoveryStaticOrderMiddleware));

        // Should also discover notification middleware (defaults to same as discoverMiddleware)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test AddMediatorWithNotificationMiddleware with only discoverNotificationMiddleware=true
    /// </summary>
    [Fact]
    public void AddMediatorWithNotificationMiddleware_OnlyDiscoverNotificationMiddleware_DefaultsRequestToFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Only specify discoverNotificationMiddleware, discoverMiddleware should default to false
        services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: true, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should NOT discover request middleware (discoverMiddleware defaults to false)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);

        // Should discover notification middleware (discoverNotificationMiddleware=true)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test AddMediator with configureMiddleware=null and discoverMiddleware=true
    /// </summary>
    [Fact]
    public void AddMediator_NullConfigureWithDiscoverMiddleware_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - null configureMiddleware with discoverMiddleware=true  
        services.AddMediator(null, discoverMiddleware: true, discoverNotificationMiddleware: true, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should discover both types of middleware (discoverNotificationMiddleware defaults to discoverMiddleware)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(0);
        requestMiddleware.ShouldContain(typeof(AutoDiscoveryStaticOrderMiddleware));

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test AddMediator with configureMiddleware=null and only discoverNotificationMiddleware=true
    /// </summary>
    [Fact]
    public void AddMediator_NullConfigureWithOnlyNotificationDiscovery_RequestMiddlewareNotDiscovered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - null configureMiddleware with only discoverNotificationMiddleware=true
        services.AddMediator(
            config => config.WithNotificationMiddlewareDiscovery(),
            _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should NOT discover request middleware (discoverMiddleware=false)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);

        // Should discover notification middleware (discoverNotificationMiddleware=true)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test AddMediator with Type[] parameters instead of Assembly[] 
    /// </summary>
    [Fact]
    public void AddMediator_WithTypeMarkers_DiscoversFromCorrectAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Use type markers instead of assemblies
        services.AddMediator(discoverMiddleware: true, typeof(MiddlewareDiscoveryTests));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should discover middleware from the assembly containing the type marker
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(0);
        requestMiddleware.ShouldContain(typeof(AutoDiscoveryStaticOrderMiddleware));

        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test AddMediatorWithNotificationMiddleware with Type[] parameters
    /// </summary>
    [Fact]
    public void AddMediatorWithNotificationMiddleware_WithTypeMarkers_OnlyDiscoversNotificationMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Use type markers with notification middleware discovery only
        services.AddMediatorWithNotificationMiddleware(discoverNotificationMiddleware: true, typeof(MiddlewareDiscoveryTests));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should NOT discover request middleware
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);

        // Should discover notification middleware
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test mixed scenario: manual configuration + discovery
    /// </summary>
    [Fact]
    public void AddMediator_ManualConfigurationWithDiscovery_CombinesBoth()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Manual configuration + auto-discovery
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>(); // Manual request middleware
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>(); // Manual notification middleware
        }, discoverMiddleware: true, discoverNotificationMiddleware: true, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should have both manual and discovered request middleware
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBeGreaterThan(1); // At least manual + some discovered
        requestMiddleware.ShouldContain(typeof(FirstQueryMiddleware)); // Manual
        requestMiddleware.ShouldContain(typeof(AutoDiscoveryStaticOrderMiddleware)); // Discovered

        // Should have both manual and discovered notification middleware
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(1); // Manual + discovered (but LoggingNotificationMiddleware appears in both)
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware)); // Manual + Discovered
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware)); // Discovered
    }

    /// <summary>
    /// Test that discoverMiddleware=false truly disables discovery
    /// </summary>
    [Fact]
    public void AddMediator_DiscoverMiddlewareFalse_OnlyUsesManualRegistration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Explicit false for discovery with manual configuration
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should only have manually registered middleware
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(1); // Only the manually registered one
        requestMiddleware.ShouldContain(typeof(FirstQueryMiddleware));
        requestMiddleware.ShouldNotContain(typeof(AutoDiscoveryStaticOrderMiddleware)); // Not discovered

        // Notification middleware should also not be discovered (defaults to discoverMiddleware value)
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBe(0); // No manual registration, no discovery
    }

    #endregion
}