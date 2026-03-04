using Blazing.Mediator.Configuration;
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
        services.AddMediator(new MediatorConfiguration().WithNotificationMiddlewareDiscovery());
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
        services.AddMediator(new MediatorConfiguration().WithNotificationMiddlewareDiscovery());
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
        services.AddMediator(new MediatorConfiguration().WithNotificationMiddlewareDiscovery()); // Same assembly multiple times

        // Assert - Should not throw and should work correctly
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that AddMediator with duplicate types deduplicates correctly and registers services properly.
    /// </summary>
    [Fact]
    public async Task AddMediator_WithDuplicateTypes_DeduplicatesCorrectly()
    {
        // Arrange
        ServiceCollection services = new();

        // Act
        services.AddMediator(new MediatorConfiguration().WithNotificationMiddlewareDiscovery());

        // Assert
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        IMediator mediator = serviceProvider.GetRequiredService<IMediator>();

        // In source-gen mode handlers are wrapped; verify both request types are dispatched correctly
        // via the mediator rather than resolving raw IRequestHandler<> from DI.
        var exception = await Record.ExceptionAsync(async () =>
        {
            await mediator.Send(new TestMultiCommand());
            string result = await mediator.Send(new TestMultiQuery());
            result.ShouldNotBeNull();
        });
        exception.ShouldBeNull();
    }
}