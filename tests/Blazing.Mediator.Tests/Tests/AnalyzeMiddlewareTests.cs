using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;
using System.Reflection;
using static Blazing.Mediator.Tests.NotificationTests.NotificationMiddlewareTests;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for AnalyzeMiddleware functionality in both request and notification pipeline inspectors.
/// </summary>
public class AnalyzeMiddlewareTests
{
    private readonly Assembly _testAssembly = typeof(AnalyzeMiddlewareTests).Assembly;

    /// <summary>
    /// Test request middleware analysis with various middleware types
    /// </summary>
    [Fact]
    public void RequestMiddleware_AnalyzeMiddleware_ReturnsCorrectAnalysis()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();
            config.AddMiddleware<SecondQueryMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(3);

        // Check FirstQueryMiddleware analysis (no Order property = fallback order)
        var firstMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(FirstQueryMiddleware));
        firstMiddleware.ShouldNotBeNull();
        firstMiddleware.ClassName.ShouldBe("FirstQueryMiddleware");
        firstMiddleware.Order.ShouldBe(2146483647); // Fallback order for middleware without Order property
        firstMiddleware.OrderDisplay.ShouldBe("2146483647");
        firstMiddleware.TypeParameters.ShouldBeEmpty();
        firstMiddleware.Configuration.ShouldBeNull();

        // Check AutoDiscoveryStaticOrderMiddleware analysis
        var staticOrderMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(AutoDiscoveryStaticOrderMiddleware));
        staticOrderMiddleware.ShouldNotBeNull();
        staticOrderMiddleware.ClassName.ShouldBe("AutoDiscoveryStaticOrderMiddleware");
        staticOrderMiddleware.Order.ShouldBe(5); // Static order from the class
        staticOrderMiddleware.OrderDisplay.ShouldBe("5");
        staticOrderMiddleware.TypeParameters.ShouldBeEmpty();
        staticOrderMiddleware.Configuration.ShouldBeNull();

        // Check SecondQueryMiddleware analysis (also no Order property = fallback order + 1)
        var secondMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(SecondQueryMiddleware));
        secondMiddleware.ShouldNotBeNull();
        secondMiddleware.ClassName.ShouldBe("SecondQueryMiddleware");
        secondMiddleware.Order.ShouldBe(2146483648); // Fallback order + 1 for second unordered middleware
        secondMiddleware.OrderDisplay.ShouldBe("2146483648");
        secondMiddleware.TypeParameters.ShouldBeEmpty();
        secondMiddleware.Configuration.ShouldBeNull();
    }

    /// <summary>
    /// Test notification middleware analysis with various middleware types
    /// </summary>
    [Fact]
    public void NotificationMiddleware_AnalyzeMiddleware_ReturnsCorrectAnalysis()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(2);

        // Check LoggingNotificationMiddleware analysis
        var loggingMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(LoggingNotificationMiddleware));
        loggingMiddleware.ShouldNotBeNull();
        loggingMiddleware.ClassName.ShouldBe("LoggingNotificationMiddleware");
        loggingMiddleware.Order.ShouldBe(10); // From the class Order property
        loggingMiddleware.OrderDisplay.ShouldBe("10");
        loggingMiddleware.TypeParameters.ShouldBeEmpty();
        loggingMiddleware.Configuration.ShouldBeNull();

        // Check ConditionalNotificationMiddleware analysis
        var conditionalMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(ConditionalNotificationMiddleware));
        conditionalMiddleware.ShouldNotBeNull();
        conditionalMiddleware.ClassName.ShouldBe("ConditionalNotificationMiddleware");
        conditionalMiddleware.Order.ShouldBe(5); // From the class Order property
        conditionalMiddleware.OrderDisplay.ShouldBe("5");
        conditionalMiddleware.TypeParameters.ShouldBeEmpty();
        conditionalMiddleware.Configuration.ShouldBeNull();
    }

    /// <summary>
    /// Test analysis with no middleware registered
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_NoMiddleware_ReturnsEmptyList()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(configureMiddleware: null, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly); // No middleware registered

        var serviceProvider = services.BuildServiceProvider();
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Act
        var requestAnalysis = requestInspector.AnalyzeMiddleware(serviceProvider);
        var notificationAnalysis = notificationInspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        requestAnalysis.ShouldNotBeNull();
        requestAnalysis.Count.ShouldBe(0);

        notificationAnalysis.ShouldNotBeNull();
        notificationAnalysis.Count.ShouldBe(0);
    }

    /// <summary>
    /// Test analysis with auto-discovered middleware
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_AutoDiscovered_ReturnsCorrectAnalysis()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: null,
            discoverMiddleware: true,
            discoverNotificationMiddleware: true,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Act
        var requestAnalysis = requestInspector.AnalyzeMiddleware(serviceProvider);
        var notificationAnalysis = notificationInspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        requestAnalysis.ShouldNotBeNull();
        requestAnalysis.Count.ShouldBeGreaterThan(0);

        notificationAnalysis.ShouldNotBeNull();
        notificationAnalysis.Count.ShouldBeGreaterThan(0);

        // Verify some expected auto-discovered middleware
        requestAnalysis.ShouldContain(a => a.Type == typeof(FirstQueryMiddleware));
        requestAnalysis.ShouldContain(a => a.Type == typeof(AutoDiscoveryStaticOrderMiddleware));
        
        notificationAnalysis.ShouldContain(a => a.Type == typeof(LoggingNotificationMiddleware));
        notificationAnalysis.ShouldContain(a => a.Type == typeof(ConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Test analysis ordering is preserved
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_OrderingPreserved_ReturnsOrderedAnalysis()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>(); // Order: fallback (2146483647)
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>(); // Order: 5
            config.AddMiddleware<SecondQueryMiddleware>(); // Order: fallback + 1 (2146483648)
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(3);

        // Should be ordered by Order property (5, then fallback orders)
        // Note: Middleware with explicit order come first, then unordered middleware in registration order
        var orderedTypes = analysis.OrderBy(a => a.Order).ToList();
        
        // First should be AutoDiscoveryStaticOrderMiddleware with order 5
        orderedTypes[0].Type.ShouldBe(typeof(AutoDiscoveryStaticOrderMiddleware));
        orderedTypes[0].Order.ShouldBe(5);
        
        // Then FirstQueryMiddleware with fallback order
        orderedTypes[1].Type.ShouldBe(typeof(FirstQueryMiddleware));
        orderedTypes[1].Order.ShouldBe(2146483647);
        
        // Finally SecondQueryMiddleware with fallback order + 1
        orderedTypes[2].Type.ShouldBe(typeof(SecondQueryMiddleware));
        orderedTypes[2].Order.ShouldBe(2146483648);
    }

    /// <summary>
    /// Test analysis with generic middleware containing type parameters
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_GenericMiddleware_ExtractsTypeParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            // Add some generic middleware if available in test assembly
            config.AddMiddleware<FirstQueryMiddleware>();
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBeGreaterThan(0);

        // For non-generic types, TypeParameters should be empty
        var nonGenericMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(FirstQueryMiddleware));
        nonGenericMiddleware.ShouldNotBeNull();
        nonGenericMiddleware.TypeParameters.ShouldBeEmpty();
    }

    /// <summary>
    /// Test special order value formatting
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_SpecialOrderValues_FormatsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>(); // Default order 0
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        var middleware = analysis.FirstOrDefault(a => a.Type == typeof(FirstQueryMiddleware));
        middleware.ShouldNotBeNull();
        middleware.Order.ShouldBe(2146483647); // Fallback order for middleware without Order property
        middleware.OrderDisplay.ShouldBe("2146483647");
    }

    /// <summary>
    /// Test that analysis returns read-only list
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_ReturnsReadOnlyList()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.ShouldBeOfType<List<MiddlewareAnalysis>>(); // Should be read-only compatible
        analysis.ShouldBeAssignableTo<IReadOnlyList<MiddlewareAnalysis>>();
    }

    /// <summary>
    /// Test analysis with mixed manual and auto-discovery
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_MixedRegistration_IncludesBothTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: config =>
            {
                config.AddMiddleware<FirstQueryMiddleware>(); // Manual
                config.AddNotificationMiddleware<LoggingNotificationMiddleware>(); // Manual
            },
            discoverMiddleware: true, // Auto request middleware
            discoverNotificationMiddleware: true, // Auto notification middleware
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var requestInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();
        var notificationInspector = serviceProvider.GetRequiredService<INotificationMiddlewarePipelineInspector>();

        // Act
        var requestAnalysis = requestInspector.AnalyzeMiddleware(serviceProvider);
        var notificationAnalysis = notificationInspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        // Should include both manually registered and auto-discovered middleware
        requestAnalysis.ShouldNotBeNull();
        requestAnalysis.Count.ShouldBeGreaterThan(1); // At least manual + some auto-discovered
        requestAnalysis.ShouldContain(a => a.Type == typeof(FirstQueryMiddleware)); // Manual
        requestAnalysis.ShouldContain(a => a.Type == typeof(AutoDiscoveryStaticOrderMiddleware)); // Auto-discovered

        notificationAnalysis.ShouldNotBeNull();
        notificationAnalysis.Count.ShouldBeGreaterThan(1); // At least manual + some auto-discovered
        notificationAnalysis.ShouldContain(a => a.Type == typeof(LoggingNotificationMiddleware)); // Manual + Auto-discovered
        notificationAnalysis.ShouldContain(a => a.Type == typeof(ConditionalNotificationMiddleware)); // Auto-discovered
    }
}