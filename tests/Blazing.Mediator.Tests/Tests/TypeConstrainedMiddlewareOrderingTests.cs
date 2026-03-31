using Blazing.Mediator.Configuration;
using Blazing.Mediator.Pipeline;
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

    #region [Order(n)] Attribute Tests

    /// <summary>
    /// Tests that [Order(n)] attribute is used when a request middleware has no Order property override.
    /// This simulates cross-assembly middleware where the source generator cannot read the Order
    /// property from syntax trees but CAN read [Order(n)] from compiled metadata.
    /// </summary>
    [Fact]
    public void RequestMiddleware_WithOrderAttributeOnly_UsesAttributeOrder()
    {
        // Arrange — AttributeOnlyOrderMiddleware has [Order(42)] and no Order property.
        // This is the recommended pattern for middleware in NuGet packages / referenced assemblies.
        var builder = new MiddlewarePipelineBuilder();

        // Act
        builder.AddMiddleware<AttributeOnlyOrderMiddleware<TestConstrainedRequest, string>>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(42); // From [Order(42)] attribute — cross-assembly safe
        middleware[0].Type.ShouldBe(typeof(AttributeOnlyOrderMiddleware<TestConstrainedRequest, string>));
    }

    /// <summary>
    /// Tests that [Order(n)] attribute wins over a static Order property on request middleware.
    /// The runtime pipeline builder and the source generator share the same precedence:
    /// [Order(n)] attribute &gt; static property &gt; static field &gt; instance property.
    /// </summary>
    [Fact]
    public void RequestMiddleware_WithOrderAttributeAndStaticProperty_AttributeWins()
    {
        // Arrange — AttributeOverridesStaticPropertyMiddleware has both [Order(75)] and
        // public static int Order => 999. The attribute must win.
        var builder = new MiddlewarePipelineBuilder();

        // Act
        builder.AddMiddleware<AttributeOverridesStaticPropertyMiddleware<TestConstrainedRequest, string>>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(75); // [Order(75)] wins over static Order => 999
    }

    #endregion

    #region Mixed Middleware Type Tests

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

    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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

    public async ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
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

    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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
    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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
    public async ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
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

    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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

    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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

    public async ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

/// <summary>
/// Request middleware with [Order(n)] attribute only — no Order property override.
/// Simulates the recommended pattern for cross-assembly middleware in NuGet packages or
/// referenced assemblies, where the source generator reads [Order(n)] from compiled metadata.
/// </summary>
[Order(42)]
public class AttributeOnlyOrderMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ITestConstraintEntity
{
    // No Order property — order is expressed exclusively via [Order(42)] attribute.
    public ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        => next();
}

/// <summary>
/// Request middleware with [Order(75)] attribute AND a static Order property (999).
/// Used to verify that the [Order(n)] attribute wins over the static property in both the
/// runtime pipeline builder and the source generator.
/// </summary>
[Order(75)]
public class AttributeOverridesStaticPropertyMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ITestConstraintEntity
{
    /// <summary>Superseded by the [Order(75)] attribute — attribute takes priority.</summary>
    public static int Order => 999;

    public ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        => next();
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