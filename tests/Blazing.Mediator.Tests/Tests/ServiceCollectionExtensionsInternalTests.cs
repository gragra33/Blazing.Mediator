using Blazing.Mediator.Configuration;
using Blazing.Mediator.Notifications;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Tests;

/// <summary>
/// Internal registration tests covering optional pipeline builder resolution,
/// default notification publisher fallback, and manual DI setup scenarios.
/// </summary>
public class ServiceCollectionExtensionsInternalTests
{
    /// <summary>Tests that Mediator resolves successfully with no pipeline builder registered.</summary>
    [Fact]
    public void Mediator_WithNoPipelineBuilder_ResolvesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
        sp.GetService<IMiddlewarePipelineBuilder>().ShouldBeNull();
    }

    /// <summary>Tests that Mediator resolves and uses an explicitly registered pipeline builder.</summary>
    [Fact]
    public void Mediator_WithExplicitPipelineBuilder_UsesIt()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
        sp.GetRequiredService<IMiddlewarePipelineBuilder>().ShouldBeOfType<MiddlewarePipelineBuilder>();
    }

    /// <summary>Tests that when no INotificationPublisher is registered by AddMediator(),
    /// the config overload registers SequentialNotificationPublisher by default.</summary>
    [Fact]
    public void AddMediator_WithConfig_DefaultPublisher_IsSequential()
    {
        var services = new ServiceCollection();
        services.AddMediator(new MediatorConfiguration());
        var sp = services.BuildServiceProvider();

        var publisher = sp.GetRequiredService<INotificationPublisher>();
        publisher.ShouldBeOfType<SequentialNotificationPublisher>();
    }

    /// <summary>Tests that the no-arg AddMediator() does NOT register INotificationPublisher.</summary>
    [Fact]
    public void AddMediator_NoArgs_DoesNotRegisterNotificationPublisher()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        // Without config, no INotificationPublisher is auto-registered
        sp.GetService<INotificationPublisher>().ShouldBeNull();
    }

    /// <summary>Tests that a manually constructed Mediator with null stats works correctly.</summary>
    [Fact]
    public async Task Mediator_ManualConstruction_WithNullStats_SendsSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        // Manually construct mediator with null statistics (should be valid)
        var mediator = new Mediator(sp, statistics: null);

        var result = await mediator.Send(new TestRegistrationQuery());
        result.ShouldNotBeNull();
    }

    /// <summary>Tests that the notification pipeline builder is optional (no-throw when absent).</summary>
    [Fact]
    public void AddMediator_WithNoNotificationPipelineBuilder_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        var sp = services.BuildServiceProvider();

        sp.GetService<INotificationPipelineBuilder>().ShouldBeNull();
        Should.NotThrow(() => sp.GetRequiredService<IMediator>());
    }

    /// <summary>Tests that the notification pipeline builder is resolved when explicitly registered.</summary>
    [Fact]
    public void AddMediator_WithExplicitNotificationPipelineBuilder_ResolvesIt()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<INotificationPipelineBuilder>().ShouldBeOfType<NotificationPipelineBuilder>();
        sp.GetRequiredService<IMediator>().ShouldNotBeNull();
    }
}
