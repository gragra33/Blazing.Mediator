using Blazing.Mediator.Configuration;
using Xunit;
using System.Reflection;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Tests for MediatorConfiguration functionality including assembly registration.
/// </summary>
public class MediatorConfigurationTests
{
    [Fact]
    public void CanInstantiateMediatorConfiguration()
    {
        var config = new MediatorConfiguration();
        Assert.NotNull(config);
    }

    [Fact]
    public void AddFromAssembly_WithType_AddsAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddFromAssembly(typeof(string));

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddFromAssembly_WithGeneric_AddsAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddFromAssembly<string>();

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddFromAssembly_WithAssemblyInstance_AddsAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        config.AddFromAssembly(assembly);

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(assembly, config.Assemblies);
    }

    [Fact]
    public void AddFromAssembly_SameAssemblyTwice_OnlyAddsOnce()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddFromAssembly<string>();
        config.AddFromAssembly<string>();

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddFromAssemblies_WithMultipleTypes_AddsAllAssemblies()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddFromAssemblies(typeof(string), typeof(MediatorConfiguration));

        // Assert
        Assert.Equal(2, config.Assemblies.Count);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
        Assert.Contains(typeof(MediatorConfiguration).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddFromAssemblies_WithAssemblyInstances_AddsAllAssemblies()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly1 = typeof(string).Assembly;
        var assembly2 = typeof(MediatorConfiguration).Assembly;

        // Act
        config.AddFromAssemblies(assembly1, assembly2);

        // Assert
        Assert.Equal(2, config.Assemblies.Count);
        Assert.Contains(assembly1, config.Assemblies);
        Assert.Contains(assembly2, config.Assemblies);
    }

    [Fact]
    public void AddAssembly_WithType_AddsAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddAssembly(typeof(string));

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddAssembly_WithGeneric_AddsAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddAssembly<string>();

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddAssembly_WithAssemblyInstance_AddsAssembly()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        config.AddAssembly(assembly);

        // Assert
        Assert.Single(config.Assemblies);
        Assert.Contains(assembly, config.Assemblies);
    }

    [Fact]
    public void AddAssemblies_WithTypes_AddsAllAssemblies()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        config.AddAssemblies(typeof(string), typeof(MediatorConfiguration));

        // Assert
        Assert.Equal(2, config.Assemblies.Count);
        Assert.Contains(typeof(string).Assembly, config.Assemblies);
        Assert.Contains(typeof(MediatorConfiguration).Assembly, config.Assemblies);
    }

    [Fact]
    public void AddAssemblies_WithAssemblyInstances_AddsAllAssemblies()
    {
        // Arrange
        var config = new MediatorConfiguration();
        var assembly1 = typeof(string).Assembly;
        var assembly2 = typeof(MediatorConfiguration).Assembly;

        // Act
        config.AddAssemblies(assembly1, assembly2);

        // Assert
        Assert.Equal(2, config.Assemblies.Count);
        Assert.Contains(assembly1, config.Assemblies);
        Assert.Contains(assembly2, config.Assemblies);
    }

    [Fact]
    public void AddFromAssembly_WithNullType_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.AddFromAssembly((Type)null!));
    }

    [Fact]
    public void AddFromAssembly_WithNullAssembly_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.AddFromAssembly((Assembly)null!));
    }

    [Fact]
    public void AddFromAssemblies_WithNullTypes_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.AddFromAssemblies((Type[])null!));
    }

    [Fact]
    public void AddFromAssemblies_WithNullAssemblies_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => config.AddFromAssemblies((Assembly[])null!));
    }

    [Fact]
    public void Assemblies_Property_ReturnsReadOnlyList()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.AddFromAssembly<string>();

        // Act
        var assemblies = config.Assemblies;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<Assembly>>(assemblies);
        Assert.Single(assemblies);
    }

    [Fact]
    public void FluentConfiguration_CanChain()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .AddFromAssembly<string>()
            .AddFromAssembly<MediatorConfiguration>()
            .WithMiddlewareDiscovery()
            .WithStatisticsTracking();

        // Assert
        Assert.Same(config, result);
        Assert.Equal(2, config.Assemblies.Count);
        Assert.True(config.DiscoverMiddleware);
    }

    #region WithoutTelemetry Tests

    [Fact]
    public void WithoutTelemetry_ClearsTelemetryOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithTelemetry(); // First enable telemetry
        Assert.NotNull(config.TelemetryOptions); // Verify it was set

        // Act
        var result = config.WithoutTelemetry();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.TelemetryOptions); // Should clear the telemetry options
    }

    [Fact]
    public void WithoutTelemetry_WhenTelemetryNotSet_DoesNothing()
    {
        // Arrange
        var config = new MediatorConfiguration();
        Assert.Null(config.TelemetryOptions); // Verify telemetry is not set

        // Act
        var result = config.WithoutTelemetry();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.TelemetryOptions); // Should remain null
    }

    [Fact]
    public void WithoutTelemetry_CanChainWithOtherMethods()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .WithTelemetry()
            .WithoutTelemetry()
            .WithStatisticsTracking()
            .AddFromAssembly<string>();

        // Assert
        Assert.Same(config, result);
        Assert.Null(config.TelemetryOptions);
        Assert.NotNull(config.StatisticsOptions);
        Assert.Single(config.Assemblies);
    }

    #endregion

    #region WithoutStatistics Tests

    [Fact]
    public void WithoutStatistics_ClearsStatisticsOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithStatisticsTracking(); // First enable statistics
        Assert.NotNull(config.StatisticsOptions); // Verify it was set
#pragma warning disable CS0618 // Suppress obsolete warning for test
        Assert.True(config.EnableStatisticsTracking); // Verify backwards compatibility flag was set
#pragma warning restore CS0618

        // Act
        var result = config.WithoutStatistics();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.StatisticsOptions); // Should clear the statistics options
#pragma warning disable CS0618 // Suppress obsolete warning for test
        Assert.False(config.EnableStatisticsTracking); // Should clear backwards compatibility flag
#pragma warning restore CS0618
    }

    [Fact]
    public void WithoutStatistics_WhenStatisticsNotSet_DoesNothing()
    {
        // Arrange
        var config = new MediatorConfiguration();
        Assert.Null(config.StatisticsOptions); // Verify statistics is not set
#pragma warning disable CS0618 // Suppress obsolete warning for test
        Assert.False(config.EnableStatisticsTracking); // Verify backwards compatibility flag is false
#pragma warning restore CS0618

        // Act
        var result = config.WithoutStatistics();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.StatisticsOptions); // Should remain null
#pragma warning disable CS0618 // Suppress obsolete warning for test
        Assert.False(config.EnableStatisticsTracking); // Should remain false
#pragma warning restore CS0618
    }

    [Fact]
    public void WithoutStatistics_CanChainWithOtherMethods()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .WithStatisticsTracking()
            .WithoutStatistics()
            .WithTelemetry()
            .AddFromAssembly<string>();

        // Assert
        Assert.Same(config, result);
        Assert.Null(config.StatisticsOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.Single(config.Assemblies);
    }

    #endregion

    #region WithoutLogging Tests

    [Fact]
    public void WithoutLogging_ClearsLoggingOptions()
    {
        // Arrange
        var config = new MediatorConfiguration();
        config.WithLogging(); // First enable logging
        Assert.NotNull(config.LoggingOptions); // Verify it was set

        // Act
        var result = config.WithoutLogging();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.LoggingOptions); // Should clear the logging options
    }

    [Fact]
    public void WithoutLogging_WhenLoggingNotSet_DoesNothing()
    {
        // Arrange
        var config = new MediatorConfiguration();
        Assert.Null(config.LoggingOptions); // Verify logging is not set

        // Act
        var result = config.WithoutLogging();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.LoggingOptions); // Should remain null
    }

    [Fact]
    public void WithoutLogging_CanChainWithOtherMethods()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config
            .WithLogging()
            .WithoutLogging()
            .WithTelemetry()
            .AddFromAssembly<string>();

        // Assert
        Assert.Same(config, result);
        Assert.Null(config.LoggingOptions);
        Assert.NotNull(config.TelemetryOptions);
        Assert.Single(config.Assemblies);
    }

    #endregion

    #region Integration Tests for All Without Methods

    [Fact]
    public void WithoutMethods_CanAllBeChainedTogether()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // First enable everything
        config
            .WithTelemetry()
            .WithStatisticsTracking()
            .WithLogging();

        // Verify everything is enabled
        Assert.NotNull(config.TelemetryOptions);
        Assert.NotNull(config.StatisticsOptions);
        Assert.NotNull(config.LoggingOptions);

        // Act - Disable everything
        var result = config
            .WithoutTelemetry()
            .WithoutStatistics()
            .WithoutLogging();

        // Assert
        Assert.Same(config, result); // Should return same instance for fluent chaining
        Assert.Null(config.TelemetryOptions); // Should clear telemetry options
        Assert.Null(config.StatisticsOptions); // Should clear statistics options
        Assert.Null(config.LoggingOptions); // Should clear logging options
    }

    [Fact]
    public void WithoutMethods_CanBeUsedSelectively()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act - Enable all, then selectively disable some
        var result = config
            .WithTelemetry()
            .WithStatisticsTracking()
            .WithLogging()
            .WithoutStatistics() // Only disable statistics
            .AddFromAssembly<string>();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.TelemetryOptions); // Should remain enabled
        Assert.Null(config.StatisticsOptions); // Should be disabled
        Assert.NotNull(config.LoggingOptions); // Should remain enabled
        Assert.Single(config.Assemblies);
    }

    [Fact]
    public void WithoutMethods_AllowEnableDisableToggling()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act - Test toggling telemetry on and off multiple times
        var result = config
            .WithTelemetry()
            .WithoutTelemetry()
            .WithTelemetry(opt => opt.Enabled = true)
            .WithoutTelemetry()
            .WithTelemetry();

        // Assert
        Assert.Same(config, result);
        Assert.NotNull(config.TelemetryOptions); // Should end up enabled
    }

    [Fact]
    public void WithoutMethods_WorkWithComplexFluentChain()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act - Complex fluent configuration
        var result = config
            .AddFromAssembly<string>()
            .WithMiddlewareDiscovery()
            .WithTelemetry(opt => opt.Enabled = true)
            .WithStatisticsTracking(opt => opt.EnablePerformanceCounters = true)
            .WithLogging()
            .WithNotificationHandlerDiscovery()
            .WithoutStatistics() // Disable statistics mid-chain
            .AddFromAssembly<MediatorConfiguration>()
            .WithoutLogging(); // Disable logging at end

        // Assert
        Assert.Same(config, result);
        Assert.Equal(2, config.Assemblies.Count); // Should have both assemblies
        Assert.True(config.DiscoverMiddleware); // Should be enabled
        Assert.True(config.DiscoverNotificationHandlers); // Should be enabled
        Assert.NotNull(config.TelemetryOptions); // Should remain enabled
        Assert.Null(config.StatisticsOptions); // Should be disabled
        Assert.Null(config.LoggingOptions); // Should be disabled
    }

    #endregion
}
