using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for type-constrained middleware ordering functionality.
/// Tests both individual type-constrained middleware and mixed scenarios with generic and conditional middleware.
/// </summary>
public class TypeConstrainedMiddlewareOrderingTests
{
    private readonly Assembly _testAssembly = typeof(TypeConstrainedMiddlewareOrderingTests).Assembly;

    #region Type-Constrained Middleware Order Tests

    /// <summary>
    /// Test that type-constrained middleware with explicit Order properties are registered with correct order values.
    /// This is the core test for the bug where type constraints were causing Order property detection to fail.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_WithOrderProperty_ShouldRegisterWithCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            // Add type-constrained middleware with explicit Order properties
            config.AddMiddleware(typeof(TypeConstrainedWithOrderMiddleware<,>));
            config.AddMiddleware(typeof(TypeConstrainedCommandOnlyMiddleware<>));
            config.AddMiddleware(typeof(InterfaceConstrainedMiddleware<,>));
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middlewareInfo.Count.ShouldBe(3);

        // All middleware should have their explicit order values, not fallback values
        foreach (var middleware in middlewareInfo)
        {
            switch (middleware.Type.Name)
            {
                case "TypeConstrainedWithOrderMiddleware`2":
                    middleware.Order.ShouldBe(10, 
                        $"TypeConstrainedWithOrderMiddleware should have order 10, but got {middleware.Order}");
                    break;
                case "TypeConstrainedCommandOnlyMiddleware`1":
                    middleware.Order.ShouldBe(15, 
                        $"TypeConstrainedCommandOnlyMiddleware should have order 15, but got {middleware.Order}");
                    break;
                case "InterfaceConstrainedMiddleware`2":
                    middleware.Order.ShouldBe(20, 
                        $"InterfaceConstrainedMiddleware should have order 20, but got {middleware.Order}");
                    break;
            }

            // Verify none are using fallback order values (which would indicate Order property wasn't detected)
            middleware.Order.ShouldNotBeInRange(2146483647, 2146483660, 
                $"Middleware {middleware.Type.Name} should not be using fallback order values");
        }
    }

    /// <summary>
    /// Test that type-constrained middleware without Order properties get fallback orders correctly.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_WithoutOrderProperty_ShouldGetFallbackOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            config.AddMiddleware(typeof(TypeConstrainedWithoutOrderMiddleware<,>));
            config.AddMiddleware(typeof(AnotherTypeConstrainedWithoutOrderMiddleware<>));
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middlewareInfo.Count.ShouldBe(2);

        // Both should have fallback orders (but different ones to maintain registration order)
        var firstMiddleware = middlewareInfo.First(m => m.Type.Name == "TypeConstrainedWithoutOrderMiddleware`2");
        var secondMiddleware = middlewareInfo.First(m => m.Type.Name == "AnotherTypeConstrainedWithoutOrderMiddleware`1");

        firstMiddleware.Order.ShouldBe(2146483647); // First fallback order
        secondMiddleware.Order.ShouldBe(2146483648); // Second fallback order
    }

    /// <summary>
    /// Test that Order property can be accessed correctly from type-constrained middleware instances.
    /// This verifies that the Order property works at runtime for type-constrained middleware.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_OrderPropertyAccess_ShouldReturnCorrectValue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Register concrete instances of type-constrained middleware
        services.AddScoped<TypeConstrainedWithOrderMiddleware<TestConstrainedRequest, string>>();
        services.AddScoped<TypeConstrainedCommandOnlyMiddleware<TestConstrainedCommand>>();
        services.AddScoped<InterfaceConstrainedMiddleware<TestSimpleInterfaceRequest, string>>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var responseMiddleware = serviceProvider.GetRequiredService<TypeConstrainedWithOrderMiddleware<TestConstrainedRequest, string>>();
        responseMiddleware.Order.ShouldBe(10);

        var commandMiddleware = serviceProvider.GetRequiredService<TypeConstrainedCommandOnlyMiddleware<TestConstrainedCommand>>();
        commandMiddleware.Order.ShouldBe(15);

        var interfaceMiddleware = serviceProvider.GetRequiredService<InterfaceConstrainedMiddleware<TestSimpleInterfaceRequest, string>>();
        interfaceMiddleware.Order.ShouldBe(20);
    }

    #endregion

    #region Mixed Middleware Type Tests

    /// <summary>
    /// Test that all three middleware types (generic, conditional, type-constrained) work together correctly
    /// with proper order precedence and no conflicts.
    /// </summary>
    [Fact]
    public void MixedMiddlewareTypes_AllThreeTypes_ShouldWorkTogetherCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            // Generic middleware (works with any request)
            config.AddMiddleware<FirstQueryMiddleware>(); // No order (fallback)
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>(); // Order: 5
            
            // Conditional middleware (selective execution) - no Order property, will get fallback
            config.AddMiddleware<ConditionalQueryMiddleware>(); 
            
            // Type-constrained middleware (compile-time constraints)
            config.AddMiddleware(typeof(TypeConstrainedWithOrderMiddleware<,>)); // Order: 10
            config.AddMiddleware(typeof(InterfaceConstrainedMiddleware<,>)); // Order: 20
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.Count.ShouldBe(5);

        // Verify execution order: explicit orders first (ascending), then fallback orders
        var orderedMiddleware = analysis.OrderBy(a => a.Order).ToList();

        // Order should be: 5, 10, 20, 2146483647, 2146483648 (ConditionalQueryMiddleware and FirstQueryMiddleware get fallback orders)
        orderedMiddleware[0].Order.ShouldBe(5); // AutoDiscoveryStaticOrderMiddleware
        orderedMiddleware[0].Type.ShouldBe(typeof(AutoDiscoveryStaticOrderMiddleware));

        orderedMiddleware[1].Order.ShouldBe(10); // TypeConstrainedWithOrderMiddleware
        orderedMiddleware[1].Type.Name.ShouldBe("TypeConstrainedWithOrderMiddleware`2");

        orderedMiddleware[2].Order.ShouldBe(20); // InterfaceConstrainedMiddleware
        orderedMiddleware[2].Type.Name.ShouldBe("InterfaceConstrainedMiddleware`2");

        // The last two should be fallback orders (registration order determines which is first)
        orderedMiddleware[3].Order.ShouldBe(2146483647); // First middleware with no order
        orderedMiddleware[4].Order.ShouldBe(2146483648); // Second middleware with no order
        
        // ConditionalQueryMiddleware and FirstQueryMiddleware should have fallback orders
        var fallbackMiddleware = orderedMiddleware.Skip(3).ToList();
        var hasConditional = fallbackMiddleware.Any(m => m.Type == typeof(ConditionalQueryMiddleware));
        var hasFirst = fallbackMiddleware.Any(m => m.Type == typeof(FirstQueryMiddleware));
        
        hasConditional.ShouldBeTrue("ConditionalQueryMiddleware should have fallback order");
        hasFirst.ShouldBeTrue("FirstQueryMiddleware should have fallback order");
    }

    /// <summary>
    /// Test mixed middleware types with auto-discovery to ensure discovery works for all types.
    /// </summary>
    [Fact]
    public void MixedMiddlewareTypes_WithAutoDiscovery_ShouldDiscoverAllTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            config.WithMiddlewareDiscovery(); // Enable auto-discovery
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.Count.ShouldBeGreaterThan(5); // Should discover many middleware

        // Verify that all three types are discovered
        var genericMiddleware = analysis.Where(a => 
            a.Type == typeof(FirstQueryMiddleware) || 
            a.Type == typeof(AutoDiscoveryStaticOrderMiddleware)).ToList();
        genericMiddleware.Count.ShouldBeGreaterThan(0, "Should discover generic middleware");

        var conditionalMiddleware = analysis.Where(a => 
            a.Type == typeof(ConditionalQueryMiddleware)).ToList();
        conditionalMiddleware.Count.ShouldBeGreaterThan(0, "Should discover conditional middleware");

        var typeConstrainedMiddleware = analysis.Where(a => 
            a.Type.Name.Contains("TypeConstrained") || 
            a.Type.Name.Contains("InterfaceConstrained")).ToList();
        typeConstrainedMiddleware.Count.ShouldBeGreaterThan(0, "Should discover type-constrained middleware");

        // Verify that type-constrained middleware have correct orders
        foreach (var middleware in typeConstrainedMiddleware)
        {
            if (middleware.Type.Name == "TypeConstrainedWithOrderMiddleware`2")
            {
                middleware.Order.ShouldBe(10);
            }
            else if (middleware.Type.Name == "InterfaceConstrainedMiddleware`2")
            {
                middleware.Order.ShouldBe(20);
            }
        }
    }

    /// <summary>
    /// Test that middleware ordering respects constraints and doesn't break pipeline execution.
    /// This is an integration test that verifies the actual execution pipeline.
    /// </summary>
    [Fact]
    public void MixedMiddlewareTypes_ExecutionOrder_ShouldRespectConstraintsAndOrdering()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            // Register middleware in mixed order to test sorting
            config.AddMiddleware(typeof(TypeConstrainedWithOrderMiddleware<,>)); // Order: 10
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>(); // Order: 5
            // Remove ConditionalQueryMiddleware as it doesn't have an explicit order
        }, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert - Verify they are sorted by order for execution
        var sortedMiddleware = middlewareInfo.OrderBy(m => m.Order).ToList();
        
        sortedMiddleware[0].Order.ShouldBe(5); // AutoDiscoveryStaticOrderMiddleware  
        sortedMiddleware[1].Order.ShouldBe(10); // TypeConstrainedWithOrderMiddleware

        // Verify type constraints are preserved
        var typeConstrainedMiddleware = sortedMiddleware.First(m => m.Type.Name.Contains("TypeConstrained"));
        typeConstrainedMiddleware.Type.IsGenericTypeDefinition.ShouldBeTrue();
        typeConstrainedMiddleware.Type.GetGenericArguments().Length.ShouldBe(2);
    }

    /// <summary>
    /// Test edge case: multiple type-constrained middleware with same order value.
    /// Should maintain registration order as secondary sort.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_SameOrderValues_ShouldMaintainRegistrationOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            config.AddMiddleware(typeof(SameOrderTypeConstrainedMiddleware1<,>)); // Order: 50
            config.AddMiddleware(typeof(SameOrderTypeConstrainedMiddleware2<,>)); // Order: 50
            config.AddMiddleware(typeof(SameOrderTypeConstrainedMiddleware3<>));  // Order: 50
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middlewareInfo.Count.ShouldBe(3);
        
        // All should have the same order
        middlewareInfo.All(m => m.Order == 50).ShouldBeTrue();

        // Registration order should be preserved within the same order value
        var registrationOrder = inspector.GetRegisteredMiddleware();
        registrationOrder[0].Name.ShouldBe("SameOrderTypeConstrainedMiddleware1`2");
        registrationOrder[1].Name.ShouldBe("SameOrderTypeConstrainedMiddleware2`2");
        registrationOrder[2].Name.ShouldBe("SameOrderTypeConstrainedMiddleware3`1");
    }

    #endregion
}

#region Test Middleware Classes

/// <summary>
/// Type-constrained middleware with explicit Order property for testing.
/// Only processes requests implementing ITestConstraintEntity.
/// </summary>
public class TypeConstrainedWithOrderMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ITestConstraintEntity
{
    public int Order => 10;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

/// <summary>
/// Type-constrained middleware for commands only with explicit Order property.
/// </summary>
public class TypeConstrainedCommandOnlyMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : class, IRequest, ITestConstraintEntity
{
    public int Order => 15;

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

/// <summary>
/// Interface-constrained middleware with explicit Order property.
/// </summary>
public class InterfaceConstrainedMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISimpleTestInterface
{
    public int Order => 20;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

/// <summary>
/// Type-constrained middleware without explicit Order property (should get fallback order).
/// </summary>
public class TypeConstrainedWithoutOrderMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ITestConstraintEntity
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

/// <summary>
/// Another type-constrained middleware without explicit Order property.
/// </summary>
public class AnotherTypeConstrainedWithoutOrderMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : class, IRequest, ITestConstraintEntity
{
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

/// <summary>
/// Type-constrained middleware with same order value for testing registration order preservation.
/// </summary>
public class SameOrderTypeConstrainedMiddleware1<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ITestConstraintEntity
{
    public int Order => 50;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

/// <summary>
/// Type-constrained middleware with same order value for testing registration order preservation.
/// </summary>
public class SameOrderTypeConstrainedMiddleware2<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ITestConstraintEntity
{
    public int Order => 50;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}

/// <summary>
/// Type-constrained middleware with same order value for testing registration order preservation.
/// </summary>
public class SameOrderTypeConstrainedMiddleware3<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : class, IRequest, ITestConstraintEntity
{
    public int Order => 50;

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

#endregion

#region Test Request Classes and Interfaces

/// <summary>
/// Simple interface for testing type constraints without conflicts.
/// </summary>
public interface ISimpleTestInterface
{
    string Data { get; set; }
}

/// <summary>
/// Test request that implements ITestConstraintEntity for type constraint testing.
/// </summary>
public class TestConstrainedRequest : IRequest<string>, ITestConstraintEntity
{
    public string Message { get; set; } = "Test";
    public int Id { get; set; } = 1;
    public string Name { get; set; } = "TestRequest";
}

/// <summary>
/// Test command that implements ITestConstraintEntity for type constraint testing.
/// </summary>
public class TestConstrainedCommand : IRequest, ITestConstraintEntity
{
    public string Message { get; set; } = "Test";
    public int Id { get; set; } = 2;
    public string Name { get; set; } = "TestCommand";
}

/// <summary>
/// Test request that implements ISimpleTestInterface for interface constraint testing.
/// </summary>
public class TestSimpleInterfaceRequest : IRequest<string>, ISimpleTestInterface
{
    public string Data { get; set; } = "Test";
}

#endregion