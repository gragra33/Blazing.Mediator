using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Edge-case and coverage tests verifying that all critical DI registrations
/// produced by AddMediator() are present and correct.
/// </summary>
public class ServiceCollectionExtensionsFullCoverageTests
{
    /// <summary>Tests that MediatorDispatcherBase is registered as a singleton.</summary>
    [Fact]
    public void AddMediator_RegistersMediatorDispatcherBase()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<MediatorDispatcherBase>().ShouldNotBeNull();
    }

    /// <summary>Tests that MediatorDispatcherBase is a singleton.</summary>
    [Fact]
    public void AddMediator_DispatcherBase_IsSingleton()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        var d1 = sp.GetRequiredService<MediatorDispatcherBase>();
        var d2 = sp.GetRequiredService<MediatorDispatcherBase>();

        d1.ShouldBeSameAs(d2);
    }

    /// <summary>Tests that sending a query end-to-end with the full generated pipeline succeeds.</summary>
    [Fact]
    public async Task AddMediator_FullPipeline_SendQuery_ReturnsResult()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new TestRegistrationQuery());

        result.ShouldNotBeNull();
    }

    /// <summary>Tests that the type catalog reports request types in the assembly.</summary>
    [Fact]
    public void AddMediator_TypeCatalog_ContainsKnownRequestTypes()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();
        var catalog = sp.GetRequiredService<IMediatorTypeCatalog>();

        var requestHandlers = catalog.RequestHandlers;

        requestHandlers.ShouldNotBeNull();
        requestHandlers.ShouldNotBeEmpty();
    }

    /// <summary>Tests that duplicate AddMediator() calls do not break IMediator singleton.</summary>
    [Fact]
    public void AddMediator_CalledTwice_MediatorStillSingleton()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddMediator(); // second call
        var sp = services.BuildServiceProvider();

        // Should still resolve without errors
        var m1 = sp.GetRequiredService<IMediator>();
        var m2 = sp.GetRequiredService<IMediator>();
        m1.ShouldBeSameAs(m2);
    }

    /// <summary>Tests that command handler types are registered by AddMediator().</summary>
    [Fact]
    public void AddMediator_RegistersCommandHandlers()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        // Verify a known handler type is registered
        sp.GetRequiredService<TestRegistrationCommandHandler>().ShouldNotBeNull();
        sp.GetRequiredService<TestRegistrationQueryHandler>().ShouldNotBeNull();
    }
}
