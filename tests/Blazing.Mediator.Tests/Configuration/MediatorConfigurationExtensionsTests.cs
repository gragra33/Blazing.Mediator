using Blazing.Mediator.Configuration;
using Microsoft.Extensions.Configuration;

namespace Blazing.Mediator.Tests.Configuration;

/// <summary>
/// Tests for MediatorConfigurationExtensions
/// </summary>
public class MediatorConfigurationExtensionsTests
{
    [Fact]
    public void WithConfiguration_WithDefaultSection_ReturnsConfigUnchangedWhenSectionDoesNotExist()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = config.WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        Assert.Null(config.StatisticsOptions);
        Assert.Null(config.TelemetryOptions);
        Assert.Null(config.LoggingOptions);
    }

    [Fact]
    public void WithConfiguration_WithValidStatisticsSection_AppliesStatisticsConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Statistics:EnableRequestMetrics"] = "true",
            ["Blazing:Mediator:Statistics:EnableNotificationMetrics"] = "false",
            ["Blazing:Mediator:Statistics:MetricsRetentionPeriod"] = "02:00:00"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.True(config.StatisticsOptions.EnableRequestMetrics);
        Assert.False(config.StatisticsOptions.EnableNotificationMetrics);
        Assert.Equal(TimeSpan.FromHours(2), config.StatisticsOptions.MetricsRetentionPeriod);
    }

    [Fact]
    public void WithConfiguration_WithValidTelemetrySection_AppliesTelemetryConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Telemetry:Enabled"] = "true",
            ["Blazing:Mediator:Telemetry:CaptureHandlerDetails"] = "false"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.TelemetryOptions);
        Assert.True(config.TelemetryOptions.Enabled);
        Assert.False(config.TelemetryOptions.CaptureHandlerDetails);
    }

    [Fact]
    public void WithConfiguration_WithValidLoggingSection_AppliesLoggingConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Logging:EnableSend"] = "true",
            ["Blazing:Mediator:Logging:EnablePublish"] = "false"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableSend);
        Assert.False(config.LoggingOptions.EnablePublish);
    }

    [Fact]
    public void WithConfiguration_WithValidDiscoverySection_AppliesDiscoveryConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Discovery:DiscoverMiddleware"] = "true",
            ["Blazing:Mediator:Discovery:DiscoverNotificationMiddleware"] = "false",
            ["Blazing:Mediator:Discovery:DiscoverConstrainedMiddleware"] = "false",
            ["Blazing:Mediator:Discovery:DiscoverNotificationHandlers"] = "true"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        Assert.True(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
        Assert.False(config.DiscoverConstrainedMiddleware);
        Assert.True(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithConfiguration_WithCustomSectionPath_AppliesConfigurationFromCustomPath()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configurationData = new Dictionary<string, string>
        {
            ["MyApp:MediatorSettings:Statistics:EnableRequestMetrics"] = "true"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithConfiguration(configuration, "MyApp:MediatorSettings");

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.True(config.StatisticsOptions.EnableRequestMetrics);
    }

    [Fact]
    public void WithConfiguration_WithPreBoundSection_AppliesConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configSection = new MediatorConfigurationSection
        {
            Statistics = new StatisticsOptions { EnableRequestMetrics = true },
            Discovery = new DiscoveryOptions { DiscoverMiddleware = true }
        };

        // Act
        var result = config.WithConfiguration(configSection);

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.True(config.StatisticsOptions.EnableRequestMetrics);
        Assert.True(config.DiscoverMiddleware);
    }

    [Fact]
    public void WithConfiguration_ThrowsArgumentNullException_WhenConfigurationIsNull()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.WithConfiguration((IConfiguration)null!));
    }

    [Fact]
    public void WithConfiguration_ThrowsArgumentNullException_WhenSectionPathIsNull()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.WithConfiguration(configuration, null!));
    }

    [Fact]
    public void WithConfiguration_ThrowsArgumentException_WhenSectionPathIsEmpty()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => config.WithConfiguration(configuration, ""));
    }

    [Fact]
    public void WithDevelopmentPreset_AppliesDevelopmentConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithDevelopmentPreset();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.DiscoverMiddleware);
        Assert.True(config.DiscoverNotificationMiddleware);
        Assert.True(config.DiscoverConstrainedMiddleware);
        Assert.True(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithProductionPreset_AppliesProductionConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithProductionPreset();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.DiscoverMiddleware);
        Assert.True(config.DiscoverNotificationMiddleware);
        Assert.True(config.DiscoverConstrainedMiddleware);
        Assert.True(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithDisabledPreset_AppliesDisabledConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithDisabledPreset();

        // Assert
        Assert.Same(config, result);
        Assert.Null(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions); // TelemetryOptions.Disabled() creates an instance
        Assert.Null(config.LoggingOptions);
        Assert.False(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
        Assert.False(config.DiscoverConstrainedMiddleware);
        Assert.False(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithEnvironmentPreset_ThrowsArgumentNullException_WhenPresetIsNull()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.WithEnvironmentPreset(null!));
    }

    [Fact]
    public void WithEnvironmentPreset_CopiesAllConfigurationFromPreset()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var preset = new MediatorConfiguration()
            .WithStatisticsTracking(options => options.EnableRequestMetrics = true)
            .WithTelemetry(options => options.Enabled = false)
            .WithLogging(options => options.EnableSend = false)
            .WithMiddlewareDiscovery()
            .WithoutNotificationMiddlewareDiscovery();

        // Act
        var result = config.WithEnvironmentPreset(preset);

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.True(config.StatisticsOptions.EnableRequestMetrics);
        Assert.NotNull(config.TelemetryOptions);
        Assert.False(config.TelemetryOptions.Enabled);
        Assert.NotNull(config.LoggingOptions);
        Assert.False(config.LoggingOptions.EnableSend);
        Assert.True(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
    }

    [Fact]
    public void WithMinimalPreset_AppliesMinimalConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithMinimalPreset();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.Null(config.LoggingOptions);
        Assert.False(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
        Assert.False(config.DiscoverConstrainedMiddleware);
        Assert.True(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithNotificationOptimizedPreset_AppliesNotificationOptimizedConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithNotificationOptimizedPreset();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        Assert.False(config.DiscoverMiddleware);
        Assert.True(config.DiscoverNotificationMiddleware);
        Assert.True(config.DiscoverConstrainedMiddleware);
        Assert.True(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithStreamingOptimizedPreset_AppliesStreamingOptimizedConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithStreamingOptimizedPreset();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
        Assert.False(config.DiscoverConstrainedMiddleware);
        Assert.False(config.DiscoverNotificationHandlers);
    }
}