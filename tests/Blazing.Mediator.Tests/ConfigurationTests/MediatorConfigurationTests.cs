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
}
