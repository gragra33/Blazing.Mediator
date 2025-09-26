using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Benchmarks for measuring constraint checking performance overhead in notification middleware.
/// Tests the impact of type constraint validation on pipeline execution performance.
/// </summary>
[Config(typeof(Config))]
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class ConstraintCheckingPerformanceBenchmarks
{
    private const int NotificationCount = 1000;

    private IServiceProvider? _serviceProvider;
    private NotificationPipelineBuilder? _pipelineBuilder;
    private NotificationPipelineBuilder? _pipelineBuilderWithConstraints;
    private INotification[]? _notifications;

    // Test notifications for constraint checking
    private OrderNotification? _orderNotification;
    private CustomerNotification? _customerNotification;
    private InventoryNotification? _inventoryNotification;
    private GeneralNotification? _generalNotification;

    public class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
        }
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Register middleware without constraints
        services.AddScoped<GeneralLoggingMiddleware>();
        services.AddScoped<GeneralAuditMiddleware>();
        services.AddScoped<GeneralValidationMiddleware>();

        // Register middleware with constraints
        services.AddScoped<OrderMiddleware>();
        services.AddScoped<CustomerMiddleware>();
        services.AddScoped<InventoryMiddleware>();
        services.AddScoped<AuditMiddleware>();

        _serviceProvider = services.BuildServiceProvider();

        // Setup pipeline without constraints
        _pipelineBuilder = new NotificationPipelineBuilder();
        _pipelineBuilder.AddMiddleware<GeneralLoggingMiddleware>();
        _pipelineBuilder.AddMiddleware<GeneralAuditMiddleware>();
        _pipelineBuilder.AddMiddleware<GeneralValidationMiddleware>();

        // Setup pipeline with constraints
        _pipelineBuilderWithConstraints = new NotificationPipelineBuilder();
        _pipelineBuilderWithConstraints.AddMiddleware<OrderMiddleware>();
        _pipelineBuilderWithConstraints.AddMiddleware<CustomerMiddleware>();
        _pipelineBuilderWithConstraints.AddMiddleware<InventoryMiddleware>();
        _pipelineBuilderWithConstraints.AddMiddleware<AuditMiddleware>();

        // Create test notifications
        _orderNotification = new OrderNotification(1, "ORD-001", 100.0m);
        _customerNotification = new CustomerNotification(1, "John Doe", "john@example.com");
        _inventoryNotification = new InventoryNotification("PROD-001", 50, 45);
        _generalNotification = new GeneralNotification("Test message");

        // Create diverse notification array for bulk testing
        _notifications = new INotification[NotificationCount];
        for (int i = 0; i < NotificationCount; i++)
        {
            _notifications[i] = (i % 4) switch
            {
                0 => new OrderNotification(i, $"ORD-{i:000}", i * 10.0m),
                1 => new CustomerNotification(i, $"Customer {i}", $"customer{i}@example.com"),
                2 => new InventoryNotification($"PROD-{i:000}", i, i - 5),
                _ => new GeneralNotification($"Message {i}")
            };
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    #region Single Notification Benchmarks

    [Benchmark(Baseline = true)]
    public async Task WithoutConstraints_OrderNotification()
    {
        await _pipelineBuilder!.ExecutePipeline(
            _orderNotification!,
            _serviceProvider!,
            async (notification, cancellationToken) => await Task.CompletedTask,
            CancellationToken.None);
    }

    [Benchmark]
    public async Task WithConstraints_OrderNotification()
    {
        await _pipelineBuilderWithConstraints!.ExecutePipeline(
            _orderNotification!,
            _serviceProvider!,
            async (notification, cancellationToken) => await Task.CompletedTask,
            CancellationToken.None);
    }

    [Benchmark]
    public async Task WithoutConstraints_CustomerNotification()
    {
        await _pipelineBuilder!.ExecutePipeline(
            _customerNotification!,
            _serviceProvider!,
            async (notification, cancellationToken) => await Task.CompletedTask,
            CancellationToken.None);
    }

    [Benchmark]
    public async Task WithConstraints_CustomerNotification()
    {
        await _pipelineBuilderWithConstraints!.ExecutePipeline(
            _customerNotification!,
            _serviceProvider!,
            async (notification, cancellationToken) => await Task.CompletedTask,
            CancellationToken.None);
    }

    [Benchmark]
    public async Task WithoutConstraints_GeneralNotification()
    {
        await _pipelineBuilder!.ExecutePipeline(
            _generalNotification!,
            _serviceProvider!,
            async (notification, cancellationToken) => await Task.CompletedTask,
            CancellationToken.None);
    }

    [Benchmark]
    public async Task WithConstraints_GeneralNotification()
    {
        await _pipelineBuilderWithConstraints!.ExecutePipeline(
            _generalNotification!,
            _serviceProvider!,
            async (notification, cancellationToken) => await Task.CompletedTask,
            CancellationToken.None);
    }

    #endregion

    #region Bulk Processing Benchmarks

    [Benchmark]
    public async Task BulkProcessing_WithoutConstraints()
    {
        foreach (var notification in _notifications!)
        {
            await _pipelineBuilder!.ExecutePipeline(
                notification,
                _serviceProvider!,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }
    }

    [Benchmark]
    public async Task BulkProcessing_WithConstraints()
    {
        foreach (var notification in _notifications!)
        {
            await _pipelineBuilderWithConstraints!.ExecutePipeline(
                notification,
                _serviceProvider!,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }
    }

    #endregion

    #region Constraint Checking Overhead Benchmarks

    [Benchmark]
    public void ConstraintChecking_TypeAssignability()
    {
        Type notificationType = typeof(OrderNotification);
        Type constraintType = typeof(IOrderNotification);
        
        for (int i = 0; i < 10000; i++)
        {
            _ = constraintType.IsAssignableFrom(notificationType);
        }
    }

    [Benchmark]
    public void ConstraintChecking_InterfaceDiscovery()
    {
        Type middlewareType = typeof(OrderMiddleware);
        
        for (int i = 0; i < 10000; i++)
        {
            _ = middlewareType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                .ToArray();
        }
    }

    [Benchmark]
    public void ConstraintChecking_GenericTypeCreation()
    {
        Type genericType = typeof(INotificationMiddleware<>);
        Type notificationType = typeof(OrderNotification);
        
        for (int i = 0; i < 10000; i++)
        {
            _ = genericType.MakeGenericType(notificationType);
        }
    }

    #endregion

    #region Memory Usage Benchmarks

    [Benchmark]
    public async Task MemoryUsage_WithoutConstraints()
    {
        long initialMemory = GC.GetTotalMemory(true);
        
        for (int i = 0; i < 100; i++)
        {
            await _pipelineBuilder!.ExecutePipeline(
                _orderNotification!,
                _serviceProvider!,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        long finalMemory = GC.GetTotalMemory(false);
        _ = finalMemory - initialMemory; // Memory delta
    }

    [Benchmark]
    public async Task MemoryUsage_WithConstraints()
    {
        long initialMemory = GC.GetTotalMemory(true);
        
        for (int i = 0; i < 100; i++)
        {
            await _pipelineBuilderWithConstraints!.ExecutePipeline(
                _orderNotification!,
                _serviceProvider!,
                async (n, ct) => await Task.CompletedTask,
                CancellationToken.None);
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        long finalMemory = GC.GetTotalMemory(false);
        _ = finalMemory - initialMemory; // Memory delta
    }

    #endregion

    #region Detailed Constraint Overhead Analysis

    [Benchmark]
    public void DetailedConstraintAnalysis_SmallPipeline()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate small pipeline with 3 middleware
        Type[] middlewareTypes = [typeof(OrderMiddleware), typeof(CustomerMiddleware), typeof(AuditMiddleware)];
        Type notificationType = typeof(OrderNotification);
        
        for (int i = 0; i < 1000; i++)
        {
            foreach (Type middlewareType in middlewareTypes)
            {
                // Constraint checking logic
                var constrainedInterfaces = middlewareType.GetInterfaces()
                    .Where(iface => iface.IsGenericType && 
                                   iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                    .ToArray();
                
                if (constrainedInterfaces.Length > 0)
                {
                    foreach (var constrainedInterface in constrainedInterfaces)
                    {
                        var constraintType = constrainedInterface.GetGenericArguments()[0];
                        _ = constraintType.IsAssignableFrom(notificationType);
                    }
                }
            }
        }
        
        stopwatch.Stop();
    }

    [Benchmark]
    public void DetailedConstraintAnalysis_LargePipeline()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate large pipeline with 10 middleware
        Type[] middlewareTypes = [
            typeof(OrderMiddleware), typeof(CustomerMiddleware), typeof(InventoryMiddleware),
            typeof(AuditMiddleware), typeof(GeneralLoggingMiddleware), typeof(GeneralAuditMiddleware),
            typeof(GeneralValidationMiddleware), typeof(OrderMiddleware), typeof(CustomerMiddleware),
            typeof(InventoryMiddleware)
        ];
        Type notificationType = typeof(OrderNotification);
        
        for (int i = 0; i < 1000; i++)
        {
            foreach (Type middlewareType in middlewareTypes)
            {
                // Constraint checking logic
                var constrainedInterfaces = middlewareType.GetInterfaces()
                    .Where(iface => iface.IsGenericType && 
                                   iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                    .ToArray();
                
                if (constrainedInterfaces.Length > 0)
                {
                    foreach (var constrainedInterface in constrainedInterfaces)
                    {
                        var constraintType = constrainedInterface.GetGenericArguments()[0];
                        _ = constraintType.IsAssignableFrom(notificationType);
                    }
                }
            }
        }
        
        stopwatch.Stop();
    }

    #endregion
}

#region Test Notification Interfaces and Types

public interface IOrderNotification : INotification
{
    int OrderId { get; }
    string OrderNumber { get; }
    decimal Amount { get; }
}

public interface ICustomerNotification : INotification
{
    int CustomerId { get; }
    string CustomerName { get; }
    string Email { get; }
}

public interface IInventoryNotification : INotification
{
    string ProductId { get; }
    int OldQuantity { get; }
    int NewQuantity { get; }
}

public class OrderNotification : IOrderNotification
{
    public int OrderId { get; }
    public string OrderNumber { get; }
    public decimal Amount { get; }

    public OrderNotification(int orderId, string orderNumber, decimal amount)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        Amount = amount;
    }
}

public class CustomerNotification : ICustomerNotification
{
    public int CustomerId { get; }
    public string CustomerName { get; }
    public string Email { get; }

    public CustomerNotification(int customerId, string customerName, string email)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        Email = email;
    }
}

public class InventoryNotification : IInventoryNotification
{
    public string ProductId { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    public InventoryNotification(string productId, int oldQuantity, int newQuantity)
    {
        ProductId = productId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }
}

public class GeneralNotification : INotification
{
    public string Message { get; }

    public GeneralNotification(string message)
    {
        Message = message;
    }
}

#endregion

#region Test Middleware

// Middleware without constraints
public class GeneralLoggingMiddleware : INotificationMiddleware
{
    public int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Simulate logging work
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }
}

public class GeneralAuditMiddleware : INotificationMiddleware
{
    public int Order => 20;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Simulate audit work
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }
}

public class GeneralValidationMiddleware : INotificationMiddleware
{
    public int Order => 30;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Simulate validation work
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }
}

// Middleware with constraints
public class OrderMiddleware : INotificationMiddleware<IOrderNotification>
{
    public int Order => 50;

    public async Task InvokeAsync(IOrderNotification notification, NotificationDelegate<IOrderNotification> next, CancellationToken cancellationToken)
    {
        // Order-specific processing
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }

    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class CustomerMiddleware : INotificationMiddleware<ICustomerNotification>
{
    public int Order => 60;

    public async Task InvokeAsync(ICustomerNotification notification, NotificationDelegate<ICustomerNotification> next, CancellationToken cancellationToken)
    {
        // Customer-specific processing
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }

    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class InventoryMiddleware : INotificationMiddleware<IInventoryNotification>
{
    public int Order => 70;

    public async Task InvokeAsync(IInventoryNotification notification, NotificationDelegate<IInventoryNotification> next, CancellationToken cancellationToken)
    {
        // Inventory-specific processing
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }

    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
    {
        throw new InvalidOperationException("Should be handled by pipeline execution logic");
    }
}

public class AuditMiddleware : INotificationMiddleware
{
    public int Order => 100;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // General audit processing
        await Task.Delay(1, cancellationToken);
        await next(notification, cancellationToken);
    }
}

#endregion