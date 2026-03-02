using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Core registration tests for the source-generated AddMediator() extension methods.
/// Verifies that IMediator, handlers, and the type catalog are registered correctly.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    /// <summary>Tests that AddMediator() registers IMediator.</summary>
    [Fact]
    public void AddMediator_NoArgs_RegistersIMediator()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that the resolved IMediator is of the concrete Mediator type.</summary>
    [Fact]
    public void AddMediator_NoArgs_MediatorIsCorrectType()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldBeOfType<Mediator>();
    }

    /// <summary>Tests that AddMediator() registers all compile-time-discovered handlers.</summary>
    [Fact]
    public void AddMediator_NoArgs_RegistersDiscoveredHandlers()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        // These handlers are discovered and registered by the source generator
        sp.GetRequiredService<TestRegistrationCommandHandler>().ShouldNotBeNull();
        sp.GetRequiredService<TestRegistrationQueryHandler>().ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator() allows IMediator.Send to execute a query handler.</summary>
    [Fact]
    public async Task AddMediator_NoArgs_CanSendQuery()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new TestRegistrationQuery());

        result.ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator() registers the IMediatorTypeCatalog singleton.</summary>
    [Fact]
    public void AddMediator_NoArgs_RegistersTypeCatalog()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediatorTypeCatalog>().ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator(config) succeeds with statistics enabled.</summary>
    [Fact]
    public void AddMediator_WithStatisticsConfig_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking();

        services.AddMediator(config);
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator(config) succeeds with logging enabled.</summary>
    [Fact]
    public void AddMediator_WithLoggingConfig_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();
        config.WithLogging();

        services.AddMediator(config);
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator(config) succeeds with telemetry enabled.</summary>
    [Fact]
    public void AddMediator_WithTelemetryConfig_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();
        config.WithTelemetry();

        services.AddMediator(config);
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator() can be called multiple times without throwing.</summary>
    [Fact]
    public void AddMediator_CalledTwice_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        Should.NotThrow(() => services.AddMediator());
    }

    /// <summary>Tests that AddMediator() registers IMediator as a singleton.</summary>
    [Fact]
    public void AddMediator_IMediator_IsRegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        var mediator1 = sp.GetRequiredService<IMediator>();
        var mediator2 = sp.GetRequiredService<IMediator>();

        mediator1.ShouldBeSameAs(mediator2);
    }

    /// <summary>Tests that MediatorConfiguration preset methods produce a valid registration.</summary>
    [Fact]
    public void AddMediator_WithProductionPreset_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator(MediatorConfiguration.Production());
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that MediatorConfiguration.Development preset produces a valid registration.</summary>
    [Fact]
    public void AddMediator_WithDevelopmentPreset_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator(MediatorConfiguration.Development());
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that MediatorConfiguration.Minimal preset produces a valid registration.</summary>
    [Fact]
    public void AddMediator_WithMinimalPreset_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator(MediatorConfiguration.Minimal());
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that MediatorConfiguration.Disabled preset produces a valid registration.</summary>
    [Fact]
    public void AddMediator_WithDisabledPreset_RegistersSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator(MediatorConfiguration.Disabled());
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }

    /// <summary>Tests that null MediatorConfiguration throws ArgumentNullException.</summary>
    [Fact]
    public void AddMediator_WithNullConfig_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(() => services.AddMediator((MediatorConfiguration)null!));
    }
}
