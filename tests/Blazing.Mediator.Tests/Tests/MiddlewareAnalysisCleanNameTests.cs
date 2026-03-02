using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests that MiddlewareAnalysis.ClassName and related formatting properties
/// do not contain backtick characters from generic type name mangling.
/// </summary>
public class MiddlewareAnalysisCleanNameTests
{
    [Fact]
    public void AnalyzeMiddleware_NonGenericMiddleware_ClassNameHasNoBacktick()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<FirstQueryMiddleware>();

        var services = new ServiceCollection();
        services.AddTransient<FirstQueryMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].ClassName.ShouldNotContain("`");
    }

    [Fact]
    public void AnalyzeMiddleware_OpenGenericMiddleware_ClassNameHasNoBacktick()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware(typeof(GenericMiddleware<,>));

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].ClassName.ShouldNotContain("`");
    }

    [Fact]
    public void AnalyzeMiddleware_SingleParamGenericMiddleware_ClassNameHasNoBacktick()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware(typeof(GenericTestMiddleware<>));

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].ClassName.ShouldNotContain("`");
    }

    [Fact]
    public void AnalyzeMiddleware_ClosedGenericMiddleware_ClassNameHasNoBacktick()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<GenericMiddleware<MiddlewareTestQuery, string>>();

        var services = new ServiceCollection();
        services.AddTransient<GenericMiddleware<MiddlewareTestQuery, string>>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].ClassName.ShouldNotContain("`");
    }

    [Fact]
    public void AnalyzeMiddleware_ClosedGenericMiddleware_OrderDisplayNotEmpty()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<GenericMiddleware<MiddlewareTestQuery, string>>();

        var services = new ServiceCollection();
        services.AddTransient<GenericMiddleware<MiddlewareTestQuery, string>>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results[0].OrderDisplay.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void AnalyzeMiddleware_MixedMiddleware_AllClassNamesClean()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<FirstQueryMiddleware>();
        pb.AddMiddleware(typeof(GenericMiddleware<,>));

        var services = new ServiceCollection();
        services.AddTransient<FirstQueryMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(2);
        foreach (var result in results)
        {
            result.ClassName.ShouldNotContain("`");
        }
    }

    [Fact]
    public void AnalyzeMiddleware_TwoParameterMiddleware_ClassNameHasNoBacktick()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware(typeof(TwoParameterTestMiddleware<,>));

        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].ClassName.ShouldNotContain("`");
    }
}

/// <summary>
/// Single-parameter generic test middleware for clean name verification.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public class GenericTestMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public int Order => 0;

    public async ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        await next();
    }
}

/// <summary>
/// Two-parameter generic test middleware for clean name verification.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class TwoParameterTestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 0;

    public async ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await next();
    }
}
