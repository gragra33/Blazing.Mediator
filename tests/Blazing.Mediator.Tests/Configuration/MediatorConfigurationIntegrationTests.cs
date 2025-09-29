using Blazing.Mediator.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using System.Reflection;

namespace Blazing.Mediator.Tests.Configuration;

/// <summary>
/// Integration tests for MediatorConfigurationExtensions features
/// </summary>
public class MediatorConfigurationIntegrationTests
{
    [Fact]
    public void WithEnvironmentConfiguration_Development_AppliesDevelopmentPresetAndJsonOverrides()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var environment = new HostingEnvironment { EnvironmentName = "Development" };
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Statistics:MetricsRetentionPeriod"] = "03:00:00", // Override development default
            ["Blazing:Mediator:Discovery:DiscoverMiddleware"] = "false" // Override development default
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithEnvironmentConfiguration(configuration, environment);

        // Assert
        Assert.Same(config, result);
        
        // Should have development preset applied
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        
        // Should have JSON overrides applied
        Assert.Equal(TimeSpan.FromHours(3), config.StatisticsOptions.MetricsRetentionPeriod);
        Assert.False(config.DiscoverMiddleware); // Overridden from development default
        
        // Other development defaults should remain - but let's check what the actual values are
        // First let's verify the development preset defaults by testing separately
        var devPreset = MediatorConfiguration.Development();
        Assert.True(devPreset.DiscoverNotificationMiddleware, $"Development preset should have DiscoverNotificationMiddleware=true, but was {devPreset.DiscoverNotificationMiddleware}");
        Assert.True(devPreset.DiscoverConstrainedMiddleware, $"Development preset should have DiscoverConstrainedMiddleware=true, but was {devPreset.DiscoverConstrainedMiddleware}");
        Assert.True(devPreset.DiscoverNotificationHandlers, $"Development preset should have DiscoverNotificationHandlers=true, but was {devPreset.DiscoverNotificationHandlers}");
        
        // Now check our actual configuration
        Assert.True(config.DiscoverNotificationMiddleware, $"Expected DiscoverNotificationMiddleware=true, but was {config.DiscoverNotificationMiddleware}");
        Assert.True(config.DiscoverConstrainedMiddleware, $"Expected DiscoverConstrainedMiddleware=true, but was {config.DiscoverConstrainedMiddleware}");
        Assert.True(config.DiscoverNotificationHandlers, $"Expected DiscoverNotificationHandlers=true, but was {config.DiscoverNotificationHandlers}");
    }

    [Fact]
    public void WithEnvironmentConfiguration_Production_AppliesProductionPresetAndJsonOverrides()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var environment = new HostingEnvironment { EnvironmentName = "Production" };
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Telemetry:PacketLevelTelemetryEnabled"] = "true", // Override production default
            ["Blazing:Mediator:Statistics:EnableDetailedAnalysis"] = "true" // Override production default
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithEnvironmentConfiguration(configuration, environment);

        // Assert
        Assert.Same(config, result);
        
        // Should have production preset applied
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        
        // Should have JSON overrides applied
        Assert.True(config.TelemetryOptions.PacketLevelTelemetryEnabled);
        Assert.True(config.StatisticsOptions.EnableDetailedAnalysis);
    }

    [Fact]
    public void WithEnvironmentConfiguration_Testing_AppliesDisabledPreset()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var environment = new HostingEnvironment { EnvironmentName = "Testing" };
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = config.WithEnvironmentConfiguration(configuration, environment);

        // Assert
        Assert.Same(config, result);
        
        // Should have disabled preset applied
        Assert.Null(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions); // TelemetryOptions.Disabled() creates an instance
        Assert.Null(config.LoggingOptions);
        Assert.False(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
        Assert.False(config.DiscoverConstrainedMiddleware);
        Assert.False(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void WithEnvironmentConfiguration_UnknownEnvironment_AppliesMinimalPreset()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var environment = new HostingEnvironment { EnvironmentName = "Staging" };
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = config.WithEnvironmentConfiguration(configuration, environment);

        // Assert
        Assert.Same(config, result);
        
        // Should have minimal preset applied
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.Null(config.LoggingOptions);
        Assert.False(config.DiscoverMiddleware);
        Assert.False(config.DiscoverNotificationMiddleware);
        Assert.False(config.DiscoverConstrainedMiddleware);
        Assert.True(config.DiscoverNotificationHandlers);
    }

    [Fact]
    public void ValidateForEnvironment_Production_ThrowsForVerboseLogging()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableDetailedHandlerInfo = true;
                options.EnableConstraintLogging = true;
                // Disable the combination that causes warnings
                options.EnableMiddlewareRoutingLogging = false;
                options.EnablePerformanceTiming = false;
            })
            .AddAssembly(Assembly.GetExecutingAssembly());
        var environment = new HostingEnvironment { EnvironmentName = "Production" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.ValidateForEnvironment(environment));
        Assert.Contains("Production environment should not have detailed logging enabled", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_Production_ThrowsForPacketLevelTelemetry()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.PacketLevelTelemetryEnabled = true;
            })
            .AddAssembly(Assembly.GetExecutingAssembly());
        var environment = new HostingEnvironment { EnvironmentName = "Production" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.ValidateForEnvironment(environment));
        Assert.Contains("Production environment should not have packet-level telemetry enabled", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_Production_ThrowsForShortRetentionPeriod()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(options =>
            {
                options.MetricsRetentionPeriod = TimeSpan.FromMinutes(30);
                options.CleanupInterval = TimeSpan.FromMinutes(15); // Make cleanup interval shorter
            })
            .AddAssembly(Assembly.GetExecutingAssembly());
        var environment = new HostingEnvironment { EnvironmentName = "Production" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.ValidateForEnvironment(environment));
        Assert.Contains("Production environment should have metrics retention period of at least 1 hour", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_Development_ThrowsWhenNoLogging()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithoutLogging()
            .AddAssembly(Assembly.GetExecutingAssembly());
        var environment = new HostingEnvironment { EnvironmentName = "Development" };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.ValidateForEnvironment(environment));
        Assert.Contains("Development environment should have logging enabled", exception.Message);
    }

    [Fact]
    public void ValidateForEnvironment_ValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithLogging(options =>
            {
                options.EnableSend = true;
                options.EnablePublish = true;
                // Don't enable the combination that causes validation warnings
                options.EnableConstraintLogging = false;
                options.EnableMiddlewareRoutingLogging = false;
                options.EnablePerformanceTiming = true;
            })
            .WithStatisticsTracking()
            .WithTelemetry()
            .AddAssembly(Assembly.GetExecutingAssembly());
        var environment = new HostingEnvironment { EnvironmentName = "Development" };

        // Act
        var result = config.ValidateForEnvironment(environment);

        // Assert
        Assert.Same(config, result);
    }

    [Fact]
    public void GetDiagnostics_ReturnsComprehensiveDiagnostics()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithStatisticsTracking()
            .WithTelemetry()
            .WithLogging(options =>
            {
                // Use safe logging combination to avoid warnings
                options.EnableConstraintLogging = false;
                options.EnableMiddlewareRoutingLogging = false;
                options.EnablePerformanceTiming = true;
            })
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery()
            .AddAssembly(Assembly.GetExecutingAssembly());
        var environment = new HostingEnvironment { EnvironmentName = "Development" };

        // Act
        var diagnostics = config.GetDiagnostics(environment);

        // Assert
        Assert.Equal("Development", diagnostics.Environment);
        Assert.True(diagnostics.IsValid);
        Assert.Empty(diagnostics.ValidationErrors);
        Assert.Equal(1, diagnostics.AssemblyCount);
        Assert.True(diagnostics.HasStatistics);
        Assert.True(diagnostics.HasTelemetry);
        Assert.True(diagnostics.HasLogging);
        Assert.True(diagnostics.StatisticsEnabled);
        Assert.True(diagnostics.TelemetryEnabled);
        Assert.True(diagnostics.StreamingTelemetryEnabled);
        Assert.NotNull(diagnostics.StatisticsRetentionPeriod);
        
        Assert.True(diagnostics.DiscoverySettings.DiscoverMiddleware);
        Assert.True(diagnostics.DiscoverySettings.DiscoverNotificationMiddleware);
        Assert.True(diagnostics.DiscoverySettings.DiscoverConstrainedMiddleware);
        Assert.True(diagnostics.DiscoverySettings.DiscoverNotificationHandlers);
    }

    [Fact]
    public void GetDiagnostics_InvalidConfiguration_ReportsValidationErrors()
    {
        // Arrange - Create a configuration that should fail validation but not throw during creation
        var config = new MediatorConfiguration();
        // No assemblies added and no middleware discovery - this will cause validation error

        // Act
        var diagnostics = config.GetDiagnostics();

        // Assert
        Assert.False(diagnostics.IsValid);
        Assert.NotEmpty(diagnostics.ValidationErrors);
        Assert.Contains(diagnostics.ValidationErrors, e => e.Contains("No assemblies configured"));
    }

    [Fact]
    public void MixedConfiguration_JsonAndPreset_OverridesCorrectly()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var configurationData = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Statistics:EnableRequestMetrics"] = "false", // Override preset
            ["Blazing:Mediator:Statistics:MetricsRetentionPeriod"] = "06:00:00", // Override preset
            ["Blazing:Mediator:Telemetry:Enabled"] = "false", // Override preset
            ["Blazing:Mediator:Discovery:DiscoverMiddleware"] = "false" // Override preset
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act - Apply preset first, then configuration
        var result = config.WithDevelopmentPreset()
                           .WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        
        // JSON overrides should take precedence
        Assert.NotNull(config.StatisticsOptions);
        Assert.False(config.StatisticsOptions.EnableRequestMetrics); // Overridden
        Assert.Equal(TimeSpan.FromHours(6), config.StatisticsOptions.MetricsRetentionPeriod); // Overridden
        Assert.NotNull(config.TelemetryOptions);
        Assert.False(config.TelemetryOptions.Enabled); // Overridden
        Assert.False(config.DiscoverMiddleware); // Overridden
        
        // Non-overridden values should remain from preset
        Assert.True(config.StatisticsOptions.EnableNotificationMetrics); // From development preset
        Assert.True(config.DiscoverNotificationMiddleware); // From development preset
    }

    [Fact]
    public void EnvironmentSpecificConfigurationFiles_LoadCorrectly()
    {
        // Arrange
        var baseConfig = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Statistics:EnableRequestMetrics"] = "true",
            ["Blazing:Mediator:Discovery:DiscoverMiddleware"] = "true"
        };

        var developmentOverrides = new Dictionary<string, string>
        {
            ["Blazing:Mediator:Statistics:EnableDetailedAnalysis"] = "true",
            ["Blazing:Mediator:Logging:EnableDetailedHandlerInfo"] = "true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(baseConfig!)
            .AddInMemoryCollection(developmentOverrides!) // Simulates appsettings.Development.json
            .Build();

        var config = new MediatorConfiguration();

        // Act
        var result = config.WithConfiguration(configuration);

        // Assert
        Assert.Same(config, result);
        
        // Base configuration should be applied
        Assert.NotNull(config.StatisticsOptions);
        Assert.True(config.StatisticsOptions.EnableRequestMetrics);
        Assert.True(config.DiscoverMiddleware);
        
        // Development overrides should be applied
        Assert.True(config.StatisticsOptions.EnableDetailedAnalysis);
        Assert.NotNull(config.LoggingOptions);
        Assert.True(config.LoggingOptions.EnableDetailedHandlerInfo);
    }

    [Fact]
    public void CustomSectionPath_WorksWithEnvironmentConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var environment = new HostingEnvironment { EnvironmentName = "Development" };
        var configurationData = new Dictionary<string, string>
        {
            ["MyApp:MediatorSettings:Statistics:EnableRequestMetrics"] = "false",
            ["MyApp:MediatorSettings:Discovery:DiscoverMiddleware"] = "false"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData!)
            .Build();

        // Act
        var result = config.WithEnvironmentConfiguration(configuration, environment, "MyApp:MediatorSettings");

        // Assert
        Assert.Same(config, result);
        
        // Should have development preset applied first
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.LoggingOptions);
        
        // Then custom section overrides should be applied
        Assert.False(config.StatisticsOptions.EnableRequestMetrics); // Overridden by custom section
        Assert.False(config.DiscoverMiddleware); // Overridden by custom section
    }
}