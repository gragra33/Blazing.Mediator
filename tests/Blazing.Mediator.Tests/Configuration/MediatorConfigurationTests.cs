using Blazing.Mediator.Configuration;
using Blazing.Mediator.OpenTelemetry;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.Configuration;

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
}

// Test types for assembly scanning
public record TestQuery(string Value) : IRequest<string>;
public record TestCommand(string Value) : IRequest;

public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled: {request.Value}");
    }
}

public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}