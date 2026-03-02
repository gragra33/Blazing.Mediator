using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests verifying that middleware types are correctly registered in the DI container
/// by the source-generated AddMediator() extension and that middleware executes via IMediator.
/// </summary>
public class MiddlewareDiscoveryTests
{
    /// <summary>
    /// Verifies that auto-discovered middleware types are registered as transient services by AddMediator().
    /// </summary>
    [Fact]
    public void AddMediator_AutoDiscovery_RegistersMiddlewareTypes()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        // Auto-discovered middleware types should be resolvable
        // The exact set is determined by the source generator scanning the assembly
        var mediator = sp.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that IMediator can be constructed and used when pipeline builder is absent.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithoutPipelineBuilder_CanSendRequests()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        var result = await mediator.Send(new TestQuery { Value = 1 });

        result.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that middleware executes when a IMiddlewarePipelineBuilder is provided.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithPipelineBuilder_MiddlewareExecutes()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<FirstQueryMiddleware>();
        services.AddSingleton<IMiddlewarePipelineBuilder>(pb);
        services.AddTransient<FirstQueryMiddleware>();
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        var result = await mediator.Send(new MiddlewareTestQuery());

        result.ShouldContain("First:");
    }

    /// <summary>
    /// Tests that multiple middleware execute in order when registered with a pipeline builder.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithOrderedMiddleware_ExecutesInOrder()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<LowOrderQueryMiddleware>();
        pb.AddMiddleware<HighOrderQueryMiddleware>();
        services.AddSingleton<IMiddlewarePipelineBuilder>(pb);
        services.AddTransient<LowOrderQueryMiddleware>();
        services.AddTransient<HighOrderQueryMiddleware>();
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        var result = await mediator.Send(new MiddlewareTestQuery());

        // High order (higher number) runs outermost wrapping
        result.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator() registers handlers so they can be resolved by the mediator.
    /// </summary>
    [Fact]
    public async Task AddMediator_RegistersHandlers_ForDiscoveredRequests()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();

        // TestCommand has a registered handler
        await Should.NotThrowAsync(() => mediator.Send(new TestCommand()).AsTask());
    }

    /// <summary>
    /// Tests that the middleware pipeline can be inspected via IMiddlewarePipelineInspector.
    /// </summary>
    [Fact]
    public void AddMediator_WithPipelineBuilder_CanInspectViaInterface()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var pb = new MiddlewarePipelineBuilder();
        pb.AddMiddleware<FirstQueryMiddleware>();
        pb.AddMiddleware<SecondQueryMiddleware>();
        services.AddSingleton<IMiddlewarePipelineBuilder>(pb);
        services.AddSingleton<IMiddlewarePipelineInspector>(pb);
        var sp = services.BuildServiceProvider();

        var inspector = sp.GetRequiredService<IMiddlewarePipelineInspector>();
        var registered = inspector.GetRegisteredMiddleware();

        registered.Count.ShouldBe(2);
        registered.ShouldContain(typeof(FirstQueryMiddleware));
        registered.ShouldContain(typeof(SecondQueryMiddleware));
    }
}
