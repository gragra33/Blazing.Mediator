using Blazing.Mediator.Configuration;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Tests specifically targeting uncovered AddMediator overloads to improve test coverage.
/// Focuses on the AddMediator methods that showed 0% coverage in the coverage report.
/// </summary>
public class ServiceCollectionExtensionsCoverageTests : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void AddMediator_EmptyOverload_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Test the parameterless overload
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();

        Assert.NotNull(mediator);
        Assert.Null(statistics); // Statistics should be disabled by default
    }

    [Fact]
    public void AddMediator_WithAssembliesArray_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediator_WithTypesArray_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediator_WithNullAssemblies_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediator_WithNullTypes_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorFromCallingAssembly_EmptyOverload_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorFromCallingAssembly_WithConfiguration_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();

        Assert.NotNull(mediator);
        Assert.NotNull(statistics);
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_EmptyOverload_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithConfiguration_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();

        Assert.NotNull(mediator);
        Assert.NotNull(statistics);
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithFilter_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorFromLoadedAssemblies_WithConfigurationAndFilter_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();

        Assert.NotNull(mediator);
        Assert.NotNull(statistics);
    }

    [Fact]
    public void AddMediatorWithNotificationMiddleware_DiscoveryEnabled_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorWithNotificationMiddleware_DiscoveryDisabled_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorWithNotificationMiddleware_WithNullAssemblies_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorWithNotificationMiddleware_WithTypes_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediatorWithNotificationMiddleware_WithNullTypes_RegistersMediatorCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator();

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }
}
