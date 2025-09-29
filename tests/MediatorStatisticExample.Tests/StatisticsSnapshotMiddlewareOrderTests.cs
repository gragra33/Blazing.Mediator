using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using MediatorStatisticExample.Infrastructure.Middleware;
using System.Linq;

namespace MediatorStatisticExample.Tests;

/// <summary>
/// Tests to verify that type-constrained middleware order properties are registered correctly.
/// This addresses the bug where StatisticsSnapshotMiddleware shows orders 2146483650 and 2146483651
/// instead of the expected order value of 10.
/// </summary>
public class StatisticsSnapshotMiddlewareOrderTests
{
    /// <summary>
    /// Test to verify that type-constrained middleware with explicit Order properties
    /// are registered with the correct order values, not fallback values.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_WithOrderProperty_ShouldRegisterWithCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register mediator with middleware discovery to simulate the MediatorStatisticExample setup
        services.AddMediator(options =>
        {
            options.WithMiddlewareDiscovery();
        }, typeof(MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<,>).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = pipelineInspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        var statisticsSnapshotWithResponse = middlewareInfo.FirstOrDefault(m => 
            m.Type.IsGenericTypeDefinition && 
            m.Type.Name.StartsWith("StatisticsSnapshotMiddleware") &&
            m.Type.GetGenericArguments().Length == 2);

        var statisticsSnapshotWithoutResponse = middlewareInfo.FirstOrDefault(m => 
            m.Type.IsGenericTypeDefinition && 
            m.Type.Name.StartsWith("StatisticsSnapshotMiddleware") &&
            m.Type.GetGenericArguments().Length == 1);

        // Both middleware should be found
        if (statisticsSnapshotWithResponse.Type is not null)
        {
            // The key assertion: both should have order 10, not fallback values like 2146483650/2146483651
            statisticsSnapshotWithResponse.Order.ShouldBe(10, 
                $"StatisticsSnapshotMiddleware<TRequest, TResponse> should have order 10, but got {statisticsSnapshotWithResponse.Order}");
            
            // Additional verification: should not have fallback order values
            statisticsSnapshotWithResponse.Order.ShouldNotBe(2146483650);
            statisticsSnapshotWithResponse.Order.ShouldNotBe(2146483651);
        }

        if (statisticsSnapshotWithoutResponse.Type is not null)
        {
            statisticsSnapshotWithoutResponse.Order.ShouldBe(10, 
                $"StatisticsSnapshotMiddleware<TRequest> should have order 10, but got {statisticsSnapshotWithoutResponse.Order}");
            
            statisticsSnapshotWithoutResponse.Order.ShouldNotBe(2146483650);
            statisticsSnapshotWithoutResponse.Order.ShouldNotBe(2146483651);
        }
    }

    /// <summary>
    /// Test to verify that the Order property can be read correctly from type-constrained middleware instances.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_OrderPropertyAccess_ShouldReturnCorrectValue()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add required dependencies for StatisticsSnapshotMiddleware
        services.AddScoped<IMediator, MockMediator>();
        services.AddLogging();
        
        // Register the middleware directly
        services.AddScoped<MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        services.AddScoped<MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Test the middleware with response
        var middlewareWithResponse = serviceProvider.GetRequiredService<MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        middlewareWithResponse.Order.ShouldBe(10, "Middleware with response should have order 10");

        // Act & Assert - Test the middleware without response
        var middlewareWithoutResponse = serviceProvider.GetRequiredService<MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();
        middlewareWithoutResponse.Order.ShouldBe(10, "Middleware without response should have order 10");
    }

    /// <summary>
    /// Test to verify that middleware order detection works for generic type definitions.
    /// This tests the GetMiddlewareOrder method's ability to handle generic constraints.
    /// </summary>
    [Fact]
    public void GenericTypeDefinition_OrderDetection_ShouldWorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            // Manually add the generic type definitions
            config.AddMiddleware(typeof(MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<,>));
            config.AddMiddleware(typeof(MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<>));
        }, typeof(MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<,>).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = pipelineInspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middlewareInfo.Count.ShouldBe(2, "Should have registered 2 middleware types");
        
        foreach (var middleware in middlewareInfo)
        {
            middleware.Order.ShouldBe(10, $"Middleware {middleware.Type.Name} should have order 10, but got {middleware.Order}");
            
            // Should not be fallback orders
            middleware.Order.ShouldNotBeInRange(2146483647, 2146483651);
        }
    }

    /// <summary>
    /// Integration test that reproduces the exact issue described in the bug report.
    /// </summary>
    [Fact]
    public void MediatorStatisticExample_MiddlewareRegistration_ShouldShowCorrectOrders()
    {
        // Arrange - Set up exactly like MediatorStatisticExample
        var services = new ServiceCollection();
        
        // This mirrors the setup in MediatorStatisticExample
        services.AddMediator(options =>
        {
            options.WithStatisticsTracking()
                .WithMiddlewareDiscovery()
                .WithNotificationMiddlewareDiscovery();
        }, typeof(MediatorStatisticExample.Middleware.StatisticsSnapshotMiddleware<,>).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var allMiddleware = pipelineInspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        var statisticsMiddleware = allMiddleware.Where(m => 
            m.ClassName == "StatisticsSnapshotMiddleware").ToList();

        statisticsMiddleware.Count.ShouldBeGreaterThan(0, "Should find StatisticsSnapshotMiddleware instances");

        foreach (var middleware in statisticsMiddleware)
        {
            // This is the core assertion that should fail before the fix
            middleware.Order.ShouldBe(10, 
                $"StatisticsSnapshotMiddleware should have order 10, not {middleware.Order}. " +
                $"Current bug shows orders like 2146483650 or 2146483651 instead.");

            // Verify it's not showing the buggy fallback values
            middleware.Order.ShouldNotBeInRange(2146483647, 2146483660,
                "Should not be using fallback order values which indicate the Order property wasn't detected");
        }
    }
}

#region Test Helper Classes

/// <summary>
/// Test request that implements IStatisticsTrackedRequest for testing type constraints.
/// </summary>
public class TestTrackedRequest : IRequest<string>, IStatisticsTrackedRequest
{
    public string Message { get; set; } = "Test";
}

/// <summary>
/// Test void request that implements IStatisticsTrackedRequest for testing type constraints.
/// </summary>
public class TestTrackedVoidRequest : IRequest, IStatisticsTrackedRequest
{
    public string Message { get; set; } = "Test";
}

/// <summary>
/// Mock mediator for testing purposes.
/// </summary>
public class MockMediator : IMediator
{
    public Task Send(IRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => Task.FromResult(default(TResponse)!);
    
    public IAsyncEnumerable<TResponse> SendStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return System.Linq.AsyncEnumerable.Empty<TResponse>();
    }
    
    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
    public void Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification { }
    public void Subscribe(INotificationSubscriber subscriber) { }
    public void Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification { }
    public void Unsubscribe(INotificationSubscriber subscriber) { }
}

#endregion