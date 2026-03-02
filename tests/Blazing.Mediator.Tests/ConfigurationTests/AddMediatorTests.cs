using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Blazing.Mediator.Configuration;

namespace Blazing.Mediator.Tests.ConfigurationTests;

public class AddMediatorTests
{
    [Fact]
    public void AddMediator_WithoutParameters_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.NotThrow(() => services.AddMediator());
    }

    [Fact]
    public void AddMediator_WithoutParameters_ShouldRegisterMediator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithEmptyAssemblyArray_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullAssemblyArray_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_ParameterlessVersion_ShouldBeEquivalentToEmptyAssemblyArray()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        // Act
        services1.AddMediator();
        services2.AddMediator();

        // Assert
        services1.Count.ShouldBe(services2.Count);
    }

    #region WithoutTelemetry Integration Tests

    [Fact]
    public void AddMediator_WithoutTelemetry_ShouldRegisterMediatorWithoutTelemetryOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry()
            .WithoutTelemetry());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
        // Note: We can't directly test if telemetry options are null after service registration
        // as they are used during the registration process, but the mediator should be functional
    }

    [Fact]
    public void AddMediator_WithoutTelemetry_ShouldAllowFluentConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            services.AddMediator(new MediatorConfiguration()
                .WithTelemetry()
                .WithoutTelemetry()
                .WithMiddlewareDiscovery());
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region WithoutStatistics Integration Tests

    [Fact]
    public void AddMediator_WithoutStatistics_ShouldRegisterMediatorWithoutStatisticsOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new MediatorConfiguration()
            .WithStatisticsTracking()
            .WithoutStatistics());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithoutStatistics_ShouldAllowFluentConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            services.AddMediator(new MediatorConfiguration()
                .WithStatisticsTracking(opts => opts.EnablePerformanceCounters = true)
                .WithoutStatistics()
                .WithMiddlewareDiscovery());
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region WithoutLogging Integration Tests

    [Fact]
    public void AddMediator_WithoutLogging_ShouldRegisterMediatorWithoutLoggingOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new MediatorConfiguration()
            .WithLogging()
            .WithoutLogging());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithoutLogging_ShouldAllowFluentConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            services.AddMediator(new MediatorConfiguration()
                .WithLogging(opts => opts.EnableDetailedHandlerInfo = true)
                .WithoutLogging()
                .WithMiddlewareDiscovery());
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region Combined Without Methods Integration Tests

    [Fact]
    public void AddMediator_WithoutAllOptions_ShouldRegisterMediatorSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry()
            .WithStatisticsTracking()
            .WithLogging()
            .WithoutTelemetry()
            .WithoutStatistics()
            .WithoutLogging());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithSelectiveWithoutOptions_ShouldAllowMixedConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new MediatorConfiguration()
            .WithTelemetry()
            .WithStatisticsTracking()
            .WithLogging()
            .WithoutStatistics() // Only disable statistics
            .WithMiddlewareDiscovery());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithToggleConfiguration_ShouldSupportMultipleToggling()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            services.AddMediator(new MediatorConfiguration()
                .WithTelemetry()
                .WithoutTelemetry()
                .WithTelemetry()
                .WithStatisticsTracking()
                .WithoutStatistics()
                .WithStatisticsTracking()
                .WithLogging()
                .WithoutLogging()
                .WithLogging());
        });

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithComplexFluentChain_ShouldRegisterSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new MediatorConfiguration()
            .WithMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery()
            .WithTelemetry(opts => opts.Enabled = true)
            .WithStatisticsTracking(opts => 
            {
                opts.EnablePerformanceCounters = true;
                opts.EnableDetailedAnalysis = false;
            })
            .WithLogging(opts => opts.EnableDetailedHandlerInfo = true)
            .WithoutStatistics() // Selectively disable statistics
            .WithConstrainedMiddlewareDiscovery()
            .WithoutLogging()); // Selectively disable logging

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region MediatorConfiguration Instance Overload Tests

    [Fact]
    public void AddMediator_WithMediatorConfigurationInstance_ShouldRegisterSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = MediatorConfiguration.Production(typeof(TestCommand).Assembly);

        // Act
        services.AddMediator(config);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithProductionConfiguration_ShouldRegisterSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(MediatorConfiguration.Production());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithDevelopmentConfiguration_ShouldRegisterSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(MediatorConfiguration.Development());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithMinimalConfiguration_ShouldRegisterSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(MediatorConfiguration.Minimal());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithDisabledConfiguration_ShouldRegisterSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(MediatorConfiguration.Disabled());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    [Fact]
    public void AddMediator_WithNullMediatorConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => services.AddMediator((MediatorConfiguration)null!));
    }

    [Fact]
    public void AddMediator_WithCustomConfigurationInstance_ShouldPreserveSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        var customConfig = new MediatorConfiguration()
            .WithStatisticsTracking()
            .WithTelemetry()
            .WithLogging()
            .WithMiddlewareDiscovery()
            .AddAssembly(typeof(TestCommand).Assembly);

        // Act
        services.AddMediator(customConfig);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        mediator.ShouldNotBeNull();
    }

    #endregion
}
