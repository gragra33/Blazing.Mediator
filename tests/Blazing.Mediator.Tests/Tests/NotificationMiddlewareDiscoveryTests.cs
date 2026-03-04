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
}