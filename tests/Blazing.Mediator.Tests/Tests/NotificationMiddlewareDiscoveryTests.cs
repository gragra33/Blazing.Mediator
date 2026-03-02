using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using static Blazing.Mediator.Tests.NotificationTests.NotificationMiddlewareTests;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for notification middleware auto-discovery functionality.
/// Tests the discoverNotificationMiddleware parameter and AddMediatorWithNotificationMiddleware methods.
/// </summary>
public class NotificationMiddlewareDiscoveryTests
{
    private readonly Assembly _testAssembly = typeof(NotificationMiddlewareDiscoveryTests).Assembly;

    /// <summary>
    /// Test AddMediatorWithNotificationMiddleware method with discovery enabled
    /// </summary>
    /// <summary>
    /// Test AddMediatorWithNotificationMiddleware with Type[] parameters
    /// </summary>
    /// <summary>
    /// Test mixed scenario: discover notification middleware but not request middleware
    /// </summary>
    /// <summary>
    /// Test that manual + auto-discovery works together for notification middleware
    /// </summary>
    /// <summary>
    /// Test that notification middleware ordering is preserved with auto-discovery
    /// </summary>
}