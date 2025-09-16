using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests.NotificationTests;

public class NotificationMiddlewareTests
{
    // Test notification
    public class TestNotification : INotification
    {
        public string Message { get; set; } = string.Empty;
    }

    // Test middleware
    public class LoggingNotificationMiddleware : INotificationMiddleware
    {
        public int Order => 10;
        public List<string> LoggedMessages { get; } = new();

        public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
        {
            LoggedMessages.Add($"Before: {typeof(TNotification).Name}");
            await next(notification, cancellationToken);
            LoggedMessages.Add($"After: {typeof(TNotification).Name}");
        }
    }

    // Test conditional middleware
    public class ConditionalNotificationMiddleware : IConditionalNotificationMiddleware
    {
        public int Order => 5;
        public List<string> ProcessedMessages { get; } = new();

        public bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification
        {
            return notification is TestNotification testNotification && testNotification.Message.StartsWith("IMPORTANT");
        }

        public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) where TNotification : INotification
        {
            ProcessedMessages.Add($"Processing: {typeof(TNotification).Name}");
            await next(notification, cancellationToken);
        }
    }

    // Test subscriber
    public class TestSubscriber : INotificationSubscriber<TestNotification>
    {
        public List<TestNotification> ReceivedNotifications { get; } = new();

        public Task OnNotification(TestNotification notification, CancellationToken cancellationToken = default)
        {
            ReceivedNotifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Notification_Should_Pass_Through_Middleware()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggingMiddleware = new LoggingNotificationMiddleware();

        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
        }, Array.Empty<Assembly>());

        services.AddSingleton(loggingMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe(subscriber);

        var notification = new TestNotification { Message = "Hello" };

        // Act
        await mediator.Publish(notification);

        // Assert
        subscriber.ReceivedNotifications.Count.ShouldBe(1);
        loggingMiddleware.LoggedMessages.Count.ShouldBe(2);
        loggingMiddleware.LoggedMessages[0].ShouldBe("Before: TestNotification");
        loggingMiddleware.LoggedMessages[1].ShouldBe("After: TestNotification");
    }

    [Fact]
    public async Task Conditional_Middleware_Should_Only_Execute_When_Condition_Met()
    {
        // Arrange
        var services = new ServiceCollection();
        var conditionalMiddleware = new ConditionalNotificationMiddleware();

        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, Array.Empty<Assembly>());

        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe(subscriber);

        // Act & Assert - should NOT execute for regular message
        var normalNotification = new TestNotification { Message = "Hello" };
        await mediator.Publish(normalNotification);

        conditionalMiddleware.ProcessedMessages.Count.ShouldBe(0);
        subscriber.ReceivedNotifications.Count.ShouldBe(1);

        // Act & Assert - SHOULD execute for important message
        var importantNotification = new TestNotification { Message = "IMPORTANT: System down" };
        await mediator.Publish(importantNotification);

        conditionalMiddleware.ProcessedMessages.Count.ShouldBe(1);
        conditionalMiddleware.ProcessedMessages[0].ShouldBe("Processing: TestNotification");
        subscriber.ReceivedNotifications.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Middleware_Should_Execute_In_Order()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggingMiddleware = new LoggingNotificationMiddleware();
        var conditionalMiddleware = new ConditionalNotificationMiddleware();

        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>(); // Order 5
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>(); // Order 10
        }, Array.Empty<Assembly>());

        services.AddSingleton(loggingMiddleware);
        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var subscriber = new TestSubscriber();
        mediator.Subscribe(subscriber);

        // Act
        var notification = new TestNotification { Message = "IMPORTANT: Test" };
        await mediator.Publish(notification);

        // Assert
        // Conditional middleware (Order 5) should execute first, then logging middleware (Order 10)
        conditionalMiddleware.ProcessedMessages.Count.ShouldBe(1);
        loggingMiddleware.LoggedMessages.Count.ShouldBe(2);

        // The logging middleware should log around the conditional middleware
        loggingMiddleware.LoggedMessages[0].ShouldBe("Before: TestNotification");
        loggingMiddleware.LoggedMessages[1].ShouldBe("After: TestNotification");
    }

    [Fact]
    public void NotificationPipelineInspector_ShouldProvideMiddlewareInformation()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggingMiddleware = new LoggingNotificationMiddleware();
        var conditionalMiddleware = new ConditionalNotificationMiddleware();

        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, Array.Empty<Assembly>());

        services.AddSingleton(loggingMiddleware);
        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Get the inspector from the mediator using reflection
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        notificationPipelineBuilderField.ShouldNotBeNull();

        var pipelineBuilder = notificationPipelineBuilderField.GetValue(mediator);
        pipelineBuilder.ShouldNotBeNull();
        pipelineBuilder.ShouldBeAssignableTo<INotificationMiddlewarePipelineInspector>();

        var inspector = (INotificationMiddlewarePipelineInspector)pipelineBuilder;

        // Act & Assert - GetRegisteredMiddleware
        var registeredMiddleware = inspector.GetRegisteredMiddleware();
        registeredMiddleware.Count.ShouldBe(2);
        registeredMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));
        registeredMiddleware.ShouldContain(typeof(ConditionalNotificationMiddleware));

        // Act & Assert - GetMiddlewareConfiguration
        var configurations = inspector.GetMiddlewareConfiguration();
        configurations.Count.ShouldBe(2);
        configurations.First(config => config.Type == typeof(LoggingNotificationMiddleware)).Configuration.ShouldBeNull(); // No configuration provided
        configurations.First(config => config.Type == typeof(ConditionalNotificationMiddleware)).Configuration.ShouldBeNull(); // No configuration provided

        // Act & Assert - GetDetailedMiddlewareInfo without service provider
        var detailedInfo = inspector.GetDetailedMiddlewareInfo();
        detailedInfo.Count.ShouldBe(2);

        var loggingInfo = detailedInfo.First(info => info.Type == typeof(LoggingNotificationMiddleware));
        loggingInfo.Order.ShouldBe(10); // From the LoggingNotificationMiddleware.Order property
        loggingInfo.Configuration.ShouldBeNull();

        var conditionalInfo = detailedInfo.First(info => info.Type == typeof(ConditionalNotificationMiddleware));
        conditionalInfo.Order.ShouldBe(5); // From the ConditionalNotificationMiddleware.Order property
        conditionalInfo.Configuration.ShouldBeNull();

        // Act & Assert - GetDetailedMiddlewareInfo with service provider
        var detailedInfoWithProvider = inspector.GetDetailedMiddlewareInfo(serviceProvider);
        detailedInfoWithProvider.Count.ShouldBe(2);

        // Should get runtime order values from DI-registered instances
        var runtimeLoggingInfo = detailedInfoWithProvider.First(info => info.Type == typeof(LoggingNotificationMiddleware));
        runtimeLoggingInfo.Order.ShouldBe(10); // Runtime value from the actual instance
        runtimeLoggingInfo.Configuration.ShouldBeNull();

        var runtimeConditionalInfo = detailedInfoWithProvider.First(info => info.Type == typeof(ConditionalNotificationMiddleware));
        runtimeConditionalInfo.Order.ShouldBe(5); // Runtime value from the actual instance  
        runtimeConditionalInfo.Configuration.ShouldBeNull();
    }

    [Fact]
    public void NotificationPipelineInspector_ShouldSupportConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggingMiddleware = new LoggingNotificationMiddleware();
        var conditionalMiddleware = new ConditionalNotificationMiddleware();

        var config = "test-configuration";

        services.AddMediator(mediatorConfig =>
        {
            mediatorConfig.AddNotificationMiddleware<LoggingNotificationMiddleware>(config);
            mediatorConfig.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, Array.Empty<Assembly>());

        services.AddSingleton(loggingMiddleware);
        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Get the inspector from the mediator using reflection
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        notificationPipelineBuilderField.ShouldNotBeNull();

        var pipelineBuilder = notificationPipelineBuilderField.GetValue(mediator);
        pipelineBuilder.ShouldNotBeNull();
        pipelineBuilder.ShouldBeAssignableTo<INotificationMiddlewarePipelineInspector>();

        var inspector = (INotificationMiddlewarePipelineInspector)pipelineBuilder;

        // Act & Assert - GetMiddlewareConfiguration should include the configuration
        var configurations = inspector.GetMiddlewareConfiguration();
        configurations.Count.ShouldBe(2);
        configurations.First(config => config.Type == typeof(LoggingNotificationMiddleware)).Configuration.ShouldBe(config);
        configurations.First(config => config.Type == typeof(ConditionalNotificationMiddleware)).Configuration.ShouldBeNull();

        // Act & Assert - GetDetailedMiddlewareInfo should also include the configuration
        var detailedInfo = inspector.GetDetailedMiddlewareInfo();
        var loggingInfo = detailedInfo.First(info => info.Type == typeof(LoggingNotificationMiddleware));
        loggingInfo.Configuration.ShouldBe(config);

        var conditionalInfo = detailedInfo.First(info => info.Type == typeof(ConditionalNotificationMiddleware));
        conditionalInfo.Configuration.ShouldBeNull();
    }

    /// <summary>
    /// Tests for AnalyzeMiddleware functionality with discoverNotificationMiddleware: true
    /// </summary>
    [Fact]
    public void NotificationMiddleware_DiscoverNotificationMiddleware_True_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator((Action<MediatorConfiguration>?)null, Assembly.GetExecutingAssembly());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Get the inspector
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        var pipelineBuilder = notificationPipelineBuilderField?.GetValue(mediator);
        var inspector = (INotificationMiddlewarePipelineInspector)pipelineBuilder!;

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert - AnalyzeMiddleware should return meaningful data
        analysis.ShouldNotBeNull();

        // Each analysis should have valid data
        foreach (var middlewareAnalysis in analysis)
        {
            middlewareAnalysis.ClassName.ShouldNotBeNullOrWhiteSpace();
            middlewareAnalysis.Type.ShouldNotBeNull();
            middlewareAnalysis.OrderDisplay.ShouldNotBeNullOrWhiteSpace();
        }
    }

    /// <summary>
    /// Tests for AnalyzeMiddleware functionality with discoverNotificationMiddleware: false  
    /// </summary>
    [Fact]
    public void NotificationMiddleware_DiscoverNotificationMiddleware_False_WorksCorrectly()
    {
        // Arrange
        var loggingMiddleware = new LoggingNotificationMiddleware();
        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
        }, Array.Empty<Assembly>());
        services.AddSingleton(loggingMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Get the inspector
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        var pipelineBuilder = notificationPipelineBuilderField?.GetValue(mediator);
        var inspector = (INotificationMiddlewarePipelineInspector)pipelineBuilder!;

        // Act
        var registeredMiddleware = inspector.GetRegisteredMiddleware();
        var analysis = inspector.AnalyzeMiddleware(serviceProvider);

        // Assert - Should only have manually registered middleware
        registeredMiddleware.Count.ShouldBe(1);
        registeredMiddleware.ShouldContain(typeof(LoggingNotificationMiddleware));

        // AnalyzeMiddleware should return the registered middleware data
        analysis.ShouldNotBeNull();
        analysis.Count.ShouldBe(1);

        var middlewareAnalysis = analysis[0];
        middlewareAnalysis.ClassName.ShouldBe("LoggingNotificationMiddleware");
        middlewareAnalysis.Type.ShouldBe(typeof(LoggingNotificationMiddleware));
        middlewareAnalysis.Order.ShouldBe(10);
        middlewareAnalysis.OrderDisplay.ShouldBe("10");
        middlewareAnalysis.TypeParameters.ShouldBe("");
    }

    /// <summary>
    /// Tests that AnalyzeMiddleware returns ordered results
    /// </summary>
    [Fact]
    public void NotificationMiddleware_AnalyzeMiddleware_ReturnsOrderedResults()
    {
        // Arrange
        var loggingMiddleware = new LoggingNotificationMiddleware(); // Order 10
        var conditionalMiddleware = new ConditionalNotificationMiddleware(); // Order 5

        var services = new ServiceCollection();
        services.AddMediator(config =>
        {
            // Add in reverse order to test sorting
            config.AddNotificationMiddleware<LoggingNotificationMiddleware>();
            config.AddNotificationMiddleware<ConditionalNotificationMiddleware>();
        }, Array.Empty<Assembly>());
        services.AddSingleton(loggingMiddleware);
        services.AddSingleton(conditionalMiddleware);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Get the inspector  
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
        var pipelineBuilder = notificationPipelineBuilderField?.GetValue(mediator);
        var inspector = (INotificationMiddlewarePipelineInspector)pipelineBuilder!;

        // Act
        var analysis = inspector.AnalyzeMiddleware(serviceProvider).ToList();

        // Assert - Should be sorted by order (ascending)
        analysis.Count.ShouldBe(2);

        // ConditionalNotificationMiddleware should be first (Order = 5)
        analysis[0].ClassName.ShouldBe("ConditionalNotificationMiddleware");
        analysis[0].Order.ShouldBe(5);

        // LoggingNotificationMiddleware should be second (Order = 10)  
        analysis[1].ClassName.ShouldBe("LoggingNotificationMiddleware");
        analysis[1].Order.ShouldBe(10);
    }
}
