using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for edge cases in the ServiceCollectionExtensions functionality.
/// Covers various registration scenarios, duplicate handlers, and boundary conditions.
/// </summary>
public class ServiceCollectionExtensionsEdgeCaseTests
{
    /// <summary>
    /// Tests that AddMediator with null assemblies does not throw an exception.
    /// </summary>
    [Fact]
    public void AddMediator_WithNullAssemblies_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();

        // Act & Assert - Should not throw
        services.AddMediator((Assembly[])null!);
        services.AddMediator(config => { }, (Assembly[])null!);
    }

    /// <summary>
    /// Tests that AddMediator with empty assemblies does not throw an exception.
    /// </summary>
    [Fact]
    public void AddMediator_WithEmptyAssemblies_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();

        // Act & Assert - Should not throw
        services.AddMediator(Array.Empty<Assembly>());
        services.AddMediator(config => { }, Array.Empty<Assembly>());
    }

    /// <summary>
    /// Tests that AddMediator with null types does not throw an exception.
    /// </summary>
    [Fact]
    public void AddMediator_WithNullTypes_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();

        // Act & Assert - Should not throw
        services.AddMediator((Type[])null!);
        services.AddMediator(config => { }, (Type[])null!);
    }

    /// <summary>
    /// Tests that AddMediator with empty types does not throw an exception.
    /// </summary>
    [Fact]
    public void AddMediator_WithEmptyTypes_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new();

        // Act & Assert - Should not throw
        services.AddMediator(Array.Empty<Type>());
        services.AddMediator(config => { }, Array.Empty<Type>());
    }

    /// <summary>
    /// Tests that AddMediator with duplicate assemblies deduplicates correctly and registers services properly.
    /// </summary>
    [Fact]
    public void AddMediator_WithDuplicateAssemblies_DeduplicatesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();
        Assembly testAssembly = typeof(TestCommandHandler).Assembly;

        // Act
        services.AddMediator(testAssembly, testAssembly, testAssembly); // Same assembly multiple times

        // Assert - Should not throw and should work correctly
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with duplicate types deduplicates correctly and registers services properly.
    /// </summary>
    [Fact]
    public void AddMediator_WithDuplicateTypes_DeduplicatesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(typeof(TestCommandHandler), typeof(TestCommandHandler)); // Same type multiple times

        // Assert - Should not throw and should work correctly
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromCallingAssembly registers services correctly.
    /// </summary>
    [Fact]
    public void AddMediatorFromCallingAssembly_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromCallingAssembly();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromCallingAssembly with middleware configuration registers services correctly.
    /// </summary>
    [Fact]
    public void AddMediatorFromCallingAssembly_WithConfiguration_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromCallingAssembly(config =>
        {
            config.PipelineBuilder.AddMiddleware<FirstQueryMiddleware>();
        });

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromLoadedAssemblies without filter registers services correctly.
    /// </summary>
    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithoutFilter_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromLoadedAssemblies();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromLoadedAssemblies with assembly filter registers services correctly.
    /// </summary>
    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromLoadedAssemblies(assembly =>
            assembly.GetName().Name?.Contains("Blazing.Mediator") == true);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediatorFromLoadedAssemblies with configuration and filter registers services correctly.
    /// </summary>
    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediatorFromLoadedAssemblies(
            config => config.PipelineBuilder.AddMiddleware<FirstQueryMiddleware>(),
            assembly => assembly.GetName().Name?.Contains("Blazing.Mediator") == true);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that RegisterHandlers with abstract class skips abstract types during registration.
    /// </summary>
    [Fact]
    public void RegisterHandlers_WithAbstractClass_SkipsAbstractTypes()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(typeof(AbstractHandler).Assembly);

        // Assert - Should not register abstract handler
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var handlerServices = serviceProvider.GetServices<IRequestHandler<TestCommand>>();
        handlerServices.ShouldNotContain(h => h.GetType() == typeof(AbstractHandler));
    }

    /// <summary>
    /// Tests that RegisterHandlers with interface types skips interface types during registration.
    /// </summary>
    [Fact]
    public void RegisterHandlers_WithInterface_SkipsInterfaceTypes()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(typeof(ITestInterface).Assembly);

        // Assert - Should not register interface as handler
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        // Should not throw when trying to get mediator
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that RegisterHandlers with already registered handler does not create duplicates.
    /// </summary>
    [Fact]
    public void RegisterHandlers_WithAlreadyRegisteredHandler_DoesNotDuplicate()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddScoped<TestCommandHandler>(); // Pre-register the handler

        // Act
        services.AddMediator(typeof(TestCommandHandler).Assembly);

        // Assert - Should only have one registration
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<IRequestHandler<TestCommand>>();
        handlers.Count().ShouldBe(1);
    }

    /// <summary>
    /// Tests that RegisterHandlers with multiple interfaces registers all implemented interfaces correctly.
    /// </summary>
    [Fact]
    public void RegisterHandlers_WithMultipleInterfaces_RegistersAllInterfaces()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(typeof(TestMultiInterfaceHandler).Assembly);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // Should be able to resolve both interfaces that TestMultiInterfaceHandler implements
        var commandHandler = serviceProvider.GetService<IRequestHandler<TestMultiCommand>>();
        var queryHandler = serviceProvider.GetService<IRequestHandler<TestMultiQuery, string>>();

        commandHandler.ShouldNotBeNull();
        queryHandler.ShouldNotBeNull();
        commandHandler.ShouldBeOfType<TestMultiInterfaceHandler>();
        queryHandler.ShouldBeOfType<TestMultiInterfaceHandler>();
    }
}