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
        var services = new ServiceCollection();
        var assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();
        mediator.Should().BeOfType<Mediator>();
    }

    [Fact]
    public void AddMediator_WithAssemblies_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var commandHandler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();
        var queryHandler = serviceProvider.GetService<IRequestHandler<TestRegistrationQuery, string>>();

        commandHandler.Should().NotBeNull();
        queryHandler.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithTypes_RegistersFromCorrectAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestRegistrationCommand), typeof(ServiceCollectionExtensionsTests));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        var handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.Should().NotBeNull();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullAssemblies_RegistersMediatorOnly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator((Assembly[])null!);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithEmptyAssemblies_RegistersMediatorOnly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(Array.Empty<Assembly>());

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullTypes_RegistersMediatorOnly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator((Type[])null!);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddMediatorFromCallingAssembly_RegistersMediatorAndHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromCallingAssembly();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        var handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.Should().NotBeNull();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithoutFilter_RegistersFromAllAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersFromFilteredAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies(assembly =>
            assembly.FullName?.Contains("Blazing.Mediator.Tests") == true);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        var handler = serviceProvider.GetService<IRequestHandler<TestRegistrationCommand>>();

        mediator.Should().NotBeNull();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddMediator_WithDuplicateAssemblies_RegistersHandlersOnce()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestRegistrationCommand).Assembly;

        // Act
        services.AddMediator(assembly, assembly); // Same assembly twice

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<IRequestHandler<TestRegistrationCommand>>();
        handlers.Should().HaveCount(1); // Should only register once
    }

    [Fact]
    public void AddMediator_WithMultipleInterfaces_RegistersAllInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestMultiInterfaceHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var commandHandler = serviceProvider.GetService<IRequestHandler<TestMultiCommand>>();
        var queryHandler = serviceProvider.GetService<IRequestHandler<TestMultiQuery, string>>();

        commandHandler.Should().NotBeNull();
        queryHandler.Should().NotBeNull();
        commandHandler.Should().BeSameAs(queryHandler); // Same instance
    }

    [Fact]
    public void AddMediator_WithAbstractHandler_DoesNotRegisterHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestAbstractHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IRequestHandler<TestAbstractCommand>>();
        handler.Should().BeNull();
    }

    [Fact]
    public void AddMediator_WithInterfaceHandler_DoesNotRegisterHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(ITestInterfaceHandler).Assembly;

        // Act
        services.AddMediator(assembly);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IRequestHandler<TestInterfaceCommand>>();
        handler.Should().BeNull();
    }
}

// Test types for registration tests

// Multi-interface handler test

// Abstract handler test

// Interface handler test