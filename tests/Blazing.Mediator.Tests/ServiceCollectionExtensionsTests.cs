using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for ServiceCollectionExtensions registration methods
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMediator_WithAssemblies_RegistersMediator()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();
        Assembly? assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
        mediator.ShouldBeOfType<Mediator>();
    }

    [Fact]
    public void AddMediator_WithAssemblies_RegistersHandlers()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();
        Assembly? assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestRegistrationCommand>? commandHandler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();
        IRequestHandler<TestRegistrationQuery, string>? queryHandler = serviceProvider.GetService<IRequestHandler<TestRegistrationQuery, string>>();

        commandHandler.ShouldNotBeNull();
        queryHandler.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithTypes_RegistersFromCorrectAssemblies()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestRegistrationCommand), typeof(ServiceCollectionExtensionsTests));

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IRequestHandler<TestRegistrationCommand>? handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.ShouldNotBeNull();
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullAssemblies_RegistersMediatorOnly()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediator((Assembly[])null!);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithEmptyAssemblies_RegistersMediatorOnly()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediator(Array.Empty<Assembly>());

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullTypes_RegistersMediatorOnly()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediator((Type[])null!);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromCallingAssembly_RegistersMediatorAndHandlers()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediatorFromCallingAssembly();

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IRequestHandler<TestRegistrationCommand>? handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.ShouldNotBeNull();
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithoutFilter_RegistersFromAllAssemblies()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies();

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersFromFilteredAssemblies()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies(assembly =>
            assembly.FullName?.Contains("Blazing.Mediator.Tests") == true);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IMediator? mediator = serviceProvider.GetService<IMediator>();
        IRequestHandler<TestRegistrationCommand>? handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.ShouldNotBeNull();
        handler.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithDuplicateAssemblies_RegistersHandlersOnce()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();
        Assembly? assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly, assembly); // Same assembly twice

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IEnumerable<IRequestHandler<TestRegistrationCommand>>? handlers = serviceProvider.GetServices<IRequestHandler<TestRegistrationCommand>>();
        handlers.Count().ShouldBe(1); // Should only register once
    }

    [Fact]
    public void AddMediator_WithMultipleInterfaces_RegistersAllInterfaces()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();
        Assembly? assembly = typeof(TestMultiInterfaceHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestMultiCommand>? commandHandler = serviceProvider.GetService<IRequestHandler<TestMultiCommand>>();
        IRequestHandler<TestMultiQuery, string>? queryHandler = serviceProvider.GetService<IRequestHandler<TestMultiQuery, string>>();

        commandHandler.ShouldNotBeNull();
        queryHandler.ShouldNotBeNull();
        commandHandler.ShouldBeSameAs(queryHandler); // Same instance
    }

    [Fact]
    public void AddMediator_WithAbstractHandler_DoesNotRegisterHandler()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();
        Assembly? assembly = typeof(TestAbstractHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestAbstractCommand>? handler = serviceProvider.GetService<IRequestHandler<TestAbstractCommand>>();
        handler.ShouldBeNull();
    }

    [Fact]
    public void AddMediator_WithInterfaceHandler_DoesNotRegisterHandler()
    {
        // Arrange
        ServiceCollection? services = new ServiceCollection();
        Assembly? assembly = typeof(ITestInterfaceHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        ServiceProvider? serviceProvider = services.BuildServiceProvider();
        IRequestHandler<TestInterfaceCommand>? handler = serviceProvider.GetService<IRequestHandler<TestInterfaceCommand>>();
        handler.ShouldBeNull();
    }
}

// Test types for registration tests

// Multi-interface handler test

// Abstract handler test

// Interface handler test