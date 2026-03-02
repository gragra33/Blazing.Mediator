using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for MiddlewarePipelineBuilder.AnalyzeMiddleware() behavior.
/// Verifies analysis results, order sorting, and class name formatting.
/// </summary>
public class AnalyzeMiddlewareTests
{
    [Fact]
    public void AnalyzeMiddleware_WithNoMiddleware_ReturnsEmptyList()
    {
        var pb = new MiddlewarePipelineBuilder();
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.ShouldNotBeNull();
        results.Count.ShouldBe(0);
    }

    [Fact]
    public void AnalyzeMiddleware_WithSingleMiddleware_ReturnsOneEntry()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<FirstQueryMiddleware>();

        var services = new ServiceCollection();
        services.AddTransient<FirstQueryMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].Type.ShouldBe(typeof(FirstQueryMiddleware));
    }

    [Fact]
    public void AnalyzeMiddleware_WithMultipleMiddleware_ReturnsSortedByOrder()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<LowOrderQueryMiddleware>();
        pb.AddMiddleware<HighOrderQueryMiddleware>();

        var services = new ServiceCollection();
        services.AddTransient<LowOrderQueryMiddleware>();
        services.AddTransient<HighOrderQueryMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(2);
        // Results should be sorted by Order ascending
        results[0].Order.ShouldBeLessThan(results[1].Order);
    }

    [Fact]
    public void AnalyzeMiddleware_ClassNameField_DoesNotContainBacktick()
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
    public void AnalyzeMiddleware_WithOrderedMiddleware_SetsOrderDisplay()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<AutoDiscoveryInstanceOrderMiddleware>();

        var services = new ServiceCollection();
        services.AddTransient<AutoDiscoveryInstanceOrderMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].OrderDisplay.ShouldNotBeNullOrEmpty();
        results[0].Order.ShouldBe(10);
    }

    [Fact]
    public void AnalyzeMiddleware_WithStaticOrderMiddleware_ReadsOrderCorrectly()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();

        var services = new ServiceCollection();
        services.AddTransient<AutoDiscoveryStaticOrderMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].Order.ShouldBeLessThan(int.MaxValue - 1000000); // Should read actual order, not fallback
    }

    [Fact]
    public void AnalyzeMiddleware_WithUnorderedMiddleware_AssignsHighFallbackOrder()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<AutoDiscoveryNoOrderMiddleware>();

        var services = new ServiceCollection();
        services.AddTransient<AutoDiscoveryNoOrderMiddleware>();
        var sp = services.BuildServiceProvider();

        var results = pb.AnalyzeMiddleware(sp);

        results.Count.ShouldBe(1);
        results[0].Order.ShouldBeGreaterThanOrEqualTo(int.MaxValue - 1000000); // Fallback order
    }

    [Fact]
    public void AnalyzeMiddleware_GetRegisteredMiddleware_MatchesAddedTypes()
    {
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<FirstQueryMiddleware>();
        pb.AddMiddleware<SecondQueryMiddleware>();

        var registered = pb.GetRegisteredMiddleware();

        registered.Count.ShouldBe(2);
        registered.ShouldContain(typeof(FirstQueryMiddleware));
        registered.ShouldContain(typeof(SecondQueryMiddleware));
    }
}
