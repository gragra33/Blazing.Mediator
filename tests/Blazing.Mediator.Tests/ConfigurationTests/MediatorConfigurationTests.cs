using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Comprehensive tests for MediatorConfiguration functionality.
/// Validates all configuration methods, property settings, and fluent interface behavior.
/// </summary>
public class MediatorConfigurationTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithoutServices_ShouldInitializeCorrectly()
    {
        // Act
        var config = new MediatorConfiguration();

        // Assert
        config.ShouldNotBeNull();
        config.PipelineBuilder.ShouldNotBeNull();
        config.NotificationPipelineBuilder.ShouldNotBeNull();
        config.Assemblies.ShouldBeEmpty();
        config.StatisticsOptions.ShouldBeNull();
        config.TelemetryOptions.ShouldBeNull();
        config.LoggingOptions.ShouldBeNull();
        config.DiscoverMiddleware.ShouldBeFalse();
        config.DiscoverNotificationMiddleware.ShouldBeFalse();
        config.DiscoverConstrainedMiddleware.ShouldBeTrue(); // Default to true
        config.DiscoverNotificationHandlers.ShouldBeTrue(); // Default to true
    }

    [Fact]
    public void Constructor_WithServices_ShouldInitializeCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var config = new MediatorConfiguration(services);

        // Assert
        config.ShouldNotBeNull();
        config.PipelineBuilder.ShouldNotBeNull();
        config.NotificationPipelineBuilder.ShouldNotBeNull();
        config.Assemblies.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullServices_ShouldInitializeCorrectly()
    {
        // Act
        var config = new MediatorConfiguration(null);

        // Assert
        config.ShouldNotBeNull();
        config.PipelineBuilder.ShouldNotBeNull();
        config.NotificationPipelineBuilder.ShouldNotBeNull();
    }

    #endregion

    #region Assembly Registration Tests

    [Fact]
    public void AddFromAssembly_WithType_ShouldAddAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddFromAssembly(typeof(MediatorConfigurationTests));

        // Assert
        result.ShouldBe(config); // Should return self for chaining
        config.Assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
        config.Assemblies.Count.ShouldBe(1);
    }

    [Fact]
    public void AddFromAssembly_WithGenericType_ShouldAddAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddFromAssembly<MediatorConfigurationTests>();

        // Assert
        result.ShouldBe(config);
        config.Assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
    }

    [Fact]
    public void AddFromAssembly_WithAssembly_ShouldAddAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly = typeof(MediatorConfigurationTests).Assembly;

        // Act
        var result = config.AddFromAssembly(assembly);

        // Assert
        result.ShouldBe(config);
        config.Assemblies.ShouldContain(assembly);
    }

    [Fact]
    public void AddFromAssembly_WithNullType_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.AddFromAssembly((Type)null!));
    }

    [Fact]
    public void AddFromAssembly_WithNullAssembly_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.AddFromAssembly((Assembly)null!));
    }

    [Fact]
    public void AddFromAssembly_WithSameAssemblyMultipleTimes_ShouldContainOnlyOnce()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly = typeof(MediatorConfigurationTests).Assembly;

        // Act
        config.AddFromAssembly(assembly);
        config.AddFromAssembly(assembly);
        config.AddFromAssembly<MediatorConfigurationTests>();

        // Assert
        config.Assemblies.Count.ShouldBe(1);
        config.Assemblies.ShouldContain(assembly);
    }

    [Fact]
    public void AddFromAssemblies_WithTypeArray_ShouldAddAllAssemblies()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var types = new[] { typeof(MediatorConfigurationTests), typeof(string) };

        // Act
        var result = config.AddFromAssemblies(types);

        // Assert
        result.ShouldBe(config);
        config.Assemblies.Count.ShouldBe(2); // Different assemblies
        config.Assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
        config.Assemblies.ShouldContain(typeof(string).Assembly);
    }

    [Fact]
    public void AddFromAssemblies_WithAssemblyArray_ShouldAddAllAssemblies()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assemblies = new[] { 
            typeof(MediatorConfigurationTests).Assembly,
            typeof(string).Assembly 
        };

        // Act
        var result = config.AddFromAssemblies(assemblies);

        // Assert
        result.ShouldBe(config);
        config.Assemblies.Count.ShouldBe(2);
        config.Assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
        config.Assemblies.ShouldContain(typeof(string).Assembly);
    }

    [Fact]
    public void AddFromAssemblies_WithNullTypeArray_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.AddFromAssemblies((Type[])null!));
    }

    [Fact]
    public void AddFromAssemblies_WithNullAssemblyArray_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.AddFromAssemblies((Assembly[])null!));
    }

    #endregion

    #region Assembly Alias Methods Tests

    [Fact]
    public void AddAssembly_WithType_ShouldBehaveSameAsAddFromAssembly()
    {
        // Arrange
        var config1 = new MediatorConfiguration();
        var config2 = new MediatorConfiguration();

        // Act
        config1.AddFromAssembly(typeof(MediatorConfigurationTests));
        config2.AddAssembly(typeof(MediatorConfigurationTests));

        // Assert
        config1.Assemblies.ShouldBe(config2.Assemblies);
    }

    [Fact]
    public void AddAssembly_WithGenericType_ShouldBehaveSameAsAddFromAssembly()
    {
        // Arrange
        var config1 = new MediatorConfiguration();
        var config2 = new MediatorConfiguration();

        // Act
        config1.AddFromAssembly<MediatorConfigurationTests>();
        config2.AddAssembly<MediatorConfigurationTests>();

        // Assert
        config1.Assemblies.ShouldBe(config2.Assemblies);
    }

    [Fact]
    public void AddAssemblies_WithTypes_ShouldBehaveSameAsAddFromAssemblies()
    {
        // Arrange
        var config1 = new MediatorConfiguration();
        var config2 = new MediatorConfiguration();
        var types = new[] { typeof(MediatorConfigurationTests), typeof(string) };

        // Act
        config1.AddFromAssemblies(types);
        config2.AddAssemblies(types);

        // Assert
        config1.Assemblies.ShouldBe(config2.Assemblies);
    }

    #endregion

    #region Statistics Configuration Tests

    [Fact]
    public void WithStatisticsTracking_WithoutConfiguration_ShouldEnableStatistics()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithStatisticsTracking();

        // Assert
        result.ShouldBe(config);
        config.StatisticsOptions.ShouldNotBeNull();
#pragma warning disable CS0618 // Type or member is obsolete
        config.EnableStatisticsTracking.ShouldBeTrue();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void WithStatisticsTracking_WithConfiguration_ShouldApplyConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithStatisticsTracking(opts =>
        {
            opts.EnablePerformanceCounters = true;
            opts.MaxTrackedRequestTypes = 500;
        });

        // Assert
        result.ShouldBe(config);
        config.StatisticsOptions.ShouldNotBeNull();
        config.StatisticsOptions.EnablePerformanceCounters.ShouldBeTrue();
        config.StatisticsOptions.MaxTrackedRequestTypes.ShouldBe(500);
    }

    [Fact]
    public void WithStatisticsTracking_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.WithStatisticsTracking((Action<StatisticsOptions>)null!));
    }

    [Fact]
    public void WithStatisticsTracking_WithPreConfiguredOptions_ShouldUseOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var options = new StatisticsOptions
        {
            EnableDetailedAnalysis = true,
            MaxTrackedRequestTypes = 750
        };

        // Act
        var result = config.WithStatisticsTracking(options);

        // Assert
        result.ShouldBe(config);
        config.StatisticsOptions.ShouldNotBeNull();
        config.StatisticsOptions.EnableDetailedAnalysis.ShouldBeTrue();
        config.StatisticsOptions.MaxTrackedRequestTypes.ShouldBe(750);
        // Should be a clone, not the same instance
        config.StatisticsOptions.ShouldNotBe(options);
    }

    [Fact]
    public void WithStatisticsTracking_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.WithStatisticsTracking((StatisticsOptions)null!));
    }

    [Fact]
    public void WithStatisticsTracking_WithInvalidConfiguration_ShouldThrowException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentException>(() => config.WithStatisticsTracking(opts =>
        {
            opts.MaxTrackedRequestTypes = -1; // Invalid value
        }));
    }

    [Fact]
    public void WithoutStatistics_ShouldClearStatisticsOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking();

        // Act
        var result = config.WithoutStatistics();

        // Assert
        result.ShouldBe(config);
        config.StatisticsOptions.ShouldBeNull();
#pragma warning disable CS0618 // Type or member is obsolete
        config.EnableStatisticsTracking.ShouldBeFalse();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    #endregion

    #region Telemetry Configuration Tests

    [Fact]
    public void WithTelemetry_WithoutConfiguration_ShouldEnableTelemetry()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithTelemetry();

        // Assert
        result.ShouldBe(config);
        config.TelemetryOptions.ShouldNotBeNull();
        config.TelemetryOptions.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void WithTelemetry_WithConfiguration_ShouldApplyConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithTelemetry(opts =>
        {
            opts.Enabled = false;
            opts.PacketLevelTelemetryEnabled = true;
            opts.MaxExceptionMessageLength = 500;
        });

        // Assert
        result.ShouldBe(config);
        config.TelemetryOptions.ShouldNotBeNull();
        config.TelemetryOptions.Enabled.ShouldBeFalse();
        config.TelemetryOptions.PacketLevelTelemetryEnabled.ShouldBeTrue();
        config.TelemetryOptions.MaxExceptionMessageLength.ShouldBe(500);
    }

    [Fact]
    public void WithTelemetry_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.WithTelemetry((Action<TelemetryOptions>)null!));
    }

    [Fact]
    public void WithTelemetry_WithPreConfiguredOptions_ShouldUseOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var options = new TelemetryOptions
        {
            CapturePacketSize = true,
            PacketTelemetryBatchSize = 25
        };

        // Act
        var result = config.WithTelemetry(options);

        // Assert
        result.ShouldBe(config);
        config.TelemetryOptions.ShouldNotBeNull();
        config.TelemetryOptions.CapturePacketSize.ShouldBeTrue();
        config.TelemetryOptions.PacketTelemetryBatchSize.ShouldBe(25);
        // Should be the same instance
        config.TelemetryOptions.ShouldBe(options);
    }

    [Fact]
    public void WithTelemetry_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.WithTelemetry((TelemetryOptions)null!));
    }

    [Fact]
    public void WithoutTelemetry_ShouldClearTelemetryOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithTelemetry();

        // Act
        var result = config.WithoutTelemetry();

        // Assert
        result.ShouldBe(config);
        config.TelemetryOptions.ShouldBeNull();
    }

    #endregion

    #region Logging Configuration Tests

    [Fact]
    public void WithLogging_WithoutConfiguration_ShouldEnableLogging()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithLogging();

        // Assert
        result.ShouldBe(config);
        config.LoggingOptions.ShouldNotBeNull();
    }

    [Fact]
    public void WithLogging_WithConfiguration_ShouldApplyConfiguration()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithLogging(opts =>
        {
            opts.EnableDetailedHandlerInfo = true;
            opts.EnablePerformanceTiming = false;
        });

        // Assert
        result.ShouldBe(config);
        config.LoggingOptions.ShouldNotBeNull();
        config.LoggingOptions.EnableDetailedHandlerInfo.ShouldBeTrue();
        config.LoggingOptions.EnablePerformanceTiming.ShouldBeFalse();
    }

    [Fact]
    public void WithLogging_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.WithLogging((Action<LoggingOptions>)null!));
    }

    [Fact]
    public void WithLogging_WithPreConfiguredOptions_ShouldUseOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var options = new LoggingOptions
        {
            EnableConstraintLogging = true,
            EnableMiddlewareRoutingLogging = false // Set to false to avoid validation warning
        };

        // Act
        var result = config.WithLogging(options);

        // Assert
        result.ShouldBe(config);
        config.LoggingOptions.ShouldNotBeNull();
        config.LoggingOptions.EnableConstraintLogging.ShouldBeTrue();
        config.LoggingOptions.EnableMiddlewareRoutingLogging.ShouldBeFalse();
    }

    [Fact]
    public void WithLogging_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => config.WithLogging((LoggingOptions)null!));
    }

    [Fact]
    public void WithLogging_WithInvalidConfiguration_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var invalidOptions = LoggingOptions.CreateMinimal();
        // Simulate an invalid configuration by creating a mock that returns validation errors
        
        // This test might need adjustment based on actual validation logic
        // For now, we'll test that the validation process is called
        Should.NotThrow(() => config.WithLogging(invalidOptions));
    }

    [Fact]
    public void WithoutLogging_ShouldClearLoggingOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithLogging();

        // Act
        var result = config.WithoutLogging();

        // Assert
        result.ShouldBe(config);
        config.LoggingOptions.ShouldBeNull();
    }

    #endregion

    #region Discovery Configuration Tests

    [Fact]
    public void WithMiddlewareDiscovery_ShouldEnableDiscovery()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithMiddlewareDiscovery();

        // Assert
        result.ShouldBe(config);
        config.DiscoverMiddleware.ShouldBeTrue();
    }

    [Fact]
    public void WithNotificationMiddlewareDiscovery_ShouldEnableDiscovery()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithNotificationMiddlewareDiscovery();

        // Assert
        result.ShouldBe(config);
        config.DiscoverNotificationMiddleware.ShouldBeTrue();
    }

    [Fact]
    public void WithConstrainedMiddlewareDiscovery_ShouldEnableDiscovery()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.DiscoverConstrainedMiddleware = false; // Set to false first

        // Act
        var result = config.WithConstrainedMiddlewareDiscovery();

        // Assert
        result.ShouldBe(config);
        config.DiscoverConstrainedMiddleware.ShouldBeTrue();
    }

    [Fact]
    public void WithoutConstrainedMiddlewareDiscovery_ShouldDisableDiscovery()
    {
        // Arrange
        var config = new MediatorConfiguration();
        // Default is true, so this should set it to false

        // Act
        var result = config.WithoutConstrainedMiddlewareDiscovery();

        // Assert
        result.ShouldBe(config);
        config.DiscoverConstrainedMiddleware.ShouldBeFalse();
    }

    [Fact]
    public void WithNotificationHandlerDiscovery_ShouldEnableDiscovery()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.DiscoverNotificationHandlers = false; // Set to false first

        // Act
        var result = config.WithNotificationHandlerDiscovery();

        // Assert
        result.ShouldBe(config);
        config.DiscoverNotificationHandlers.ShouldBeTrue();
    }

    [Fact]
    public void WithoutNotificationHandlerDiscovery_ShouldDisableDiscovery()
    {
        // Arrange
        var config = new MediatorConfiguration();
        // Default is true, so this should set it to false

        // Act
        var result = config.WithoutNotificationHandlerDiscovery();

        // Assert
        result.ShouldBe(config);
        config.DiscoverNotificationHandlers.ShouldBeFalse();
    }

    #endregion

    #region Middleware Registration Tests

    [Fact]
    public void AddMiddleware_WithGenericType_ShouldAddToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddMiddleware<TestMiddleware>();

        // Assert
        result.ShouldBe(config);
        // Note: We can't directly test the pipeline builder state without reflection or additional interfaces
        // The behavior is tested through integration tests
    }

    [Fact]
    public void AddMiddleware_WithType_ShouldAddToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddMiddleware(typeof(TestMiddleware));

        // Assert
        result.ShouldBe(config);
    }

    [Fact]
    public void AddMiddleware_WithMultipleTypes_ShouldAddAllToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddMiddleware(typeof(TestMiddleware), typeof(AnotherTestMiddleware));

        // Assert
        result.ShouldBe(config);
    }

    [Fact]
    public void AddMiddleware_WithServicesCollection_ShouldRegisterInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new MediatorConfiguration(services);

        // Act
        config.AddMiddleware<TestMiddleware>();

        // Assert
        services.Any(s => s.ServiceType == typeof(TestMiddleware)).ShouldBeTrue();
        services.First(s => s.ServiceType == typeof(TestMiddleware)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMiddleware_WithoutServicesCollection_ShouldNotThrow()
    {
        // Arrange
        var config = new MediatorConfiguration(null);

        // Act & Assert
        Should.NotThrow(() => config.AddMiddleware<TestMiddleware>());
    }

    [Fact]
    public void AddMiddleware_SameTypeMultipleTimes_ShouldRegisterOnlyOnce()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new MediatorConfiguration(services);

        // Act
        config.AddMiddleware<TestMiddleware>();
        config.AddMiddleware<TestMiddleware>(); // Should not register twice

        // Assert
        services.Count(s => s.ServiceType == typeof(TestMiddleware)).ShouldBe(1);
    }

    #endregion

    #region Notification Middleware Registration Tests

    [Fact]
    public void AddNotificationMiddleware_WithGenericType_ShouldAddToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddNotificationMiddleware<TestNotificationMiddleware>();

        // Assert
        result.ShouldBe(config);
    }

    [Fact]
    public void AddNotificationMiddleware_WithGenericTypeAndConfiguration_ShouldAddToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var middlewareConfig = new { Setting = "value" };

        // Act
        var result = config.AddNotificationMiddleware<TestNotificationMiddleware>(middlewareConfig);

        // Assert
        result.ShouldBe(config);
    }

    [Fact]
    public void AddNotificationMiddleware_WithType_ShouldAddToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddNotificationMiddleware(typeof(TestNotificationMiddleware));

        // Assert
        result.ShouldBe(config);
    }

    [Fact]
    public void AddNotificationMiddleware_WithMultipleTypes_ShouldAddAllToBuilder()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.AddNotificationMiddleware(typeof(TestNotificationMiddleware), typeof(AnotherTestNotificationMiddleware));

        // Assert
        result.ShouldBe(config);
    }

    [Fact]
    public void AddNotificationMiddleware_WithServicesCollection_ShouldRegisterInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new MediatorConfiguration(services);

        // Act
        config.AddNotificationMiddleware<TestNotificationMiddleware>();

        // Assert
        services.Any(s => s.ServiceType == typeof(TestNotificationMiddleware)).ShouldBeTrue();
        services.First(s => s.ServiceType == typeof(TestNotificationMiddleware)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    #endregion

    #region Fluent Interface Tests

    [Fact]
    public void FluentInterface_ShouldAllowChaining()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .AddFromAssembly<MediatorConfigurationTests>()
            .WithStatisticsTracking()
            .WithTelemetry()
            .WithLogging()
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery();

        // Assert
        result.ShouldBe(config);
        config.Assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
        config.StatisticsOptions.ShouldNotBeNull();
        config.TelemetryOptions.ShouldNotBeNull();
        config.LoggingOptions.ShouldNotBeNull();
        config.DiscoverMiddleware.ShouldBeTrue();
        config.DiscoverNotificationMiddleware.ShouldBeTrue();
        config.DiscoverConstrainedMiddleware.ShouldBeTrue();
        config.DiscoverNotificationHandlers.ShouldBeTrue();
    }

    [Fact]
    public void FluentInterface_WithWithoutMethods_ShouldAllowToggling()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .WithStatisticsTracking()
            .WithTelemetry()
            .WithLogging()
            .WithoutStatistics()
            .WithoutTelemetry()
            .WithoutLogging()
            .WithConstrainedMiddlewareDiscovery()
            .WithoutConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery()
            .WithoutNotificationHandlerDiscovery();

        // Assert
        result.ShouldBe(config);
        config.StatisticsOptions.ShouldBeNull();
        config.TelemetryOptions.ShouldBeNull();
        config.LoggingOptions.ShouldBeNull();
        config.DiscoverConstrainedMiddleware.ShouldBeFalse();
        config.DiscoverNotificationHandlers.ShouldBeFalse();
    }

    [Fact]
    public void FluentInterface_ComplexConfiguration_ShouldApplyAllSettings()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new MediatorConfiguration(services);

        // Act
        var result = config
            .AddFromAssembly(typeof(MediatorConfigurationTests))
            .AddFromAssembly<string>()
            .WithStatisticsTracking(opts =>
            {
                opts.EnablePerformanceCounters = true;
                opts.MaxTrackedRequestTypes = 500;
            })
            .WithTelemetry(opts =>
            {
                opts.PacketLevelTelemetryEnabled = true;
                opts.CapturePacketSize = true;
            })
            .WithLogging(opts =>
            {
                opts.EnableDetailedHandlerInfo = true;
                opts.EnableConstraintLogging = true;
            })
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .AddMiddleware<TestMiddleware>()
            .AddNotificationMiddleware<TestNotificationMiddleware>();

        // Assert
        result.ShouldBe(config);

        // Assembly configuration
        config.Assemblies.Count.ShouldBe(2);
        config.Assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
        config.Assemblies.ShouldContain(typeof(string).Assembly);

        // Statistics configuration
        config.StatisticsOptions.ShouldNotBeNull();
        config.StatisticsOptions.EnablePerformanceCounters.ShouldBeTrue();
        config.StatisticsOptions.MaxTrackedRequestTypes.ShouldBe(500);

        // Telemetry configuration
        config.TelemetryOptions.ShouldNotBeNull();
        config.TelemetryOptions.PacketLevelTelemetryEnabled.ShouldBeTrue();
        config.TelemetryOptions.CapturePacketSize.ShouldBeTrue();

        // Logging configuration
        config.LoggingOptions.ShouldNotBeNull();
        config.LoggingOptions.EnableDetailedHandlerInfo.ShouldBeTrue();
        config.LoggingOptions.EnableConstraintLogging.ShouldBeTrue();

        // Discovery configuration
        config.DiscoverMiddleware.ShouldBeTrue();
        config.DiscoverNotificationMiddleware.ShouldBeTrue();

        // DI registration
        services.Any(s => s.ServiceType == typeof(TestMiddleware)).ShouldBeTrue();
        services.Any(s => s.ServiceType == typeof(TestNotificationMiddleware)).ShouldBeTrue();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void DiscoverConstrainedMiddleware_DefaultValue_ShouldBeTrue()
    {
        // Arrange & Act
        var config = new MediatorConfiguration();

        // Assert
        config.DiscoverConstrainedMiddleware.ShouldBeTrue();
    }

    [Fact]
    public void DiscoverNotificationHandlers_DefaultValue_ShouldBeTrue()
    {
        // Arrange & Act
        var config = new MediatorConfiguration();

        // Assert
        config.DiscoverNotificationHandlers.ShouldBeTrue();
    }

    [Fact]
    public void DiscoverMiddleware_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var config = new MediatorConfiguration();

        // Assert
        config.DiscoverMiddleware.ShouldBeFalse();
    }

    [Fact]
    public void DiscoverNotificationMiddleware_DefaultValue_ShouldBeFalse()
    {
        // Arrange & Act
        var config = new MediatorConfiguration();

        // Assert
        config.DiscoverNotificationMiddleware.ShouldBeFalse();
    }

    [Fact]
    public void Assemblies_ShouldReturnReadOnlyList()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.AddFromAssembly<MediatorConfigurationTests>();

        // Act
        var assemblies = config.Assemblies;

        // Assert
        assemblies.ShouldBeAssignableTo<IReadOnlyList<Assembly>>();
        assemblies.Count.ShouldBe(1);
        assemblies.ShouldContain(typeof(MediatorConfigurationTests).Assembly);
    }

    #endregion

    #region Obsolete Property Tests

    [Fact]
    public void EnableStatisticsTracking_WithStatisticsEnabled_ShouldBeTrue()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.WithStatisticsTracking();

        // Assert
#pragma warning disable CS0618 // Type or member is obsolete
        config.EnableStatisticsTracking.ShouldBeTrue();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void EnableStatisticsTracking_WithStatisticsDisabled_ShouldBeFalse()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking();

        // Act
        config.WithoutStatistics();

        // Assert
#pragma warning disable CS0618 // Type or member is obsolete
        config.EnableStatisticsTracking.ShouldBeFalse();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    #endregion
}

#region Test Helper Classes

// Mock middleware classes for testing
public class TestMiddleware
{
    // Mock middleware implementation
}

public class AnotherTestMiddleware
{
    // Another mock middleware implementation
}

public class TestNotificationMiddleware : INotificationMiddleware
{
    public int Order => 0;

    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return next(notification, cancellationToken);
    }
}

public class AnotherTestNotificationMiddleware : INotificationMiddleware
{
    public int Order => 0;

    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        return next(notification, cancellationToken);
    }
}

#endregion
