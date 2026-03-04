using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.ConfigurationTests;

/// <summary>
/// Tests for DiscoverConstrainedMiddleware property functionality.
/// Validates that constrained middleware discovery can be enabled/disabled independently.
/// </summary>
public class ConstrainedMiddlewareDiscoveryTests
{
    private readonly Assembly _testAssembly = typeof(ConstrainedMiddlewareDiscoveryTests).Assembly;

    [Fact]
    public void DiscoverConstrainedMiddleware_DefaultValue_ShouldBeTrue()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act & Assert
        Assert.True(config.DiscoverConstrainedMiddleware);
    }

    [Fact]
    public void WithConstrainedMiddlewareDiscovery_ShouldSetPropertyToTrue()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithConstrainedMiddlewareDiscovery();

        // Assert
        Assert.Same(config, result); // Should return same instance for chaining
        Assert.True(config.DiscoverConstrainedMiddleware);
    }

    [Fact]
    public void WithoutConstrainedMiddlewareDiscovery_ShouldSetPropertyToFalse()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act
        var result = config.WithoutConstrainedMiddlewareDiscovery();

        // Assert
        Assert.Same(config, result); // Should return same instance for chaining
        Assert.False(config.DiscoverConstrainedMiddleware);
    }

    [Fact]
    public void ChainedConfiguration_ConstrainedMiddlewareDiscovery_ShouldWorkCorrectly()
    {
        // Arrange
        var config = new MediatorConfiguration();

        // Act - Chain configuration methods
        var result = config.WithNotificationMiddlewareDiscovery()
                          .WithConstrainedMiddlewareDiscovery()
                          .WithoutConstrainedMiddlewareDiscovery() // Last one wins
                          .WithConstrainedMiddlewareDiscovery(); // Back to true

        // Assert
        Assert.Same(config, result);
        Assert.True(config.DiscoverConstrainedMiddleware);
    }
}

// Test constrained middleware for testing purposes
public class ConstrainedMiddlewareTestMiddleware : INotificationMiddleware<ConstrainedTestNotification>
{
    public int Order => 100;

    public ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        return next(notification, cancellationToken);
    }

    public ValueTask InvokeAsync(ConstrainedTestNotification notification, NotificationDelegate<ConstrainedTestNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }
}

public class ConstrainedTestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}
