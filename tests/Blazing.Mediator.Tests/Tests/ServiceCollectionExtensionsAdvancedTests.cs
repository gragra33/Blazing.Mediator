using Blazing.Mediator.Configuration;
using Blazing.Mediator.Notifications;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Advanced DI registration tests covering notification publisher configuration
/// and optional pipeline builder behaviour.
/// </summary>
public class ServiceCollectionExtensionsAdvancedTests
{
    /// <summary>Tests that the default AddMediator() registers a sequential notification publisher.</summary>
    [Fact]
    public void AddMediator_DefaultConfig_RegistersSequentialPublisher()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();

        services.AddMediator(config);
        var sp = services.BuildServiceProvider();

        var publisher = sp.GetRequiredService<INotificationPublisher>();
        publisher.ShouldBeOfType<SequentialNotificationPublisher>();
    }

    /// <summary>Tests that specifying Concurrent publisher type registers ConcurrentNotificationPublisher.</summary>
    [Fact]
    public void AddMediator_WithConcurrentPublisher_RegistersConcurrentPublisher()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();

        services.AddMediator(config, opts => opts.NotificationPublisher = NotificationPublisherType.Concurrent);
        var sp = services.BuildServiceProvider();

        var publisher = sp.GetRequiredService<INotificationPublisher>();
        publisher.ShouldBeOfType<ConcurrentNotificationPublisher>();
    }

    /// <summary>Tests that specifying Sequential publisher type registers SequentialNotificationPublisher.</summary>
    [Fact]
    public void AddMediator_WithSequentialPublisher_RegistersSequentialPublisher()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();

        services.AddMediator(config, opts => opts.NotificationPublisher = NotificationPublisherType.Sequential);
        var sp = services.BuildServiceProvider();

        var publisher = sp.GetRequiredService<INotificationPublisher>();
        publisher.ShouldBeOfType<SequentialNotificationPublisher>();
    }

    /// <summary>Tests that AddMediator() does NOT require IMiddlewarePipelineBuilder in DI.</summary>
    [Fact]
    public void AddMediator_NoArgs_PipelineBuilderNotRequired()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        // Should not throw when no pipeline builder is registered
        Should.NotThrow(() => sp.GetRequiredService<IMediator>());
        sp.GetService<IMiddlewarePipelineBuilder>().ShouldBeNull();
    }

    /// <summary>Tests that manually registering IMiddlewarePipelineBuilder is picked up by the Mediator.</summary>
    [Fact]
    public void AddMediator_WithManualPipelineBuilder_MediatorResolvesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
        sp.GetRequiredService<IMiddlewarePipelineBuilder>().ShouldNotBeNull();
    }

    /// <summary>Tests that manually registering INotificationPipelineBuilder is picked up.</summary>
    [Fact]
    public void AddMediator_WithManualNotificationPipelineBuilder_MediatorResolvesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
        sp.GetRequiredService<INotificationPipelineBuilder>().ShouldNotBeNull();
    }

    /// <summary>Tests that AddMediator(config) with combined options works end-to-end.</summary>
    [Fact]
    public async Task AddMediator_ConfigAndPublisher_SendQuerySucceeds()
    {
        var services = new ServiceCollection();
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking();

        services.AddMediator(config, opts => opts.NotificationPublisher = NotificationPublisherType.Sequential);
        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new TestRegistrationQuery());

        result.ShouldNotBeNull();
    }
}
