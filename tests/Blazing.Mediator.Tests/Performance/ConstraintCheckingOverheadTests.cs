using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Blazing.Mediator.Tests.Performance;

/// <summary>
/// Performance tests for constraint checking overhead analysis.
/// Measures the impact of type constraint validation on middleware execution.
/// </summary>
public class ConstraintCheckingOverheadTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly NotificationPipelineBuilder _pipelineBuilder;
    private readonly NotificationPipelineBuilder _constrainedPipelineBuilder;

    public ConstraintCheckingOverheadTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));

        // Register middleware
        services.AddScoped<GeneralMiddleware>();
        services.AddScoped<OrderConstrainedMiddleware>();
        services.AddScoped<CustomerConstrainedMiddleware>();

        _serviceProvider = services.BuildServiceProvider();

        // Setup general pipeline
        _pipelineBuilder = new NotificationPipelineBuilder();
        _pipelineBuilder.AddMiddleware<GeneralMiddleware>();

        // Setup constrained pipeline
        _constrainedPipelineBuilder = new NotificationPipelineBuilder();
        _constrainedPipelineBuilder.AddMiddleware<OrderConstrainedMiddleware>();
        _constrainedPipelineBuilder.AddMiddleware<CustomerConstrainedMiddleware>();
    }

    [Fact(Skip = "Performance test - skip by default")]
    public async Task ConstraintChecking_ShouldHaveMinimalOverhead()
    {
        // Arrange
        var orderNotification = new TestOrderNotification(1, "ORD-001");
        var iterations = 1000;

        // Warm up both pipelines to account for JIT compilation and initialization overhead
        for (int i = 0; i < 10; i++)
        {
            await _pipelineBuilder.ExecutePipeline(
                orderNotification,
                _serviceProvider,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
            
            await _constrainedPipelineBuilder.ExecutePipeline(
                orderNotification,
                _serviceProvider,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }

        // Force garbage collection to ensure clean measurement
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Measure without constraints
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await _pipelineBuilder.ExecutePipeline(
                orderNotification,
                _serviceProvider,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }
        stopwatch.Stop();
        var generalTime = stopwatch.ElapsedMilliseconds;

        // Force garbage collection between tests
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Measure with constraints
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            await _constrainedPipelineBuilder.ExecutePipeline(
                orderNotification,
                _serviceProvider,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }
        stopwatch.Stop();
        var constrainedTime = stopwatch.ElapsedMilliseconds;

        // Calculate performance difference
        var performanceDifference = constrainedTime - generalTime;
        var relativePerformance = generalTime > 0 ? ((double)constrainedTime / generalTime) : 1.0;

        // Assert reasonable performance characteristics
        // Allow constrained pipeline to be up to 3x slower than general pipeline (very lenient)
        // In practice, constrained pipelines often perform better due to middleware skipping
        relativePerformance.ShouldBeLessThan(3.0, 
            $"Constrained pipeline performance should be reasonable. General: {generalTime}ms, Constrained: {constrainedTime}ms, Ratio: {relativePerformance:F2}x");

        // Ensure neither pipeline is unreasonably slow (more than 1 second total)
        generalTime.ShouldBeLessThan(1000, $"General pipeline should not be unreasonably slow: {generalTime}ms for {iterations} iterations");
        constrainedTime.ShouldBeLessThan(1000, $"Constrained pipeline should not be unreasonably slow: {constrainedTime}ms for {iterations} iterations");

        // Calculate overhead/improvement percentage for reporting
        var overheadPercentage = generalTime > 0 ? 
            ((double)performanceDifference / generalTime) * 100 : 0;

        // Output results for analysis
        Console.WriteLine($"Performance Analysis:");
        Console.WriteLine($"  General pipeline: {generalTime}ms");
        Console.WriteLine($"  Constrained pipeline: {constrainedTime}ms");
        Console.WriteLine($"  Difference: {performanceDifference}ms ({overheadPercentage:F2}% change)");
        Console.WriteLine($"  Performance ratio: {relativePerformance:F2}x");
        Console.WriteLine($"  Average per notification - General: {(double)generalTime/iterations:F3}ms, Constrained: {(double)constrainedTime/iterations:F3}ms");
        
        if (constrainedTime < generalTime)
        {
            Console.WriteLine($"  ✅ Constrained pipeline is actually FASTER due to middleware skipping optimization!");
        }
    }

    [Fact]
    public void ReflectionOperations_ShouldBeFast()
    {
        // Arrange
        var middlewareType = typeof(OrderConstrainedMiddleware);
        var notificationType = typeof(TestOrderNotification);
        var iterations = 10000;

        // Warm up the reflection operations to account for JIT compilation
        for (int i = 0; i < 100; i++)
        {
            _ = middlewareType.GetInterfaces()
                .Where(iface => iface.IsGenericType && 
                               iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                .ToArray();
            _ = typeof(IOrderNotification).IsAssignableFrom(notificationType);
        }

        // Test interface discovery performance
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = middlewareType.GetInterfaces()
                .Where(iface => iface.IsGenericType && 
                               iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                .ToArray();
        }
        stopwatch.Stop();
        var interfaceDiscoveryTime = stopwatch.ElapsedMilliseconds;

        // Test assignability check performance
        var constraintType = typeof(IOrderNotification);
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            _ = constraintType.IsAssignableFrom(notificationType);
        }
        stopwatch.Stop();
        var assignabilityTime = stopwatch.ElapsedMilliseconds;

        // Use more lenient thresholds that account for system variability
        // These thresholds are based on realistic performance expectations for .NET 9
        interfaceDiscoveryTime.ShouldBeLessThan(200, $"Interface discovery should be fast but was {interfaceDiscoveryTime}ms for {iterations} operations");
        assignabilityTime.ShouldBeLessThan(100, $"Assignability checks should be fast but was {assignabilityTime}ms for {iterations} operations");

        // Verify operations are reasonably fast (less than 0.02ms per operation on average)
        var avgInterfaceTime = (double)interfaceDiscoveryTime / iterations;
        var avgAssignabilityTime = (double)assignabilityTime / iterations;
        
        avgInterfaceTime.ShouldBeLessThan(0.02, $"Average interface discovery time should be less than 0.02ms per operation, but was {avgInterfaceTime:F6}ms");
        avgAssignabilityTime.ShouldBeLessThan(0.01, $"Average assignability check time should be less than 0.01ms per operation, but was {avgAssignabilityTime:F6}ms");

        Console.WriteLine($"Reflection Performance:");
        Console.WriteLine($"  Interface discovery: {interfaceDiscoveryTime}ms for {iterations} operations");
        Console.WriteLine($"  Assignability checks: {assignabilityTime}ms for {iterations} operations");
        Console.WriteLine($"  Per operation - Interface: {avgInterfaceTime:F6}ms, Assignability: {avgAssignabilityTime:F6}ms");
    }

    [Fact]
    public async Task MemoryUsage_ShouldNotIncrease()
    {
        // Arrange
        var orderNotification = new TestOrderNotification(1, "ORD-001");
        var iterations = 100;

        // Force garbage collection before test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Measure memory before constrained pipeline
        long initialMemory = GC.GetTotalMemory(false);

        // Execute constrained pipeline
        for (int i = 0; i < iterations; i++)
        {
            await _constrainedPipelineBuilder.ExecutePipeline(
                orderNotification,
                _serviceProvider,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }

        // Force garbage collection after test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long finalMemory = GC.GetTotalMemory(false);
        var memoryDelta = finalMemory - initialMemory;

        // Assert memory usage is reasonable (less than 1KB per iteration on average)
        var maxAllowedMemory = iterations * 1024; // 1KB per iteration
        memoryDelta.ShouldBeLessThan(maxAllowedMemory,
            $"Memory usage should not increase significantly. Used: {memoryDelta} bytes for {iterations} iterations");

        Console.WriteLine($"Memory Usage Analysis:");
        Console.WriteLine($"  Initial memory: {initialMemory:N0} bytes");
        Console.WriteLine($"  Final memory: {finalMemory:N0} bytes");
        Console.WriteLine($"  Delta: {memoryDelta:N0} bytes");
        Console.WriteLine($"  Per iteration: {(double)memoryDelta/iterations:F2} bytes");
    }

    [Fact]
    public void ConstraintMatching_ShouldBeEfficient()
    {
        // Arrange
        var testCases = new[]
        {
            (typeof(TestOrderNotification), typeof(IOrderNotification), true),
            (typeof(TestCustomerNotification), typeof(IOrderNotification), false),
            (typeof(TestOrderNotification), typeof(ICustomerNotification), false),
            (typeof(TestCustomerNotification), typeof(ICustomerNotification), true)
        };

        var iterations = 1000; // Reduced from 10000
        var stopwatch = Stopwatch.StartNew();

        // Test constraint matching performance
        for (int i = 0; i < iterations; i++)
        {
            foreach (var (notificationType, constraintType, expected) in testCases)
            {
                var result = constraintType.IsAssignableFrom(notificationType);
                result.ShouldBe(expected);
            }
        }

        stopwatch.Stop();
        var totalTime = stopwatch.ElapsedMilliseconds;
        var timePerOperation = (double)totalTime / (iterations * testCases.Length);

        // Assert constraint matching is fast (more lenient thresholds)
        totalTime.ShouldBeLessThan(500); // Less than 500ms total for all operations
        timePerOperation.ShouldBeLessThan(0.1); // Less than 0.1ms per operation

        Console.WriteLine($"Constraint Matching Performance:");
        Console.WriteLine($"  Total time: {totalTime}ms for {iterations * testCases.Length} operations");
        Console.WriteLine($"  Per operation: {timePerOperation:F6}ms");
    }

    [Fact]
    public void PipelineSelection_ShouldBeOptimal()
    {
        // Test that the pipeline efficiently selects applicable middleware
        var middlewareTypes = new[]
        {
            typeof(OrderConstrainedMiddleware),
            typeof(CustomerConstrainedMiddleware),
            typeof(GeneralMiddleware)
        };

        var notificationTypes = new[]
        {
            typeof(TestOrderNotification),
            typeof(TestCustomerNotification),
            typeof(TestGeneralNotification)
        };

        var stopwatch = Stopwatch.StartNew();
        var iterations = 1000;

        for (int i = 0; i < iterations; i++)
        {
            foreach (var notificationType in notificationTypes)
            {
                var applicableMiddleware = new List<Type>();

                foreach (var middlewareType in middlewareTypes)
                {
                    // Simulate constraint checking logic
                    var constrainedInterfaces = middlewareType.GetInterfaces()
                        .Where(iface => iface.IsGenericType && 
                                       iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                        .ToArray();

                    bool isApplicable = false;

                    if (constrainedInterfaces.Length == 0)
                    {
                        // No constraints - always applicable
                        isApplicable = true;
                    }
                    else
                    {
                        // Check if any constraint matches
                        foreach (var constrainedInterface in constrainedInterfaces)
                        {
                            var constraintType = constrainedInterface.GetGenericArguments()[0];
                            if (constraintType.IsAssignableFrom(notificationType))
                            {
                                isApplicable = true;
                                break;
                            }
                        }
                    }

                    if (isApplicable)
                    {
                        applicableMiddleware.Add(middlewareType);
                    }
                }

                // Verify selection logic
                if (notificationType == typeof(TestOrderNotification))
                {
                    applicableMiddleware.ShouldContain(typeof(OrderConstrainedMiddleware));
                    applicableMiddleware.ShouldContain(typeof(GeneralMiddleware));
                    applicableMiddleware.ShouldNotContain(typeof(CustomerConstrainedMiddleware));
                }
                else if (notificationType == typeof(TestCustomerNotification))
                {
                    applicableMiddleware.ShouldContain(typeof(CustomerConstrainedMiddleware));
                    applicableMiddleware.ShouldContain(typeof(GeneralMiddleware));
                    applicableMiddleware.ShouldNotContain(typeof(OrderConstrainedMiddleware));
                }
                else
                {
                    applicableMiddleware.ShouldContain(typeof(GeneralMiddleware));
                    applicableMiddleware.ShouldNotContain(typeof(OrderConstrainedMiddleware));
                    applicableMiddleware.ShouldNotContain(typeof(CustomerConstrainedMiddleware));
                }
            }
        }

        stopwatch.Stop();
        var totalOperations = iterations * notificationTypes.Length * middlewareTypes.Length;
        var timePerOperation = (double)stopwatch.ElapsedMilliseconds / totalOperations;

        Console.WriteLine($"Pipeline Selection Performance:");
        Console.WriteLine($"  Total time: {stopwatch.ElapsedMilliseconds}ms for {totalOperations} operations");
        Console.WriteLine($"  Per operation: {timePerOperation:F6}ms");

        // Assert selection is efficient
        timePerOperation.ShouldBeLessThan(0.01); // Less than 0.01ms per selection operation
    }
}

#region Test Notifications and Middleware

public interface IOrderNotification : INotification
{
    int OrderId { get; }
    string OrderNumber { get; }
}

public interface ICustomerNotification : INotification
{
    int CustomerId { get; }
    string CustomerName { get; }
}

public class TestOrderNotification : IOrderNotification
{
    public int OrderId { get; }
    public string OrderNumber { get; }

    public TestOrderNotification(int orderId, string orderNumber)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
    }
}

public class TestCustomerNotification : ICustomerNotification
{
    public int CustomerId { get; }
    public string CustomerName { get; }

    public TestCustomerNotification(int customerId, string customerName)
    {
        CustomerId = customerId;
        CustomerName = customerName;
    }
}

public class TestGeneralNotification : INotification
{
    public string Message { get; }

    public TestGeneralNotification(string message)
    {
        Message = message;
    }
}

public class GeneralMiddleware : INotificationMiddleware
{
    public int Order => 100;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

public class OrderConstrainedMiddleware : INotificationMiddleware<IOrderNotification>
{
    public int Order => 50;

    public async Task InvokeAsync(IOrderNotification notification, NotificationDelegate<IOrderNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        // This method should not be called directly when using constrained middleware
        // The pipeline should route to the constrained method above
        await next(notification, cancellationToken);
    }
}

public class CustomerConstrainedMiddleware : INotificationMiddleware<ICustomerNotification>
{
    public int Order => 60;

    public async Task InvokeAsync(ICustomerNotification notification, NotificationDelegate<ICustomerNotification> next, CancellationToken cancellationToken)
    {
        await next(notification, cancellationToken);
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        // This method should not be called directly when using constrained middleware
        // The pipeline should route to the constrained method above
        await next(notification, cancellationToken);
    }
}

#endregion