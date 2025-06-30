using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;
using System.Reflection;
using Shouldly;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Tests for ServiceCollectionExtensions edge cases and error scenarios
/// </summary>
public class ServiceCollectionExtensionsEdgeCaseTests
{
    [Fact]
    public void AddMediator_WithNullAssemblies_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddMediator((Assembly[])null!);
        services.AddMediator(configureMiddleware: null, (Assembly[])null!);
    }

    [Fact]
    public void AddMediator_WithEmptyAssemblies_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddMediator(Array.Empty<Assembly>());
        services.AddMediator(configureMiddleware: null, Array.Empty<Assembly>());
    }

    [Fact]
    public void AddMediator_WithNullTypes_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddMediator((Type[])null!);
        services.AddMediator(configureMiddleware: null, (Type[])null!);
    }

    [Fact]
    public void AddMediator_WithEmptyTypes_DoesNotThrow()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddMediator(Array.Empty<Type>());
        services.AddMediator(configureMiddleware: null, Array.Empty<Type>());
    }

    [Fact]
    public void AddMediator_WithDuplicateAssemblies_DeduplicatesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        Assembly testAssembly = typeof(TestCommandHandler).Assembly;

        // Act
        services.AddMediator(testAssembly, testAssembly, testAssembly); // Same assembly multiple times

        // Assert - Should not throw and should work correctly
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithDuplicateTypes_DeduplicatesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(TestCommandHandler), typeof(TestCommandHandler)); // Same type multiple times

        // Assert - Should not throw and should work correctly
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromCallingAssembly_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediatorFromCallingAssembly();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromCallingAssembly_WithConfiguration_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

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

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithoutFilter_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies();

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies(assembly => 
            assembly.GetName().Name?.Contains("Blazing.Mediator") == true);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_RegistersServicesCorrectly()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediatorFromLoadedAssemblies(
            config => config.PipelineBuilder.AddMiddleware<FirstQueryMiddleware>(),
            assembly => assembly.GetName().Name?.Contains("Blazing.Mediator") == true);

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void RegisterHandlers_WithAbstractClass_SkipsAbstractTypes()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(AbstractHandler).Assembly);

        // Assert - Should not register abstract handler
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var handlerServices = serviceProvider.GetServices<IRequestHandler<TestCommand>>();
        handlerServices.ShouldNotContain(h => h.GetType() == typeof(AbstractHandler));
    }

    [Fact]
    public void RegisterHandlers_WithInterface_SkipsInterfaceTypes()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(ITestInterface).Assembly);

        // Assert - Should not register interface as handler
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        // Should not throw when trying to get mediator
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void RegisterHandlers_WithAlreadyRegisteredHandler_DoesNotDuplicate()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();
        services.AddScoped<TestCommandHandler>(); // Pre-register the handler

        // Act
        services.AddMediator(typeof(TestCommandHandler).Assembly);

        // Assert - Should only have one registration
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<IRequestHandler<TestCommand>>();
        handlers.Count().ShouldBe(1);
    }

    [Fact]
    public void RegisterHandlers_WithMultipleInterfaces_RegistersAllInterfaces()
    {
        // Arrange
        ServiceCollection services = new ServiceCollection();

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

// Test types for edge cases
public abstract class AbstractHandler : IRequestHandler<TestCommand>
{
    public abstract Task Handle(TestCommand request, CancellationToken cancellationToken = default);
}

public interface ITestInterface : IRequestHandler<TestCommand>
{
    // Interface should be skipped during registration
}
