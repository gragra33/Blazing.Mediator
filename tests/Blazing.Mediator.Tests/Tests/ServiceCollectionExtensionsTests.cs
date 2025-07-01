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
}