using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Test to verify that type-constrained middleware constraint satisfaction is working correctly.
/// This test was originally created to debug the constraint satisfaction issue and has been 
/// updated to be a proper test now that the issue is resolved.
/// </summary>
public class DebugConstraintTest
{
    [Fact]
    public void Debug_TypeConstrainedMiddleware_OrderDetection()
    {
        // Enable console output for debug messages
        Trace.Listeners.Add(new ConsoleTraceListener());
        Debug.WriteLine("=== STARTING DEBUG TEST ===");
        
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator(config =>
        {
            config.AddMiddleware(typeof(TypeConstrainedWithOrderMiddleware<,>));
        }, discoverMiddleware: false, discoverNotificationMiddleware: false);

        var serviceProvider = services.BuildServiceProvider();
        var inspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert & Debug
        Debug.WriteLine($"Found {middlewareInfo.Count} middleware");
        foreach (var middleware in middlewareInfo)
        {
            Debug.WriteLine($"Middleware: {middleware.Type.Name}, Order: {middleware.Order}");
        }

        // The middleware should have order 10, not a fallback value
        var typeConstrainedMiddleware = middlewareInfo.First(m => m.Type.Name.Contains("TypeConstrained"));
        
        Debug.WriteLine($"Expected order: 10, Actual order: {typeConstrainedMiddleware.Order}");
        Debug.WriteLine("=== END DEBUG TEST ===");
        
        // Now that the fix is complete, this should pass instead of throwing an exception
        Assert.Equal(10, typeConstrainedMiddleware.Order);
        Assert.Equal("TypeConstrainedWithOrderMiddleware`2", typeConstrainedMiddleware.Type.Name);
        
        Debug.WriteLine("? DEBUG TEST PASSED - Type-constrained middleware constraint satisfaction is working correctly!");
    }
}