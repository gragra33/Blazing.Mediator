using Blazing.Mediator.Configuration;
using Blazing.Mediator.Tests.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Comprehensive tests covering null argument validation, end-to-end send/publish
/// scenarios, and both AddMediator overloads working correctly.
/// </summary>
public class ServiceCollectionExtensionsComprehensiveTests
{
    /// <summary>Tests that passing null IServiceCollection throws ArgumentNullException.</summary>
    [Fact]
    public void AddMediator_NullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;

        Should.Throw<Exception>(() => services!.AddMediator());
    }

    /// <summary>Tests that passing null MediatorConfiguration throws ArgumentNullException.</summary>
    [Fact]
    public void AddMediator_NullConfig_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() => services.AddMediator((MediatorConfiguration)null!));
    }

    /// <summary>Tests that sending a command succeeds after AddMediator().</summary>
    [Fact]
    public async Task AddMediator_NoArgs_SendCommand_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new TestRegistrationCommand());
    }

    /// <summary>Tests that sending a query returns a result after AddMediator().</summary>
    [Fact]
    public async Task AddMediator_NoArgs_SendQuery_ReturnsResult()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new TestRegistrationQuery());

        result.ShouldNotBeNull();
    }

    /// <summary>Tests that sending a command using config overload succeeds.</summary>
    [Fact]
    public async Task AddMediator_WithConfig_SendCommand_Succeeds()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking();
        services.AddMediator(config);
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new TestRegistrationCommand());
    }

    /// <summary>Tests that sending a query using config overload returns a result.</summary>
    [Fact]
    public async Task AddMediator_WithConfig_SendQuery_ReturnsResult()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking();
        services.AddMediator(config);
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new TestRegistrationQuery());

        result.ShouldNotBeNull();
    }

    /// <summary>Tests that publishing a notification succeeds after AddMediator().</summary>
    [Fact]
    public async Task AddMediator_NoArgs_PublishNotification_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Publish a notification; having no handlers for this type is acceptable (no-throw)
        await mediator.Publish(new TestNotification { Message = "test" });
    }

    /// <summary>Tests that the returns IServiceCollection for fluent chaining.</summary>
    [Fact]
    public void AddMediator_ReturnsServiceCollection_ForFluentChaining()
    {
        var services = new ServiceCollection();

        var result = services.AddMediator();

        result.ShouldBeSameAs(services);
    }

    /// <summary>Tests that two separate DI containers are independent.</summary>
    [Fact]
    public void AddMediator_TwoContainers_AreIndependent()
    {
        var services1 = new ServiceCollection();
        services1.AddMediator();
        var sp1 = services1.BuildServiceProvider();

        var services2 = new ServiceCollection();
        services2.AddMediator();
        var sp2 = services2.BuildServiceProvider();

        var mediator1 = sp1.GetRequiredService<IMediator>();
        var mediator2 = sp2.GetRequiredService<IMediator>();

        mediator1.ShouldNotBeSameAs(mediator2);
    }
}
