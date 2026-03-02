using Blazing.Mediator.Configuration;
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