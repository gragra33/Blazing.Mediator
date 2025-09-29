using Blazing.Mediator.Configuration;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Tests for IConfigurationOptions and IEnvironmentConfigurationOptions interface implementations.
/// Validates that all configuration classes follow the same patterns and contracts.
/// </summary>
public class ConfigurationOptionsInterfaceTests
{
    #region StatisticsOptions Interface Compliance Tests

    [Fact]
    public void StatisticsOptions_ShouldImplementIEnvironmentConfigurationOptions()
    {
        // Act & Assert
        typeof(StatisticsOptions).GetInterfaces().ShouldContain(typeof(IEnvironmentConfigurationOptions<StatisticsOptions>));
        typeof(StatisticsOptions).GetInterfaces().ShouldContain(typeof(IConfigurationOptions<StatisticsOptions>));
    }

    [Fact]
    public void StatisticsOptions_Validate_ShouldReturnEmptyListForValidConfiguration()
    {
        // Arrange
        var options = new StatisticsOptions();

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void StatisticsOptions_ValidateAndThrow_ShouldNotThrowForValidConfiguration()
    {
        // Arrange
        var options = new StatisticsOptions();

        // Act & Assert
        Should.NotThrow(() => options.ValidateAndThrow());
    }

    [Fact]
    public void StatisticsOptions_Clone_ShouldCreateExactCopy()
    {
        // Arrange
        var original = new StatisticsOptions
        {
            EnableRequestMetrics = false,
            EnableNotificationMetrics = false,
            EnableMiddlewareMetrics = true,
            EnablePerformanceCounters = true,
            EnableDetailedAnalysis = true,
            MaxTrackedRequestTypes = 500,
            MetricsRetentionPeriod = TimeSpan.FromMinutes(30),
            CleanupInterval = TimeSpan.FromMinutes(10)
        };

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBeSameAs(original);
        clone.EnableRequestMetrics.ShouldBe(original.EnableRequestMetrics);
        clone.EnableNotificationMetrics.ShouldBe(original.EnableNotificationMetrics);
        clone.EnableMiddlewareMetrics.ShouldBe(original.EnableMiddlewareMetrics);
        clone.EnablePerformanceCounters.ShouldBe(original.EnablePerformanceCounters);
        clone.EnableDetailedAnalysis.ShouldBe(original.EnableDetailedAnalysis);
        clone.MaxTrackedRequestTypes.ShouldBe(original.MaxTrackedRequestTypes);
        clone.MetricsRetentionPeriod.ShouldBe(original.MetricsRetentionPeriod);
        clone.CleanupInterval.ShouldBe(original.CleanupInterval);
    }

    [Fact]
    public void StatisticsOptions_Development_ShouldReturnDevelopmentConfiguration()
    {
        // Act
        var options = StatisticsOptions.Development();

        // Assert
        options.ShouldNotBeNull();
        options.EnableRequestMetrics.ShouldBeTrue();
        options.EnableNotificationMetrics.ShouldBeTrue();
        options.EnableMiddlewareMetrics.ShouldBeTrue();
        options.EnableDetailedAnalysis.ShouldBeTrue();
        options.MetricsRetentionPeriod.ShouldBe(TimeSpan.FromHours(1));
        options.CleanupInterval.ShouldBe(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void StatisticsOptions_Production_ShouldReturnProductionConfiguration()
    {
        // Act
        var options = StatisticsOptions.Production();

        // Assert
        options.ShouldNotBeNull();
        options.EnableRequestMetrics.ShouldBeTrue();
        options.EnableNotificationMetrics.ShouldBeTrue();
        options.EnableMiddlewareMetrics.ShouldBeFalse();
        options.EnableDetailedAnalysis.ShouldBeFalse();
        options.MetricsRetentionPeriod.ShouldBe(TimeSpan.FromHours(24));
        options.CleanupInterval.ShouldBe(TimeSpan.FromHours(4));
    }

    [Fact]
    public void StatisticsOptions_Disabled_ShouldReturnDisabledConfiguration()
    {
        // Act
        var options = StatisticsOptions.Disabled();

        // Assert
        options.ShouldNotBeNull();
        options.EnableRequestMetrics.ShouldBeFalse();
        options.EnableNotificationMetrics.ShouldBeFalse();
        options.EnableMiddlewareMetrics.ShouldBeFalse();
        options.EnablePerformanceCounters.ShouldBeFalse();
        options.EnableDetailedAnalysis.ShouldBeFalse();
        options.IsEnabled.ShouldBeFalse();
    }

    #endregion

    #region TelemetryOptions Interface Compliance Tests

    [Fact]
    public void TelemetryOptions_ShouldImplementIEnvironmentConfigurationOptions()
    {
        // Act & Assert
        typeof(TelemetryOptions).GetInterfaces().ShouldContain(typeof(IEnvironmentConfigurationOptions<TelemetryOptions>));
        typeof(TelemetryOptions).GetInterfaces().ShouldContain(typeof(IConfigurationOptions<TelemetryOptions>));
    }

    [Fact]
    public void TelemetryOptions_Validate_ShouldReturnEmptyListForValidConfiguration()
    {
        // Arrange
        var options = new TelemetryOptions();

        // Act
        var errors = options.Validate();

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void TelemetryOptions_ValidateAndThrow_ShouldNotThrowForValidConfiguration()
    {
        // Arrange
        var options = new TelemetryOptions();

        // Act & Assert
        Should.NotThrow(() => options.ValidateAndThrow());
    }

    [Fact]
    public void TelemetryOptions_Clone_ShouldCreateExactCopy()
    {
        // Arrange
        var original = new TelemetryOptions
        {
            Enabled = false,
            CaptureMiddlewareDetails = false,
            CaptureHandlerDetails = true,
            MaxExceptionMessageLength = 150,
            MaxStackTraceLines = 5,
            PacketLevelTelemetryEnabled = true,
            PacketTelemetryBatchSize = 25,
            CaptureNotificationHandlerDetails = false,
            CreateHandlerChildSpans = false,
            SensitiveDataPatterns = ["test1", "test2"]
        };

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBeSameAs(original);
        clone.Enabled.ShouldBe(original.Enabled);
        clone.CaptureMiddlewareDetails.ShouldBe(original.CaptureMiddlewareDetails);
        clone.CaptureHandlerDetails.ShouldBe(original.CaptureHandlerDetails);
        clone.MaxExceptionMessageLength.ShouldBe(original.MaxExceptionMessageLength);
        clone.MaxStackTraceLines.ShouldBe(original.MaxStackTraceLines);
        clone.PacketLevelTelemetryEnabled.ShouldBe(original.PacketLevelTelemetryEnabled);
        clone.PacketTelemetryBatchSize.ShouldBe(original.PacketTelemetryBatchSize);
        clone.CaptureNotificationHandlerDetails.ShouldBe(original.CaptureNotificationHandlerDetails);
        clone.CreateHandlerChildSpans.ShouldBe(original.CreateHandlerChildSpans);
        clone.SensitiveDataPatterns.ShouldNotBeSameAs(original.SensitiveDataPatterns);
        clone.SensitiveDataPatterns.ShouldBe(original.SensitiveDataPatterns);
    }

    [Fact]
    public void TelemetryOptions_Development_ShouldReturnDevelopmentConfiguration()
    {
        // Act
        var options = TelemetryOptions.Development();

        // Assert
        options.ShouldNotBeNull();
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeTrue();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeTrue();
        options.CaptureSubscriberMetrics.ShouldBeTrue();
        options.CaptureNotificationMiddlewareDetails.ShouldBeTrue();
        options.MaxExceptionMessageLength.ShouldBe(500);
        options.MaxStackTraceLines.ShouldBe(10);
        options.PacketLevelTelemetryEnabled.ShouldBeTrue();
    }

    [Fact]
    public void TelemetryOptions_Production_ShouldReturnProductionConfiguration()
    {
        // Act
        var options = TelemetryOptions.Production();

        // Assert
        options.ShouldNotBeNull();
        options.Enabled.ShouldBeTrue();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeTrue();
        options.CaptureNotificationHandlerDetails.ShouldBeTrue();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
        options.MaxExceptionMessageLength.ShouldBe(200);
        options.MaxStackTraceLines.ShouldBe(3);
        options.PacketLevelTelemetryEnabled.ShouldBeFalse();
    }

    [Fact]
    public void TelemetryOptions_Disabled_ShouldReturnDisabledConfiguration()
    {
        // Act
        var options = TelemetryOptions.Disabled();

        // Assert
        options.ShouldNotBeNull();
        options.Enabled.ShouldBeFalse();
        options.CaptureMiddlewareDetails.ShouldBeFalse();
        options.CaptureHandlerDetails.ShouldBeFalse();
        options.CaptureNotificationHandlerDetails.ShouldBeFalse();
        options.CreateHandlerChildSpans.ShouldBeFalse();
        options.CaptureSubscriberMetrics.ShouldBeFalse();
        options.CaptureNotificationMiddlewareDetails.ShouldBeFalse();
        options.IsEnabled.ShouldBeFalse();
    }

    #endregion

    #region MediatorConfiguration Factory Method Tests

    [Fact]
    public void MediatorConfiguration_Development_ShouldReturnDevelopmentConfiguration()
    {
        // Arrange
        var assembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;

        // Act
        var config = MediatorConfiguration.Development(assembly);

        // Assert
        config.ShouldNotBeNull();
        config.StatisticsOptions.ShouldNotBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldNotBeNull();
        config.DiscoverMiddleware.ShouldBeTrue();
        config.DiscoverNotificationMiddleware.ShouldBeTrue();
        config.DiscoverConstrainedMiddleware.ShouldBeTrue();
        config.DiscoverNotificationHandlers.ShouldBeTrue();
        config.Assemblies.ShouldContain(assembly);
    }

    [Fact]
    public void MediatorConfiguration_Production_ShouldReturnProductionConfiguration()
    {
        // Arrange
        var assembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;

        // Act
        var config = MediatorConfiguration.Production(assembly);

        // Assert
        config.ShouldNotBeNull();
        config.StatisticsOptions.ShouldNotBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldNotBeNull();
        config.DiscoverMiddleware.ShouldBeTrue();
        config.DiscoverNotificationMiddleware.ShouldBeTrue();
        config.DiscoverConstrainedMiddleware.ShouldBeTrue();
        config.DiscoverNotificationHandlers.ShouldBeTrue();
        config.Assemblies.ShouldContain(assembly);
        
        // Production should have optimized settings
        config.StatisticsOptions.EnableMiddlewareMetrics.ShouldBeFalse();
        config.TelemetryOptions.CaptureMiddlewareDetails.ShouldBeFalse();
    }

    [Fact]
    public void MediatorConfiguration_Minimal_ShouldReturnMinimalConfiguration()
    {
        // Arrange
        var assembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;

        // Act
        var config = MediatorConfiguration.Minimal(assembly);

        // Assert
        config.ShouldNotBeNull();
        config.StatisticsOptions.ShouldNotBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldBeNull();
        config.DiscoverConstrainedMiddleware.ShouldBeFalse();
        config.DiscoverNotificationHandlers.ShouldBeTrue();
        config.Assemblies.ShouldContain(assembly);
        
        // Minimal should have disabled statistics and minimal telemetry
        config.StatisticsOptions.IsEnabled.ShouldBeFalse();
        config.TelemetryOptions.CaptureMiddlewareDetails.ShouldBeFalse();
        config.TelemetryOptions.CaptureHandlerDetails.ShouldBeFalse();
    }

    [Fact]
    public void MediatorConfiguration_Disabled_ShouldReturnDisabledConfiguration()
    {
        // Arrange
        var assembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;

        // Act
        var config = MediatorConfiguration.Disabled(assembly);

        // Assert
        config.ShouldNotBeNull();
        config.StatisticsOptions.ShouldBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldBeNull();
        config.DiscoverConstrainedMiddleware.ShouldBeFalse();
        config.DiscoverNotificationHandlers.ShouldBeFalse();
        config.Assemblies.ShouldContain(assembly);
        
        // Disabled should have everything turned off
        config.TelemetryOptions.IsEnabled.ShouldBeFalse();
    }

    [Fact]
    public void MediatorConfiguration_NotificationOptimized_ShouldReturnNotificationOptimizedConfiguration()
    {
        // Arrange
        var assembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;

        // Act
        var config = MediatorConfiguration.NotificationOptimized(assembly);

        // Assert
        config.ShouldNotBeNull();
        config.StatisticsOptions.ShouldNotBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldNotBeNull();
        config.DiscoverNotificationMiddleware.ShouldBeTrue();
        config.DiscoverConstrainedMiddleware.ShouldBeTrue();
        config.DiscoverNotificationHandlers.ShouldBeTrue();
        config.Assemblies.ShouldContain(assembly);
        
        // Should be optimized for notifications
        config.StatisticsOptions.EnableNotificationMetrics.ShouldBeTrue();
        config.StatisticsOptions.EnableRequestMetrics.ShouldBeFalse();
        config.TelemetryOptions.CaptureNotificationHandlerDetails.ShouldBeTrue();
        config.TelemetryOptions.CreateHandlerChildSpans.ShouldBeTrue();
    }

    [Fact]
    public void MediatorConfiguration_StreamingOptimized_ShouldReturnStreamingOptimizedConfiguration()
    {
        // Arrange
        var assembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;

        // Act
        var config = MediatorConfiguration.StreamingOptimized(assembly);

        // Assert
        config.ShouldNotBeNull();
        config.StatisticsOptions.ShouldNotBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldNotBeNull();
        config.DiscoverMiddleware.ShouldBeTrue();
        config.DiscoverNotificationMiddleware.ShouldBeFalse();
        config.DiscoverConstrainedMiddleware.ShouldBeFalse();
        config.DiscoverNotificationHandlers.ShouldBeFalse();
        config.Assemblies.ShouldContain(assembly);
        
        // Should be optimized for streaming
        config.StatisticsOptions.EnableRequestMetrics.ShouldBeTrue();
        config.StatisticsOptions.EnableNotificationMetrics.ShouldBeFalse();
        config.StatisticsOptions.EnablePerformanceCounters.ShouldBeTrue();
        config.TelemetryOptions.PacketLevelTelemetryEnabled.ShouldBeTrue();
        config.TelemetryOptions.EnableStreamingMetrics.ShouldBeTrue();
    }

    #endregion

    #region MediatorConfiguration Validation Tests

    [Fact]
    public void MediatorConfiguration_Validate_ShouldReturnEmptyListForValidConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .AddAssembly(typeof(ConfigurationOptionsInterfaceTests));

        // Act
        var errors = config.Validate();

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void MediatorConfiguration_Validate_ShouldReturnErrorsForInvalidSubConfigurations()
    {
        // Arrange
        var config = new MediatorConfiguration();
        // Directly set invalid statistics options instead of using WithStatisticsTracking which calls ValidateAndThrow
        config.StatisticsOptions = new StatisticsOptions
        {
            MetricsRetentionPeriod = TimeSpan.FromMinutes(-1) // Invalid
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.ShouldNotBeEmpty();
        errors.ShouldContain(e => e.Contains("Statistics"));
        errors.ShouldContain(e => e.Contains("MetricsRetentionPeriod cannot be negative"));
    }

    [Fact]
    public void MediatorConfiguration_ValidateAndThrow_ShouldThrowForInvalidConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration()
            .WithTelemetry(options =>
            {
                options.MaxExceptionMessageLength = -1; // Invalid
            });

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => config.ValidateAndThrow());
        exception.Message.ShouldContain("Invalid MediatorConfiguration");
        exception.Message.ShouldContain("Telemetry");
        exception.Message.ShouldContain("MaxExceptionMessageLength cannot be negative");
    }

    [Fact]
    public void MediatorConfiguration_Clone_ShouldCreateExactCopy()
    {
        // Arrange
        var originalAssembly = typeof(ConfigurationOptionsInterfaceTests).Assembly;
        var original = new MediatorConfiguration()
            .AddAssembly(originalAssembly)
            .WithStatisticsTracking(StatisticsOptions.Development())
            .WithTelemetry(TelemetryOptions.Production())
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery();

        // Act
        var clone = original.Clone();

        // Assert
        clone.ShouldNotBeSameAs(original);
        clone.Assemblies.ShouldBe(original.Assemblies);
        clone.DiscoverMiddleware.ShouldBe(original.DiscoverMiddleware);
        clone.DiscoverNotificationMiddleware.ShouldBe(original.DiscoverNotificationMiddleware);
        clone.DiscoverConstrainedMiddleware.ShouldBe(original.DiscoverConstrainedMiddleware);
        clone.DiscoverNotificationHandlers.ShouldBe(original.DiscoverNotificationHandlers);
        
        clone.StatisticsOptions.ShouldNotBeSameAs(original.StatisticsOptions);
        clone.StatisticsOptions.ShouldNotBeNull();
        clone.StatisticsOptions.EnableDetailedAnalysis.ShouldBe(original.StatisticsOptions!.EnableDetailedAnalysis);
        
        clone.TelemetryOptions.ShouldNotBeSameAs(original.TelemetryOptions);
        clone.TelemetryOptions.ShouldNotBeNull();
        clone.TelemetryOptions.CaptureMiddlewareDetails.ShouldBe(original.TelemetryOptions!.CaptureMiddlewareDetails);
    }

    #endregion
}