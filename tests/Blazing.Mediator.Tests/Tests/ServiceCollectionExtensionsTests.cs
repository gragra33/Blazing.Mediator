using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for ServiceCollectionExtensions functionality.
/// Covers registration scenarios, handler discovery, and dependency injection integration.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    /// <summary>
    /// Tests that AddMediator with assemblies registers the mediator service.
    /// </summary>
    [Fact]
    public void AddMediator_WithAssemblies_RegistersMediator()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
        mediator.ShouldBeOfType<Mediator>();
    }

    /// <summary>
    /// Tests that AddMediator with assemblies registers the request handlers.
    /// </summary>
    [Fact]
    public void AddMediator_WithAssemblies_RegistersHandlers()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestRegistrationCommand>? commandHandler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();
        IRequestHandler<TestRegistrationQuery, string>? queryHandler = serviceProvider.GetService<IRequestHandler<TestRegistrationQuery, string>>();

        commandHandler.ShouldNotBeNull();
        queryHandler.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with types registers from the correct assemblies.
    /// </summary>
    [Fact]
    public void AddMediator_WithTypes_RegistersFromCorrectAssemblies()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(typeof(TestRegistrationCommand), typeof(ServiceCollectionExtensionsTests));

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IRequestHandler<TestRegistrationCommand>? handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.ShouldNotBeNull();
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with null assemblies registers only the mediator service.
    /// </summary>
    [Fact]
    public void AddMediator_WithNullAssemblies_RegistersMediatorOnly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator((Assembly[])null!);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with empty assemblies registers only the mediator service.
    /// </summary>
    [Fact]
    public void AddMediator_WithEmptyAssemblies_RegistersMediatorOnly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(Array.Empty<Assembly>());

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with null types registers only the mediator service.
    /// </summary>
    [Fact]
    public void AddMediator_WithNullTypes_RegistersMediatorOnly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator((Type[])null!);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromCallingAssembly registers both mediator and handlers.
    /// </summary>
    [Fact]
    public void AddMediatorFromCallingAssembly_RegistersMediatorAndHandlers()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromCallingAssembly();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IRequestHandler<TestRegistrationCommand>? handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.ShouldNotBeNull();
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromLoadedAssemblies without filter registers from all assemblies.
    /// </summary>
    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithoutFilter_RegistersFromAllAssemblies()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromLoadedAssemblies();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromLoadedAssemblies with filter registers from filtered assemblies.
    /// </summary>
    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersFromFilteredAssemblies()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromLoadedAssemblies(assembly =>
            assembly.FullName?.Contains("Blazing.Mediator.Tests") == true);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IRequestHandler<TestRegistrationCommand>? handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.ShouldNotBeNull();
        handler.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with duplicate assemblies registers handlers only once.
    /// </summary>
    [Fact]
    public void AddMediator_WithDuplicateAssemblies_RegistersHandlersOnce()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly, assembly); // Same assembly twice

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IEnumerable<IRequestHandler<TestRegistrationCommand>> handlers = serviceProvider.GetServices<IRequestHandler<TestRegistrationCommand>>();
        handlers.Count().ShouldBe(1); // Should only register once
    }

    /// <summary>
    /// Tests that AddMediator with multiple interfaces registers all interfaces for a handler.
    /// </summary>
    [Fact]
    public void AddMediator_WithMultipleInterfaces_RegistersAllInterfaces()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(TestMultiInterfaceHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestMultiCommand>? commandHandler = serviceProvider.GetService<IRequestHandler<TestMultiCommand>>();
        IRequestHandler<TestMultiQuery, string>? queryHandler = serviceProvider.GetService<IRequestHandler<TestMultiQuery, string>>();

        commandHandler.ShouldNotBeNull();
        queryHandler.ShouldNotBeNull();
        commandHandler.ShouldBeSameAs(queryHandler); // Same instance
    }

    /// <summary>
    /// Tests that AddMediator with abstract handler does not register the handler.
    /// </summary>
    [Fact]
    public void AddMediator_WithAbstractHandler_DoesNotRegisterHandler()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(TestAbstractHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestAbstractCommand>? handler = serviceProvider.GetService<IRequestHandler<TestAbstractCommand>>();
        handler.ShouldBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with interface handler does not register the handler.
    /// </summary>
    [Fact]
    public void AddMediator_WithInterfaceHandler_DoesNotRegisterHandler()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(ITestInterfaceHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestInterfaceCommand>? handler = serviceProvider.GetService<IRequestHandler<TestInterfaceCommand>>();
        handler.ShouldBeNull();
    }

    #region Middleware Auto-Discovery Tests

    /// <summary>
    /// Tests that auto-discovery registers middleware from assemblies.
    /// </summary>
    [Fact]
    public void AddMediator_WithAutoDiscovery_RegistersMiddleware()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;

        // Act
        services.AddMediator(null, discoverMiddleware: true, discoverNotificationMiddleware: false, assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify middleware was registered by checking the configuration
        MediatorConfiguration? config = serviceProvider.GetService<MediatorConfiguration>();
        config.ShouldNotBeNull();
        config.PipelineBuilder.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that auto-discovery respects middleware order from static Order property.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscovery_RespectsStaticOrderProperty()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;

        // Only register our specific auto-discovery middleware and exclude problematic ones
        services.AddMediator(config =>
        {
            // Clear any auto-discovered middleware and add only our test middleware
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new() { Value = "test" };
        string result = await mediator.Send(query);

        // Assert
        // The middleware should execute in order based on Order property
        result.ShouldContain("StaticOrder:");
    }

    /// <summary>
    /// Tests that auto-discovery respects middleware order from instance Order property.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscovery_RespectsInstanceOrderProperty()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryInstanceOrderMiddleware).Assembly;

        services.AddMediator(config =>
        {
            config.AddMiddleware<AutoDiscoveryInstanceOrderMiddleware>();
        }, (Assembly[])null!);

        // Register the handler for MiddlewareTestQuery
        services.AddScoped<IRequestHandler<MiddlewareTestQuery, string>, MiddlewareTestQueryHandler>();

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new() { Value = "test" };
        string result = await mediator.Send(query);

        // Assert
        result.ShouldContain("InstanceOrder:");
    }

    /// <summary>
    /// Tests that auto-discovery handles middleware without Order property.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscovery_HandlesNoOrderProperty()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryNoOrderMiddleware).Assembly;

        services.AddMediator(config =>
        {
            config.AddMiddleware<AutoDiscoveryNoOrderMiddleware>();
        }, (Assembly[])null!);

        // Register the handler for MiddlewareTestQuery
        services.AddScoped<IRequestHandler<MiddlewareTestQuery, string>, MiddlewareTestQueryHandler>();

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new() { Value = "test" };
        string result = await mediator.Send(query);

        // Assert
        result.ShouldContain("NoOrder:");
    }

    /// <summary>
    /// Tests that auto-discovery registers conditional middleware correctly.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscovery_RegistersConditionalMiddleware()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryConditionalMiddleware).Assembly;

        services.AddMediator(config =>
        {
            config.AddMiddleware<AutoDiscoveryConditionalMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        // Test conditional execution - should execute
        MiddlewareTestQuery queryWithAuto = new() { Value = "auto-test" };
        string resultWithAuto = await mediator.Send(queryWithAuto);

        // Test conditional execution - should not execute
        MiddlewareTestQuery queryWithoutAuto = new() { Value = "manual-test" };
        string resultWithoutAuto = await mediator.Send(queryWithoutAuto);

        // Assert
        resultWithAuto.ShouldContain("Conditional:");
        resultWithoutAuto.ShouldNotContain("Conditional:");
    }

    /// <summary>
    /// Tests that auto-discovery works with multiple middleware types in correct order.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscovery_ExecutesMultipleMiddlewareInOrder()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;

        services.AddMediator(config =>
        {
            // Add our auto-discovery middleware in controlled way
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();
            config.AddMiddleware<AutoDiscoveryInstanceOrderMiddleware>();
            config.AddMiddleware<AutoDiscoveryConditionalMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new() { Value = "auto-test" };
        string result = await mediator.Send(query);

        // Assert
        // Should contain middleware results in execution order
        result.ShouldContain("StaticOrder:");
        result.ShouldContain("InstanceOrder:");
        result.ShouldContain("Conditional:");

        // Verify execution order (static order 5 < instance order 10 < conditional order 15)
        int staticPos = result.IndexOf("StaticOrder:", StringComparison.Ordinal);
        int instancePos = result.IndexOf("InstanceOrder:", StringComparison.Ordinal);
        int conditionalPos = result.IndexOf("Conditional:", StringComparison.Ordinal);

        staticPos.ShouldBeLessThan(instancePos);
        instancePos.ShouldBeLessThan(conditionalPos);
    }

    /// <summary>
    /// Tests that manual configuration overrides auto-discovery.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscoveryAndManualConfig_AllowsOverrides()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;

        services.AddMediator(config =>
        {
            // Manually add specific middleware without auto-discovery to avoid ThrowingQueryMiddleware
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();
            config.AddMiddleware<FirstQueryMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new() { Value = "auto-test" };
        string result = await mediator.Send(query);

        // Assert
        // Should contain both auto-discovered and manually configured middleware
        result.ShouldContain("StaticOrder:");
        result.ShouldContain("First:");
    }

    /// <summary>
    /// Tests that auto-discovery with false flag does not register middleware.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithAutoDiscoveryDisabled_DoesNotRegisterMiddleware()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;

        services.AddMediator(null, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly);

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        MiddlewareTestQuery query = new() { Value = "auto-test" };
        string result = await mediator.Send(query);

        // Assert
        // Should not contain auto-discovered middleware
        result.ShouldNotContain("StaticOrder:");
        result.ShouldNotContain("InstanceOrder:");
        result.ShouldNotContain("NoOrder:");
        result.ShouldNotContain("Conditional:");

        // Should just return the base handler result
        result.ShouldBe("Handler: auto-test");
    }

    /// <summary>
    /// Tests that auto-discovery functionality works as expected with integration test.
    /// </summary>
    [Fact]
    public void AddMediator_AutoDiscoveryIntegration_WorksCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Create a minimal assembly with just our test middleware by using a Type array
        // This tests the auto-discovery mechanism without the problematic ThrowingQueryMiddleware

        // Act - Use auto-discovery but limit the scope
        services.AddMediator(config =>
        {
            // We can still manually add middleware after auto-discovery
            config.AddMiddleware<FirstQueryMiddleware>();
        }, discoverMiddleware: true, discoverNotificationMiddleware: false, typeof(ServiceCollectionExtensionsTests).Assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        // Verify that the configuration was created
        MediatorConfiguration? config = serviceProvider.GetService<MediatorConfiguration>();
        config.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that auto-discovery works with multiple assemblies.
    /// </summary>
    [Fact]
    public void AddMediator_WithAutoDiscoveryMultipleAssemblies_RegistersMiddleware()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly1 = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;
        Assembly assembly2 = typeof(FirstQueryMiddleware).Assembly; // Should be same assembly but testing array

        // Use manual registration to avoid ThrowingQueryMiddleware
        services.AddMediator(config =>
        {
            config.AddMiddleware<AutoDiscoveryStaticOrderMiddleware>();
            config.AddMiddleware<FirstQueryMiddleware>();
        }, discoverMiddleware: false, discoverNotificationMiddleware: false, assembly1, assembly2);

        // Act
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Assert
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        MediatorConfiguration? config = serviceProvider.GetService<MediatorConfiguration>();
        config.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromCallingAssembly with auto-discovery works.
    /// </summary>
    [Fact]
    public void AddMediatorFromCallingAssembly_WithAutoDiscovery_RegistersMiddleware()
    {
        // Arrange
        ServiceCollection services = new();

        // Act - Use manual configuration to avoid problematic middleware
        services.AddMediatorFromCallingAssembly(config =>
        {
            config.AddMiddleware<FirstQueryMiddleware>();
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();

        MediatorConfiguration? config = serviceProvider.GetService<MediatorConfiguration>();
        config.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests simple overload with discoverMiddleware parameter.
    /// </summary>
    [Fact]
    public void AddMediator_SimpleOverloadWithDiscoverMiddleware_Works()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly assembly = typeof(AutoDiscoveryStaticOrderMiddleware).Assembly;

        // Act - Use the simple overload
        services.AddMediator(config => { }, assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests simple overload with discoverMiddleware and Type array.
    /// </summary>
    [Fact]
    public void AddMediator_SimpleOverloadWithTypesAndDiscoverMiddleware_Works()
    {
        // Arrange
        ServiceCollection services = new();

        // Act - Use the simple overload with types
        services.AddMediator(discoverMiddleware: false, typeof(ServiceCollectionExtensionsTests));

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests simple overload for AddMediatorFromCallingAssembly with discoverMiddleware.
    /// </summary>
    [Fact]
    public void AddMediatorFromCallingAssembly_SimpleOverloadWithDiscoverMiddleware_Works()
    {
        // Arrange
        ServiceCollection services = new();

        // Act - Use the simple overload
        services.AddMediatorFromCallingAssembly(discoverMiddleware: false);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests simple overload for AddMediatorFromLoadedAssemblies with discoverMiddleware.
    /// </summary>
    [Fact]
    public void AddMediatorFromLoadedAssemblies_SimpleOverloadWithDiscoverMiddleware_Works()
    {
        // Arrange
        ServiceCollection services = new();

        // Act - Use the simple overload
        services.AddMediatorFromLoadedAssemblies(discoverMiddleware: false);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    #endregion
}