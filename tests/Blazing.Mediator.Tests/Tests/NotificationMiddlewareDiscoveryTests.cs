using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static Blazing.Mediator.Tests.NotificationTests.NotificationMiddlewareTests;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for notification middleware auto-discovery functionality.
/// Tests the discoverNotificationMiddleware parameter and AddMediatorWithNotificationMiddleware methods.
/// </summary>
public class NotificationMiddlewareDiscoveryTests
{
    private readonly Assembly _testAssembly = typeof(NotificationMiddlewareDiscoveryTests).Assembly;

    /// <summary>
    /// Test that discoverNotificationMiddleware: true discovers notification middleware automatically
    /// </summary>
    [Fact]
    public void AddMediator_DiscoverNotificationMiddleware_True_DiscoversMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Enable notification middleware discovery only
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: false,
            discoverNotificationMiddleware: true,
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Get notification middleware inspector
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();

        // Should discover notification middleware
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test that discoverNotificationMiddleware: false does not discover middleware
    /// </summary>
    [Fact]
    public void AddMediator_DiscoverNotificationMiddleware_False_DoesNotDiscoverMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Disable notification middleware discovery
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Get notification middleware inspector
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();

        // Should NOT discover any notification middleware
        notificationMiddleware.Count.ShouldBe(0);
    }

    /// <summary>
    /// Test AddMediatorWithNotificationMiddleware method with discovery enabled
    /// </summary>
    [Fact]
    public void AddMediatorWithNotificationMiddleware_DiscoversMiddlewareCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorWithNotificationMiddleware(
            discoverNotificationMiddleware: true,
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should NOT discover request middleware (only notification middleware)
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(0);

        // Should discover notification middleware
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test AddMediatorWithNotificationMiddleware with Type[] parameters
    /// </summary>
    [Fact]
    public void AddMediatorWithNotificationMiddleware_WithTypeMarkers_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorWithNotificationMiddleware(
            discoverNotificationMiddleware: true,
            typeof(NotificationMiddlewareDiscoveryTests)
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should discover notification middleware from the assembly containing the type marker
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
    }

    /// <summary>
    /// Test mixed scenario: discover notification middleware but not request middleware
    /// </summary>
    [Fact]
    public void AddMediator_DiscoverNotificationMiddlewareOnly_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Only discover notification middleware
        services.AddMediator(
            configureMiddleware: config =>
            {
                // Manually add some request middleware
                config.AddMiddleware<FirstQueryMiddleware>();
            },
            discoverMiddleware: false, // Don't auto-discover request middleware
            discoverNotificationMiddleware: true, // Do auto-discover notification middleware
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should have only manually registered request middleware
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var requestMiddleware = requestInspector.GetRegisteredMiddleware();
        requestMiddleware.Count.ShouldBe(1);
        requestMiddleware.ShouldContain(typeof(FirstQueryMiddleware));

        // Should have auto-discovered notification middleware
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();
        notificationMiddleware.Count.ShouldBeGreaterThan(0);
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test that manual + auto-discovery works together for notification middleware
    /// </summary>
    [Fact]
    public void AddMediator_ManualAndAutoDiscovery_CombinesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Manual + auto discovery
        services.AddMediator(config =>
        {
            // Manually register one notification middleware
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
        },
        discoverMiddleware: false,
        discoverNotificationMiddleware: true, // Auto-discover as well
        _testAssembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Should have both manual and auto-discovered notification middleware
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var notificationMiddleware = notificationInspector.GetRegisteredMiddleware();

        // Should include manually registered middleware
        notificationMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));

        // Should also include auto-discovered middleware (ConditionalNotificationMiddleware)
        notificationMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));

        // Depending on implementation, LoggingNotificationMiddleware might appear twice
        // (once manual, once auto-discovered) or be deduplicated
        notificationMiddleware.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    /// <summary>
    /// Test that notification middleware ordering is preserved with auto-discovery
    /// </summary>
    [Fact]
    public void NotificationMiddleware_AutoDiscovery_PreservesOrdering()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: false,
            discoverNotificationMiddleware: true,
            _testAssembly
        );

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();
        var analysis = notificationInspector.AnalyzeMiddleware(serviceProvider).ToList();

        analysis.Count.ShouldBeGreaterThan(0);

        // Should be ordered by Order property
        if (analysis.Count >= 2)
        {
            // ConditionalNotificationMiddleware has Order = 5
            // LoggingNotificationMiddleware has Order = 10
            var conditional = analysis.FirstOrDefault(a => a.Type == typeof(ConditionalNotificationMiddleware));
            var logging = analysis.FirstOrDefault(a => a.Type == typeof(LoggingNotificationMiddleware));

            if (conditional != null && logging != null)
            {
                conditional.Order.ShouldBe(5);
                logging.Order.ShouldBe(10);
                analysis.IndexOf(conditional).ShouldBeLessThan(analysis.IndexOf(logging));
            }
        }
    }
}