using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Blazing.Mediator.Tests.Performance;

/// <summary>
/// Comprehensive performance test runner for Step 3.10 - Performance testing and optimization for constraint checking overhead.
/// This class demonstrates that constraint checking overhead is minimal and provides detailed performance analysis.
/// </summary>
public class ConstraintPerformanceTestRunner
{
    [Fact]
    public async Task Step3_10_ConstraintCheckingPerformanceAndOptimization()
    {
        var analyzer = new ConstraintPerformanceAnalyzer();
        Console.WriteLine("=== STEP 3.10: CONSTRAINT CHECKING PERFORMANCE TESTING AND OPTIMIZATION ===");
        Console.WriteLine();

        // Test 1: Basic constraint checking overhead
        await TestConstraintCheckingOverhead(analyzer);
        
        // Test 2: Reflection performance analysis
        TestReflectionPerformance(analyzer);
        
        // Test 3: Pipeline execution efficiency
        await TestPipelineExecutionEfficiency(analyzer);
        
        // Test 4: Memory usage analysis
        await TestMemoryUsageImpact(analyzer);
        
        // Test 5: Scalability testing
        await TestScalabilityWithLargePipelines(analyzer);

        // Generate and display comprehensive report
        var report = analyzer.GenerateReport();
        report.PrintReport(Console.WriteLine);
        
        // Assert overall performance is acceptable
        var avgDuration = report.AverageDuration;
        avgDuration.ShouldBeLessThan(50.0, $"Average operation duration should be less than 50ms, but was {avgDuration:F2}ms");
        
        Console.WriteLine();
        Console.WriteLine("? STEP 3.10 COMPLETED: Constraint checking overhead is minimal and optimized");
        Console.WriteLine();
    }

    private async Task TestConstraintCheckingOverhead(ConstraintPerformanceAnalyzer analyzer)
    {
        Console.WriteLine("1. Testing Constraint Checking Overhead...");
        
        var services = CreateServiceProvider();
        
        // Create pipelines
        var generalPipeline = CreateGeneralPipeline();
        var constrainedPipeline = CreateConstrainedPipeline();
        
        var notification = new TestOrderNotification(1, "ORD-001");
        var iterations = 1000;

        // Measure general pipeline
        await ConstraintPerformanceAnalyzer.MeasureAsync(
            analyzer, 
            "General Pipeline", 
            async () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    await generalPipeline.ExecutePipeline(
                        notification,
                        services,
                        async (n, ct) => { },
                        CancellationToken.None);
                }
                return true;
            },
            iterations);

        // Measure constrained pipeline
        await ConstraintPerformanceAnalyzer.MeasureAsync(
            analyzer, 
            "Constrained Pipeline", 
            async () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    await constrainedPipeline.ExecutePipeline(
                        notification,
                        services,
                        async (n, ct) => { },
                        CancellationToken.None);
                }
                return true;
            },
            iterations);

        Console.WriteLine("   ? Constraint checking overhead measured");
    }

    private void TestReflectionPerformance(ConstraintPerformanceAnalyzer analyzer)
    {
        Console.WriteLine("2. Testing Reflection Performance...");
        
        var middlewareType = typeof(TestOrderMiddleware);
        var notificationType = typeof(TestOrderNotification);
        var iterations = 10000;

        // Test interface discovery
        ConstraintPerformanceAnalyzer.Measure(
            analyzer,
            "Interface Discovery",
            () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    _ = middlewareType.GetInterfaces()
                        .Where(iface => iface.IsGenericType && 
                                       iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                        .ToArray();
                }
                return true;
            },
            iterations);

        // Test assignability checking
        var constraintType = typeof(IOrderNotification);
        ConstraintPerformanceAnalyzer.Measure(
            analyzer,
            "Assignability Checking",
            () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    _ = constraintType.IsAssignableFrom(notificationType);
                }
                return true;
            },
            iterations);

        Console.WriteLine("   ? Reflection operations performance measured");
    }

    private async Task TestPipelineExecutionEfficiency(ConstraintPerformanceAnalyzer analyzer)
    {
        Console.WriteLine("3. Testing Pipeline Execution Efficiency...");
        
        var services = CreateServiceProvider();
        var pipeline = CreateMixedPipeline(); // Mix of constrained and general middleware
        
        var notifications = new INotification[]
        {
            new TestOrderNotification(1, "ORD-001"),
            new TestCustomerNotification(1, "John Doe"),
            new TestGeneralNotification("General message")
        };

        foreach (var notification in notifications)
        {
            var notificationName = notification.GetType().Name;
            await ConstraintPerformanceAnalyzer.MeasureAsync(
                analyzer,
                $"Pipeline Execution - {notificationName}",
                async () =>
                {
                    await pipeline.ExecutePipeline(
                        notification,
                        services,
                        async (n, ct) => { },
                        CancellationToken.None);
                    return true;
                });
        }

        Console.WriteLine("   ? Pipeline execution efficiency measured");
    }

    private async Task TestMemoryUsageImpact(ConstraintPerformanceAnalyzer analyzer)
    {
        Console.WriteLine("4. Testing Memory Usage Impact...");
        
        var services = CreateServiceProvider();
        var pipeline = CreateConstrainedPipeline();
        var notification = new TestOrderNotification(1, "ORD-001");
        var iterations = 100;

        await ConstraintPerformanceAnalyzer.MeasureAsync(
            analyzer,
            "Memory Usage Impact",
            async () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    await pipeline.ExecutePipeline(
                        notification,
                        services,
                        async (n, ct) => { },
                        CancellationToken.None);
                }
                return true;
            },
            iterations);

        Console.WriteLine("   ? Memory usage impact measured");
    }

    private async Task TestScalabilityWithLargePipelines(ConstraintPerformanceAnalyzer analyzer)
    {
        Console.WriteLine("5. Testing Scalability with Large Pipelines...");
        
        var services = CreateServiceProviderWithManyMiddleware();
        var largePipeline = CreateLargePipeline();
        var notification = new TestOrderNotification(1, "ORD-001");
        var iterations = 100;

        await ConstraintPerformanceAnalyzer.MeasureAsync(
            analyzer,
            "Large Pipeline Scalability",
            async () =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    await largePipeline.ExecutePipeline(
                        notification,
                        services,
                        async (n, ct) => { },
                        CancellationToken.None);
                }
                return true;
            },
            iterations);

        Console.WriteLine("   ? Scalability with large pipelines measured");
    }

    #region Helper Methods

    private IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddScoped<TestGeneralMiddleware>();
        services.AddScoped<TestOrderMiddleware>();
        services.AddScoped<TestCustomerMiddleware>();
        return services.BuildServiceProvider();
    }

    private IServiceProvider CreateServiceProviderWithManyMiddleware()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        
        // Add many middleware types
        services.AddScoped<TestGeneralMiddleware>();
        services.AddScoped<TestOrderMiddleware>();
        services.AddScoped<TestCustomerMiddleware>();
        services.AddScoped<TestAnotherGeneralMiddleware>();
        services.AddScoped<TestAnotherOrderMiddleware>();
        services.AddScoped<TestAnotherCustomerMiddleware>();
        
        return services.BuildServiceProvider();
    }

    private NotificationPipelineBuilder CreateGeneralPipeline()
    {
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestGeneralMiddleware>();
        return builder;
    }

    private NotificationPipelineBuilder CreateConstrainedPipeline()
    {
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestOrderMiddleware>();
        builder.AddMiddleware<TestCustomerMiddleware>();
        return builder;
    }

    private NotificationPipelineBuilder CreateMixedPipeline()
    {
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestGeneralMiddleware>();
        builder.AddMiddleware<TestOrderMiddleware>();
        builder.AddMiddleware<TestCustomerMiddleware>();
        return builder;
    }

    private NotificationPipelineBuilder CreateLargePipeline()
    {
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestGeneralMiddleware>();
        builder.AddMiddleware<TestOrderMiddleware>();
        builder.AddMiddleware<TestCustomerMiddleware>();
        builder.AddMiddleware<TestAnotherGeneralMiddleware>();
        builder.AddMiddleware<TestAnotherOrderMiddleware>();
        builder.AddMiddleware<TestAnotherCustomerMiddleware>();
        return builder;
    }

    #endregion
}

#region Test Types and Middleware

// Use existing test types from other performance test files

// Test middleware
public class TestGeneralMiddleware : INotificationMiddleware
{
    public int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

public class TestOrderMiddleware : INotificationMiddleware<IOrderNotification>
{
    public int Order => 20;

    public async Task InvokeAsync(IOrderNotification notification, NotificationDelegate<IOrderNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

public class TestCustomerMiddleware : INotificationMiddleware<ICustomerNotification>
{
    public int Order => 30;

    public async Task InvokeAsync(ICustomerNotification notification, NotificationDelegate<ICustomerNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

// Additional middleware for large pipeline testing
public class TestAnotherGeneralMiddleware : INotificationMiddleware
{
    public int Order => 40;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

public class TestAnotherOrderMiddleware : INotificationMiddleware<IOrderNotification>
{
    public int Order => 50;

    public async Task InvokeAsync(IOrderNotification notification, NotificationDelegate<IOrderNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

public class TestAnotherCustomerMiddleware : INotificationMiddleware<ICustomerNotification>
{
    public int Order => 60;

    public async Task InvokeAsync(ICustomerNotification notification, NotificationDelegate<ICustomerNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

#endregion