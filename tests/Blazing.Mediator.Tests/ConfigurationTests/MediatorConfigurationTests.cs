using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Configuration;
using Blazing.Mediator.OpenTelemetry;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Tests for MediatorConfiguration integration with telemetry and statistics options.
/// Validates that configuration methods work correctly and integrate with service registration.
/// </summary>
public class MediatorConfigurationTests : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public void WithTelemetry_DefaultOptions_ShouldRegisterTelemetryOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithTelemetry();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var telemetryOptions = _serviceProvider.GetService<MediatorTelemetryOptions>();
        Assert.NotNull(telemetryOptions);
        Assert.True(telemetryOptions.Enabled);
    }

    [Fact]
    public void WithTelemetry_CustomOptions_ShouldConfigureCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithTelemetry(options =>
            {
                options.Enabled = false;
                options.CaptureMiddlewareDetails = false;
                options.MaxExceptionMessageLength = 100;
                options.PacketLevelTelemetryEnabled = true;
            });
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var telemetryOptions = _serviceProvider.GetService<MediatorTelemetryOptions>();
        Assert.NotNull(telemetryOptions);
        Assert.False(telemetryOptions.Enabled);
        Assert.False(telemetryOptions.CaptureMiddlewareDetails);
        Assert.Equal(100, telemetryOptions.MaxExceptionMessageLength);
        Assert.True(telemetryOptions.PacketLevelTelemetryEnabled);
    }

    [Fact]
    public void WithTelemetry_PreConfiguredOptions_ShouldUseProvidedOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var customOptions = new MediatorTelemetryOptions
        {
            Enabled = true,
            PacketLevelTelemetryEnabled = true,
            PacketTelemetryBatchSize = 5,
            CaptureMiddlewareDetails = false
        };

        // Act
        services.AddMediator(config =>
        {
            config.WithTelemetry(customOptions);
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var telemetryOptions = _serviceProvider.GetService<MediatorTelemetryOptions>();
        Assert.NotNull(telemetryOptions);
        Assert.True(telemetryOptions.Enabled);
        Assert.True(telemetryOptions.PacketLevelTelemetryEnabled);
        Assert.Equal(5, telemetryOptions.PacketTelemetryBatchSize);
        Assert.False(telemetryOptions.CaptureMiddlewareDetails);
    }

    [Fact]
    public void WithStatisticsTracking_DefaultOptions_ShouldRegisterStatisticsOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();
        Assert.NotNull(statistics);
    }

    [Fact]
    public void WithStatisticsTracking_CustomOptions_ShouldConfigureCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = false;
                options.EnableMiddlewareMetrics = true;
                options.EnablePerformanceCounters = false;
                options.MaxTrackedRequestTypes = 50;
            });
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();
        Assert.NotNull(statistics);
    }

    [Fact]
    public void WithBothTelemetryAndStatistics_ShouldConfigureBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithTelemetry(telemetry =>
            {
                telemetry.Enabled = true;
                telemetry.PacketLevelTelemetryEnabled = true;
            })
            .WithStatisticsTracking(stats =>
            {
                stats.EnableRequestMetrics = true;
                stats.EnableNotificationMetrics = true;
            });
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        
        var telemetryOptions = _serviceProvider.GetService<MediatorTelemetryOptions>();
        Assert.NotNull(telemetryOptions);
        Assert.True(telemetryOptions.Enabled);
        Assert.True(telemetryOptions.PacketLevelTelemetryEnabled);

        var statistics = _serviceProvider.GetService<MediatorStatistics>();
        Assert.NotNull(statistics);
    }

    [Fact]
    public void WithMiddlewareDiscovery_ShouldEnableDiscovery()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithMiddlewareDiscovery();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void FluentConfiguration_ShouldChainMethods()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert - Should not throw
        var result = services.AddMediator(config =>
        {
            config.WithTelemetry()
                  .WithStatisticsTracking()
                  .WithMiddlewareDiscovery()
                  .WithNotificationMiddlewareDiscovery();
        }, typeof(TestQuery).Assembly);

        Assert.NotNull(result);
    }

    [Fact]
    public void Configuration_WithoutTelemetry_ShouldNotRegisterTelemetryOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var telemetryOptions = _serviceProvider.GetService<MediatorTelemetryOptions>();
        Assert.Null(telemetryOptions);
    }

    [Fact]
    public void Configuration_WithoutStatistics_ShouldNotRegisterStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithTelemetry();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var statistics = _serviceProvider.GetService<MediatorStatistics>();
        Assert.Null(statistics);
    }

    [Fact]
    public void AddMiddleware_Generic_ShouldRegisterMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.AddMiddleware<TestMiddleware>();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var middleware = _serviceProvider.GetService<TestMiddleware>();
        Assert.NotNull(middleware);
    }

    [Fact]
    public void AddMiddleware_Type_ShouldRegisterMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.AddMiddleware(typeof(TestMiddleware));
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var middleware = _serviceProvider.GetService<TestMiddleware>();
        Assert.NotNull(middleware);
    }

    [Fact]
    public void AddMiddleware_MultipleTypes_ShouldRegisterAllMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.AddMiddleware(typeof(TestMiddleware), typeof(SecondTestMiddleware));
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var middleware1 = _serviceProvider.GetService<TestMiddleware>();
        var middleware2 = _serviceProvider.GetService<SecondTestMiddleware>();
        Assert.NotNull(middleware1);
        Assert.NotNull(middleware2);
    }

    [Fact]
    public void AddMiddleware_WithoutServices_ShouldNotThrow()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert - Should not throw when no services collection is provided
        var result = config.AddMiddleware<TestMiddleware>();
        Assert.NotNull(result);
        Assert.Same(config, result); // Should return same instance for chaining
    }

    [Fact]
    public void AddMiddleware_DuplicateRegistration_ShouldNotRegisterTwice()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.AddMiddleware<TestMiddleware>();
            config.AddMiddleware<TestMiddleware>(); // Add same middleware twice
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var middlewareServices = services.Where(s => s.ServiceType == typeof(TestMiddleware)).ToList();
        Assert.Single(middlewareServices); // Should only be registered once
    }

    [Fact]
    public void AddNotificationMiddleware_ChainableReturnsConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddNotificationMiddleware<TestNotificationMiddleware>();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void AddMiddleware_NullSingleTypeThrowsException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert - Currently throws NullReferenceException, which indicates a potential improvement needed
        Assert.Throws<NullReferenceException>(() => config.AddMiddleware((Type)null!));
    }

    [Fact]
    public void AddMiddleware_NullParamsArrayThrowsException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert - Currently throws NullReferenceException, which indicates a potential improvement needed
        Assert.Throws<NullReferenceException>(() => config.AddMiddleware((Type[])null!));
    }

    [Fact]
    public void AddNotificationMiddleware_NullSingleTypeThrowsException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert - Currently throws NullReferenceException, which indicates a potential improvement needed
        Assert.Throws<NullReferenceException>(() => config.AddNotificationMiddleware((Type)null!));
    }

    [Fact]
    public void AddNotificationMiddleware_NullParamsArrayThrowsException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert - Currently throws NullReferenceException, which indicates a potential improvement needed
        Assert.Throws<NullReferenceException>(() => config.AddNotificationMiddleware((Type[])null!));
    }

    [Fact]
    public void WithMiddlewareDiscovery_ReturnsConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithMiddlewareDiscovery();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void WithNotificationMiddlewareDiscovery_ReturnsConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithNotificationMiddlewareDiscovery();

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void WithMiddlewareDiscovery_SetsDiscoveryFlag()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithMiddlewareDiscovery();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);

        // Verify that middleware discovery worked by checking if built-in middleware is registered
        var inspector = _serviceProvider.GetService<IMiddlewarePipelineInspector>();
        Assert.NotNull(inspector);
    }

    [Fact]
    public void WithNotificationMiddlewareDiscovery_SetsDiscoveryFlag()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithNotificationMiddlewareDiscovery();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);

        // Verify that notification middleware discovery worked
        var inspector = _serviceProvider.GetService<INotificationMiddlewarePipelineInspector>();
        Assert.NotNull(inspector);
    }

    [Fact]
    public void WithBothDiscoveryMethods_ShouldEnableBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddMediator(config =>
        {
            config.WithMiddlewareDiscovery()
                  .WithNotificationMiddlewareDiscovery();
        }, typeof(TestQuery).Assembly);

        // Assert
        _serviceProvider = services.BuildServiceProvider();
        var mediator = _serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);

        // Verify both inspectors are available
        var requestInspector = _serviceProvider.GetService<IMiddlewarePipelineInspector>();
        var notificationInspector = _serviceProvider.GetService<INotificationMiddlewarePipelineInspector>();
        
        Assert.NotNull(requestInspector);
        Assert.NotNull(notificationInspector);
    }

    [Fact]
    public void DiscoveryMethods_ShouldBeChainable()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert - Should be able to chain multiple discovery methods
        var result = config.WithMiddlewareDiscovery()
                          .WithNotificationMiddlewareDiscovery()
                          .WithTelemetry()
                          .WithStatisticsTracking();

        Assert.Same(config, result);
    }
}

// Test types for assembly scanning

// Test middleware classes