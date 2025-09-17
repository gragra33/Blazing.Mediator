using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for NotificationPipelineBuilder covering all scenarios,
/// edge cases, and error conditions to ensure 100% coverage.
/// </summary>
public class NotificationPipelineBuilderComprehensiveTests
{
    #region Middleware Order Tests

    /// <summary>
    /// Tests that middleware without Order property gets fallback order.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithoutOrderProperty_GetsFallbackOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(1); // First unordered middleware gets order 1
    }

    /// <summary>
    /// Tests that multiple middleware without Order properties get incremental orders.
    /// </summary>
    [Fact]
    public void AddMiddleware_MultipleWithoutOrder_GetsIncrementalOrders()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>();
        builder.AddMiddleware<AnotherNotificationMiddlewareWithoutOrder>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(2);
        middleware[0].Order.ShouldBe(1); // First
        middleware[1].Order.ShouldBe(2); // Second
    }

    /// <summary>
    /// Tests that static Order property is respected.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithStaticOrderProperty_UsesStaticOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithStaticOrder>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(10); // From static Order property
    }

    /// <summary>
    /// Tests that static Order field is respected.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithStaticOrderField_UsesStaticOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithStaticOrderField>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(5); // From static Order field
    }

    /// <summary>
    /// Tests that OrderAttribute is respected.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithOrderAttribute_UsesAttributeOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithOrderAttribute>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(15); // From OrderAttribute
    }

    /// <summary>
    /// Tests that instance Order property is respected when non-default.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithInstanceOrderProperty_UsesInstanceOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithInstanceOrder>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(20); // From instance Order property
    }

    /// <summary>
    /// Tests that instance Order property with default value (0) uses fallback order.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithDefaultInstanceOrder_UsesFallbackOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithDefaultInstanceOrder>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(1); // Fallback order since instance order is 0 (default)
    }

    /// <summary>
    /// Tests order precedence: static property > static field > attribute > instance property > fallback.
    /// </summary>
    [Fact]
    public void GetMiddlewareOrder_OrderPrecedence_StaticPropertyWins()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithAllOrderTypes>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(100); // Static property should win
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Tests that middleware that can't be instantiated uses fallback order.
    /// </summary>
    [Fact]
    public void AddMiddleware_CantCreateInstance_UsesFallbackOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithNoParameterlessConstructor>();
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(1); // Fallback order when instance can't be created
    }

    #endregion

    #region Configuration Tests

    /// <summary>
    /// Tests adding middleware with configuration.
    /// </summary>
    [Fact]
    public void AddMiddleware_WithConfiguration_StoresConfiguration()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        var config = new { Setting = "test" };

        // Act
        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>(config);
        var middleware = builder.GetMiddlewareConfiguration();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Type.ShouldBe(typeof(NotificationMiddlewareWithoutOrder));
        middleware[0].Configuration.ShouldBe(config);
    }

    /// <summary>
    /// Tests that GetMiddlewareConfiguration returns correct info.
    /// </summary>
    [Fact]
    public void GetMiddlewareConfiguration_ReturnsCorrectInfo()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        var config1 = new { Setting = "test1" };
        var config2 = new { Setting = "test2" };

        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>(config1);
        builder.AddMiddleware<AnotherNotificationMiddlewareWithoutOrder>(config2);

        // Act
        var middleware = builder.GetMiddlewareConfiguration();

        // Assert
        middleware.Count.ShouldBe(2);
        middleware[0].Configuration.ShouldBe(config1);
        middleware[1].Configuration.ShouldBe(config2);
    }

    /// <summary>
    /// Tests that GetRegisteredMiddleware returns correct types.
    /// </summary>
    [Fact]
    public void GetRegisteredMiddleware_ReturnsCorrectTypes()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();

        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>();
        builder.AddMiddleware<AnotherNotificationMiddlewareWithoutOrder>();

        // Act
        var middleware = builder.GetRegisteredMiddleware();

        // Assert
        middleware.Count.ShouldBe(2);
        middleware[0].ShouldBe(typeof(NotificationMiddlewareWithoutOrder));
        middleware[1].ShouldBe(typeof(AnotherNotificationMiddlewareWithoutOrder));
    }

    #endregion

    #region GetDetailedMiddlewareInfo Tests

    /// <summary>
    /// Tests GetDetailedMiddlewareInfo without service provider returns cached orders.
    /// </summary>
    [Fact]
    public void GetDetailedMiddlewareInfo_WithoutServiceProvider_ReturnsCachedOrders()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<NotificationMiddlewareWithStaticOrder>();

        // Act
        var middleware = builder.GetDetailedMiddlewareInfo();

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(10); // Cached order from registration
    }

    /// <summary>
    /// Tests GetDetailedMiddlewareInfo with service provider gets runtime orders.
    /// </summary>
    [Fact]
    public void GetDetailedMiddlewareInfo_WithServiceProvider_GetsRuntimeOrders()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<NotificationMiddlewareWithInstanceOrder>();

        var services = new ServiceCollection();
        services.AddScoped<NotificationMiddlewareWithInstanceOrder>();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var middleware = builder.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(20); // Runtime order from instance
    }

    /// <summary>
    /// Tests GetDetailedMiddlewareInfo when service can't be resolved uses cached order.
    /// </summary>
    [Fact]
    public void GetDetailedMiddlewareInfo_ServiceNotResolved_UsesCachedOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<NotificationMiddlewareWithInstanceOrder>();

        var services = new ServiceCollection();
        // Don't register the middleware service
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var middleware = builder.GetDetailedMiddlewareInfo(serviceProvider);

        // Assert
        middleware.Count.ShouldBe(1);
        middleware[0].Order.ShouldBe(20); // Cached order since service can't be resolved
    }

    #endregion

    #region Build and Execute Pipeline Tests

    /// <summary>
    /// Tests building and executing notification pipeline.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_WithMiddleware_ExecutesInOrder()
    {
        // Arrange
        TestNotificationExecutionTracker.Reset();

        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestNotificationMiddleware1>();
        builder.AddMiddleware<TestNotificationMiddleware2>();

        var services = new ServiceCollection();
        services.AddScoped<TestNotificationMiddleware1>();
        services.AddScoped<TestNotificationMiddleware2>();
        var serviceProvider = services.BuildServiceProvider();

        var notification = new TestNotification { Message = "test" };

        NotificationDelegate<TestNotification> finalHandler = async (_, _) =>
        {
            TestNotificationExecutionTracker.ExecutionOrder.Add("FinalHandler");
            await Task.CompletedTask;
        };

        // Act
        await builder.ExecutePipeline(notification, serviceProvider, finalHandler, CancellationToken.None);

        // Assert
        TestNotificationExecutionTracker.ExecutionOrder.Count.ShouldBe(3);
        TestNotificationExecutionTracker.ExecutionOrder[0].ShouldBe("Middleware1");
        TestNotificationExecutionTracker.ExecutionOrder[1].ShouldBe("Middleware2");
        TestNotificationExecutionTracker.ExecutionOrder[2].ShouldBe("FinalHandler");
    }

    /// <summary>
    /// Tests building notification pipeline with Build method.
    /// </summary>
    [Fact]
    public async Task Build_WithMiddleware_ReturnsPipelineDelegate()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestNotificationMiddleware1>();

        var services = new ServiceCollection();
        services.AddScoped<TestNotificationMiddleware1>();
        var serviceProvider = services.BuildServiceProvider();

        var notification = new TestNotification { Message = "test" };
        var executed = false;

        NotificationDelegate<TestNotification> finalHandler = async (_, _) =>
        {
            executed = true;
            await Task.CompletedTask;
        };

        // Act
        var pipeline = builder.Build(serviceProvider, finalHandler);
        await pipeline(notification, CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();
    }

    /// <summary>
    /// Tests conditional middleware execution.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_WithConditionalMiddleware_ExecutesWhenShouldExecute()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<ConditionalNotificationMiddleware>();

        var services = new ServiceCollection();
        services.AddScoped<ConditionalNotificationMiddleware>();
        var serviceProvider = services.BuildServiceProvider();

        var notification = new TestNotification { Message = "shouldexecute" }; // Triggers condition
        var executed = false;

        NotificationDelegate<TestNotification> finalHandler = async (_, _) =>
        {
            executed = true;
            await Task.CompletedTask;
        };

        // Act
        await builder.ExecutePipeline(notification, serviceProvider, finalHandler, CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();
        ConditionalNotificationMiddleware.LastExecuted.ShouldBe(notification);
    }

    /// <summary>
    /// Tests conditional middleware skipping when should not execute.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_WithConditionalMiddleware_SkipsWhenShouldNotExecute()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<ConditionalNotificationMiddleware>();

        var services = new ServiceCollection();
        services.AddScoped<ConditionalNotificationMiddleware>();
        var serviceProvider = services.BuildServiceProvider();

        var notification = new TestNotification { Message = "skip" }; // Doesn't trigger condition
        var executed = false;

        NotificationDelegate<TestNotification> finalHandler = async (_, _) =>
        {
            executed = true;
            await Task.CompletedTask;
        };

        ConditionalNotificationMiddleware.LastExecuted = null; // Reset

        // Act
        await builder.ExecutePipeline(notification, serviceProvider, finalHandler, CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();
        ConditionalNotificationMiddleware.LastExecuted.ShouldBeNull(); // Should not have executed
    }

    /// <summary>
    /// Tests middleware service resolution failure throws InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task ExecutePipeline_MiddlewareNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<TestNotificationMiddleware1>();

        var services = new ServiceCollection();
        // Don't register the middleware
        var serviceProvider = services.BuildServiceProvider();

        var notification = new TestNotification { Message = "test" };

        NotificationDelegate<TestNotification> finalHandler = async (_, _) =>
        {
            await Task.CompletedTask;
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            builder.ExecutePipeline(notification, serviceProvider, finalHandler, CancellationToken.None));
    }

    #endregion

    #region AnalyzeMiddleware Tests

    /// <summary>
    /// Tests AnalyzeMiddleware returns correct analysis.
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_ReturnsCorrectAnalysis()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<NotificationMiddlewareWithStaticOrder>();
        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>();

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var analysis = builder.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.Count.ShouldBe(2);

        var staticOrderMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(NotificationMiddlewareWithStaticOrder));
        staticOrderMiddleware.ShouldNotBeNull();
        staticOrderMiddleware.Order.ShouldBe(10);
        staticOrderMiddleware.OrderDisplay.ShouldBe("10");
        staticOrderMiddleware.ClassName.ShouldBe("NotificationMiddlewareWithStaticOrder");

        var noOrderMiddleware = analysis.FirstOrDefault(a => a.Type == typeof(NotificationMiddlewareWithoutOrder));
        noOrderMiddleware.ShouldNotBeNull();
        noOrderMiddleware.Order.ShouldBe(11);
        noOrderMiddleware.OrderDisplay.ShouldBe("11");
        noOrderMiddleware.ClassName.ShouldBe("NotificationMiddlewareWithoutOrder");
    }

    /// <summary>
    /// Tests AnalyzeMiddleware with no middleware returns empty list.
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_WithNoMiddleware_ReturnsEmptyList()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var analysis = builder.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.Count.ShouldBe(0);
    }

    /// <summary>
    /// Tests AnalyzeMiddleware orders results by execution order.
    /// </summary>
    [Fact]
    public void AnalyzeMiddleware_OrdersByExecutionOrder()
    {
        // Arrange
        var builder = new NotificationPipelineBuilder();
        builder.AddMiddleware<NotificationMiddlewareWithoutOrder>(); // Order 1
        builder.AddMiddleware<NotificationMiddlewareWithStaticOrder>(); // Order 10
        builder.AddMiddleware<AnotherNotificationMiddlewareWithoutOrder>(); // Order 2

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var analysis = builder.AnalyzeMiddleware(serviceProvider);

        // Assert
        analysis.Count.ShouldBe(3);
        analysis[0].Order.ShouldBe(1); // First unordered gets order 1
        analysis[1].Order.ShouldBe(10); // Static order middleware 
        analysis[2].Order.ShouldBe(11); // Second unordered gets order 11
    }

    #endregion
}

#region Test Middleware Classes

/// <summary>
/// Test notification for testing purposes.
/// </summary>
public class TestNotification : INotification
{
    public string? Message { get; set; }
}

/// <summary>
/// Notification middleware without Order property for testing fallback order.
/// </summary>
public class NotificationMiddlewareWithoutOrder : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Another notification middleware without Order property for testing multiple middleware.
/// </summary>
public class AnotherNotificationMiddlewareWithoutOrder : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with static Order property.
/// </summary>
public class NotificationMiddlewareWithStaticOrder : INotificationMiddleware
{
    public static int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with static Order field.
/// </summary>
public class NotificationMiddlewareWithStaticOrderField : INotificationMiddleware
{
    public static int Order = 5;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with OrderAttribute.
/// </summary>
[Order(15)]
public class NotificationMiddlewareWithOrderAttribute : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with instance Order property.
/// </summary>
public class NotificationMiddlewareWithInstanceOrder : INotificationMiddleware
{
    public int Order => 20;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with default instance Order property.
/// </summary>
public class NotificationMiddlewareWithDefaultInstanceOrder : INotificationMiddleware
{
    public int Order => 0; // Default value

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with all order types to test precedence.
/// </summary>
[Order(25)]
public class NotificationMiddlewareWithAllOrderTypes : INotificationMiddleware
{
    public static int Order => 100; // Should win over field and attribute
    public static int Order2 = 50; // Field 
    public int InstanceOrder => 75; // Instance property

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Notification middleware with no parameterless constructor.
/// </summary>
public class NotificationMiddlewareWithNoParameterlessConstructor : INotificationMiddleware
{
    public NotificationMiddlewareWithNoParameterlessConstructor(string required)
    {
        // No parameterless constructor
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Test notification middleware for pipeline execution tests.
/// </summary>
public class TestNotificationMiddleware1 : INotificationMiddleware
{
    public int Order => 1;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Add to execution order tracking if it exists
        if (notification is TestNotification testNotification)
        {
            TestNotificationExecutionTracker.ExecutionOrder.Add("Middleware1");
        }
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Second test notification middleware for pipeline execution tests.
/// </summary>
public class TestNotificationMiddleware2 : INotificationMiddleware
{
    public int Order => 2;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Add to execution order tracking if it exists
        if (notification is TestNotification testNotification)
        {
            TestNotificationExecutionTracker.ExecutionOrder.Add("Middleware2");
        }
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Conditional notification middleware for testing conditional execution.
/// </summary>
public class ConditionalNotificationMiddleware : IConditionalNotificationMiddleware
{
    public static TestNotification? LastExecuted { get; set; }

    public int Order => 5;

    public bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification
    {
        if (notification is TestNotification testNotification)
        {
            return testNotification.Message?.Contains("shouldexecute") == true;
        }
        return false;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        if (notification is TestNotification testNotification)
        {
            LastExecuted = testNotification;
        }
        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Static class to track execution order in tests.
/// </summary>
public static class TestNotificationExecutionTracker
{
    public static List<string> ExecutionOrder { get; } = new List<string>();

    public static void Reset()
    {
        ExecutionOrder.Clear();
    }
}

/// <summary>
/// Simple OrderAttribute for testing.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class OrderAttribute : Attribute
{
    public int Order { get; }

    public OrderAttribute(int order)
    {
        Order = order;
    }
}

#endregion