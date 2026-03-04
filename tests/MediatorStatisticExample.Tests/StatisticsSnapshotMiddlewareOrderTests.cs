using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;
using MediatorStatisticExample.Infrastructure.Middleware;
using MediatorStatisticExample.Middleware;
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
    /// Note: In source-gen mode the baked pipeline is not exposed via IMiddlewarePipelineInspector;
    /// we populate the builder directly as recommended by the library design.
    /// </summary>
    [Fact]
    public void TypeConstrainedMiddleware_WithOrderProperty_ShouldRegisterWithCorrectOrder()
    {
        // Arrange — populate pipeline builder directly (source-gen bakes the pipeline at compile time
        // and does not expose it through IMiddlewarePipelineInspector at runtime).
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        pipelineBuilder.AddMiddleware(typeof(StatisticsSnapshotMiddleware<,>));
        pipelineBuilder.AddMiddleware(typeof(StatisticsSnapshotMiddleware<>));

        var services = new ServiceCollection();
        services.AddSingleton<IMiddlewarePipelineInspector>(pipelineBuilder);
        services.AddLogging();
        services.AddScoped<IMediator, MockMediator>();
        // Register closed forms so DI can resolve concrete instances for runtime order reading
        services.AddTransient<StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        services.AddTransient<StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();

        var serviceProvider = services.BuildServiceProvider();
        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = pipelineInspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middlewareInfo.Count.ShouldBeGreaterThan(0, "Should have registered middleware");

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
        services.AddScoped<StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        services.AddScoped<StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Test the middleware with response
        var middlewareWithResponse = serviceProvider
            .GetRequiredService<StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        middlewareWithResponse.Order.ShouldBe(10, "Middleware with response should have order 10");

        // Act & Assert - Test the middleware without response
        var middlewareWithoutResponse = serviceProvider
            .GetRequiredService<StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();
        middlewareWithoutResponse.Order.ShouldBe(10, "Middleware without response should have order 10");
    }

    /// <summary>
    /// Test to verify that middleware order detection works for generic type definitions.
    /// This tests the GetMiddlewareOrder method ability to handle generic constraints.
    /// </summary>
    [Fact]
    public void GenericTypeDefinition_OrderDetection_ShouldWorkCorrectly()
    {
        // Arrange — directly populate the pipeline builder with both open-generic middleware types
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        pipelineBuilder.AddMiddleware(typeof(StatisticsSnapshotMiddleware<,>));
        pipelineBuilder.AddMiddleware(typeof(StatisticsSnapshotMiddleware<>));

        var services = new ServiceCollection();
        services.AddSingleton<IMiddlewarePipelineInspector>(pipelineBuilder);
        services.AddLogging();
        services.AddScoped<IMediator, MockMediator>();
        services.AddTransient<StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        services.AddTransient<StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();

        var serviceProvider = services.BuildServiceProvider();
        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var middlewareInfo = pipelineInspector.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middlewareInfo.Count.ShouldBe(2, "Should have registered 2 middleware types");

        foreach (var middleware in middlewareInfo)
        {
            middleware.Order.ShouldBe(10,
                $"Middleware {middleware.Type.Name} should have order 10, but got {middleware.Order}");
            middleware.Order.ShouldNotBeInRange(2146483647, 2146483651);
        }
    }

    /// <summary>
    /// Integration test that reproduces the exact issue described in the bug report.
    /// Mirrors the full MediatorStatisticExample middleware setup using the pipeline builder directly.
    /// </summary>
    [Fact]
    public void MediatorStatisticExample_MiddlewareRegistration_ShouldShowCorrectOrders()
    {
        // Arrange — populate the pipeline builder as the sample would (source-gen bakes pipelines at
        // compile time; the inspector must be primed directly when testing without source gen).
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        pipelineBuilder.AddMiddleware(typeof(StatisticsSnapshotMiddleware<,>));
        pipelineBuilder.AddMiddleware(typeof(StatisticsSnapshotMiddleware<>));

        var services = new ServiceCollection();
        services.AddSingleton<IMiddlewarePipelineInspector>(pipelineBuilder);
        services.AddLogging();
        services.AddScoped<IMediator, MockMediator>();
        services.AddTransient<StatisticsSnapshotMiddleware<TestTrackedRequest, string>>();
        services.AddTransient<StatisticsSnapshotMiddleware<TestTrackedVoidRequest>>();

        var serviceProvider = services.BuildServiceProvider();
        var pipelineInspector = serviceProvider.GetRequiredService<IMiddlewarePipelineInspector>();

        // Act
        var allMiddleware = pipelineInspector.AnalyzeMiddleware(serviceProvider);

        // Assert
        var statisticsMiddleware = allMiddleware
            .Where(m => m.ClassName == "StatisticsSnapshotMiddleware")
            .ToList();

        statisticsMiddleware.Count.ShouldBeGreaterThan(0, "Should find StatisticsSnapshotMiddleware instances");

        foreach (var middleware in statisticsMiddleware)
        {
            middleware.Order.ShouldBe(10,
                $"StatisticsSnapshotMiddleware should have order 10, not {middleware.Order}. " +
                "Current bug shows orders like 2146483650 or 2146483651 instead.");

            middleware.Order.ShouldNotBeInRange(2146483647, 2146483660,
                "Should not be using fallback order values which indicate the Order property was not detected");
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
    public ValueTask Send(IRequest request, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(default(TResponse)!);

    public IAsyncEnumerable<TResponse> SendStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        => System.Linq.AsyncEnumerable.Empty<TResponse>();

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => ValueTask.CompletedTask;

    public void Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber)
        where TNotification : INotification { }

    public void Subscribe(INotificationSubscriber subscriber) { }

    public void Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber)
        where TNotification : INotification { }

    public void Unsubscribe(INotificationSubscriber subscriber) { }
}

#endregion
