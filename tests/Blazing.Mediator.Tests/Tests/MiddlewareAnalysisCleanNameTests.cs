using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests to ensure middleware analysis returns clean type names without backtick notation.
/// </summary>
public class MiddlewareAnalysisCleanNameTests
{
    private readonly Assembly _testAssembly = typeof(MiddlewareAnalysisCleanNameTests).Assembly;

    [Fact]
    public void AnalyzeMiddleware_GenericMiddleware_ReturnsCleanTypeNames()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<GenericTestMiddleware<TestCommand>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        
        // Verify that class name does not contain backticks
        middleware.ClassName.ShouldNotContain('`');
        middleware.ClassName.ShouldBe("GenericTestMiddleware");
        
        // Verify that type parameters do not contain backticks
        middleware.TypeParameters.ShouldNotContain('`');
        if (!string.IsNullOrEmpty(middleware.TypeParameters))
        {
            middleware.TypeParameters.ShouldBe("<TestCommand>");
        }
        
        // Verify that generic constraints do not contain backticks
        middleware.GenericConstraints.ShouldNotContain('`');
    }

    [Fact]
    public void AnalyzeMiddleware_MultipleGenericParameters_ReturnsCleanTypeNames()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddMiddleware<TwoParameterMiddleware<TestQuery, string>>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, _testAssembly);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middleware = analysis.First();
        
        // Verify that class name does not contain backticks
        middleware.ClassName.ShouldNotContain('`');
        middleware.ClassName.ShouldBe("TwoParameterMiddleware");
        
        // Verify that type parameters do not contain backticks
        middleware.TypeParameters.ShouldNotContain('`');
        if (!string.IsNullOrEmpty(middleware.TypeParameters))
        {
            middleware.TypeParameters.ShouldBe("<TestQuery, String>");
        }
        
        // Verify that generic constraints do not contain backticks
        middleware.GenericConstraints.ShouldNotContain('`');
    }
}

/// <summary>
/// Test middleware with single generic parameter for testing clean name extraction.
/// </summary>
public class GenericTestMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public int Order => 100;

    public Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        return next();
    }
}

/// <summary>
/// Test middleware with two generic parameters for testing clean name extraction.
/// </summary>
public class TwoParameterMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 200;

    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return next();
    }
}