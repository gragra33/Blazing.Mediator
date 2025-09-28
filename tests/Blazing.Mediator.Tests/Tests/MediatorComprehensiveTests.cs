using Blazing.Mediator.Pipeline;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Comprehensive tests for the Mediator class covering all constructor scenarios,
/// error conditions, and edge cases to ensure 100% coverage.
/// </summary>
public class MediatorComprehensiveTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests that Mediator constructor throws ArgumentNullException when serviceProvider is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        var notificationPipelineBuilder = new NotificationPipelineBuilder();
        var statistics = new MediatorStatistics(new ConsoleStatisticsRenderer());

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Mediator(null!, pipelineBuilder, notificationPipelineBuilder, statistics));
        exception.ParamName.ShouldBe("serviceProvider");
    }

    /// <summary>
    /// Tests that Mediator constructor throws ArgumentNullException when pipelineBuilder is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullPipelineBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var notificationPipelineBuilder = new NotificationPipelineBuilder();
        var statistics = new MediatorStatistics(new ConsoleStatisticsRenderer());

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Mediator(serviceProvider, null!, notificationPipelineBuilder, statistics));
        exception.ParamName.ShouldBe("pipelineBuilder");
    }

    /// <summary>
    /// Tests that Mediator constructor throws ArgumentNullException when notificationPipelineBuilder is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullNotificationPipelineBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        var statistics = new MediatorStatistics(new ConsoleStatisticsRenderer());

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Mediator(serviceProvider, pipelineBuilder, null!, statistics));
        exception.ParamName.ShouldBe("notificationPipelineBuilder");
    }

    /// <summary>
    /// Tests that Mediator constructor accepts null statistics (should be allowed).
    /// </summary>
    [Fact]
    public void Constructor_WithNullStatistics_CreatesInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        var notificationPipelineBuilder = new NotificationPipelineBuilder();

        // Act
        var mediator = new Mediator(serviceProvider, pipelineBuilder, notificationPipelineBuilder, null);

        // Assert
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that Mediator constructor with valid parameters creates instance.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var pipelineBuilder = new MiddlewarePipelineBuilder();
        var notificationPipelineBuilder = new NotificationPipelineBuilder();
        var statistics = new MediatorStatistics(new ConsoleStatisticsRenderer());

        // Act
        var mediator = new Mediator(serviceProvider, pipelineBuilder, notificationPipelineBuilder, statistics);

        // Assert
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Tests that multiple handlers for the same command throw InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task Send_CommandWithMultipleHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register the same handler multiple times to create conflict
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<IRequestHandler<TestCommand>, SecondTestCommandHandler>();

        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        exception.Message.ShouldContain("Multiple handlers found");
        exception.Message.ShouldContain("Only one handler per request type is allowed");
    }

    /// <summary>
    /// Tests that multiple handlers for the same query throw InvalidOperationException.
    /// </summary>
    [Fact]
    public async Task Send_QueryWithMultipleHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register the same handler multiple times to create conflict
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<IRequestHandler<TestQuery, string>, SecondTestQueryHandler>();

        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestQuery { Value = 42 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
        exception.Message.ShouldContain("Multiple handlers found");
        exception.Message.ShouldContain("Only one handler per request type is allowed");
    }

    /// <summary>
    /// Tests that handler invocation exceptions are properly unwrapped.
    /// </summary>
    [Fact]
    public async Task Send_HandlerThrowsException_UnwrapsTargetInvocationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<ThrowingQuery, string>, ThrowingQueryHandler>();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new ThrowingQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(query));
        exception.Message.ShouldBe("Query handler threw an exception");
        // Should be the original exception, not wrapped in TargetInvocationException
    }

    /// <summary>
    /// Tests that void command handler invocation exceptions are properly unwrapped.
    /// </summary>
    [Fact]
    public async Task Send_VoidCommandHandlerThrowsException_UnwrapsTargetInvocationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<ThrowingCommand>, ThrowingCommandHandler>();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new ThrowingCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(command));
        exception.Message.ShouldBe("Handler threw an exception");
    }

    #endregion

    #region Pipeline Method Missing Tests

    /// <summary>
    /// Tests behavior when pipeline ExecutePipeline method is not found (fallback scenario).
    /// </summary>
    [Fact]
    public async Task Send_WithMissingPipelineMethod_UsesFallbackExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();

        // Create a custom pipeline builder that doesn't have ExecutePipeline method
        var customPipelineBuilder = new EmptyPipelineBuilder();
        services.AddSingleton<IMiddlewarePipelineBuilder>(customPipelineBuilder);
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>(); // Add renderer for statistics
        services.AddSingleton<MediatorStatistics>(); // Add statistics service so Mediator can be constructed
        services.AddSingleton<IMediator, Mediator>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act & Assert - Should not throw and execute the handler directly
        await mediator.Send(command);
        TestCommandHandler.LastExecutedCommand.ShouldBe(command);
    }

    /// <summary>
    /// Tests behavior when pipeline ExecutePipeline method is not found for query requests.
    /// </summary>
    [Fact]
    public async Task Send_QueryWithMissingPipelineMethod_UsesFallbackExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();

        // Create a custom pipeline builder that doesn't have ExecutePipeline method
        var customPipelineBuilder = new EmptyPipelineBuilder();
        services.AddSingleton<IMiddlewarePipelineBuilder>(customPipelineBuilder);
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>(); // Add renderer for statistics
        services.AddSingleton<MediatorStatistics>(); // Add statistics service so Mediator can be constructed
        services.AddSingleton<IMediator, Mediator>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestQuery { Value = 42 };

        // Act
        var result = await mediator.Send(query);

        // Assert
        result.ShouldBe("Result: 42");
    }

    #endregion

    #region Streaming Tests

    /// <summary>
    /// Tests that multiple stream handlers throw InvalidOperationException.
    /// </summary>
    [Fact]
    public void SendStream_WithMultipleHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register multiple handlers for the same stream request
        services.AddScoped<IStreamRequestHandler<TestStreamRequest, string>, TestStreamHandler>();
        services.AddScoped<IStreamRequestHandler<TestStreamRequest, string>, SecondTestStreamHandler>();

        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest { Value = "test" };

        // Act & Assert
        var ex = Assert.Throws<AggregateException>(() =>
        {
            var stream = mediator.SendStream(request);
            // Force enumeration to trigger the exception
            var enumerator = stream.GetAsyncEnumerator();
            return enumerator.MoveNextAsync().AsTask().Result;
        });

        // Verify the inner exception is the expected type
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    /// <summary>
    /// Tests stream execution with missing pipeline method uses fallback.
    /// </summary>
    [Fact]
    public async Task SendStream_WithMissingPipelineMethod_UsesFallbackExecution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IStreamRequestHandler<TestStreamRequest, string>, TestStreamHandler>();

        var customPipelineBuilder = new EmptyPipelineBuilder();
        services.AddSingleton<IMiddlewarePipelineBuilder>(customPipelineBuilder);
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>(); // Add renderer for statistics
        services.AddSingleton<MediatorStatistics>(); // Add statistics service so Mediator can be constructed
        services.AddSingleton<IMediator, Mediator>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestStreamRequest { Value = "test" };

        // Act
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Count.ShouldBe(3);
        results[0].ShouldBe("test-1");
        results[1].ShouldBe("test-2");
        results[2].ShouldBe("test-3");
    }

    #endregion

    #region Notification Tests

    /// <summary>
    /// Tests that publishing notification with null throws ArgumentNullException.
    /// </summary>
    [Fact]
    public async Task Publish_WithNullNotification_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Publish<TestNotification>(null!));
    }

    /// <summary>
    /// Tests notification subscription with null subscriber throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Subscribe_WithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            mediator.Subscribe<TestNotification>(null!));
    }

    /// <summary>
    /// Tests generic notification subscription with null subscriber throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Subscribe_GenericWithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            mediator.Subscribe((INotificationSubscriber)null!));
    }

    /// <summary>
    /// Tests notification unsubscription with null subscriber throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Unsubscribe_WithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            mediator.Unsubscribe<TestNotification>(null!));
    }

    /// <summary>
    /// Tests generic notification unsubscription with null subscriber throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Unsubscribe_GenericWithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(Array.Empty<Assembly>());
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            mediator.Unsubscribe((INotificationSubscriber)null!));
    }

    #endregion

    #region Statistics Integration Tests

    /// <summary>
    /// Tests that mediator with null statistics doesn't track execution.
    /// </summary>
    [Fact]
    public async Task Send_WithNullStatistics_DoesNotTrack()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();

        // Register mediator with null statistics
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();
        services.AddSingleton<IMediator>(serviceProvider =>
            new Mediator(serviceProvider,
                serviceProvider.GetRequiredService<IMiddlewarePipelineBuilder>(),
                serviceProvider.GetRequiredService<INotificationPipelineBuilder>(),
                null)); // Null statistics

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestQuery { Value = 42 };

        // Act & Assert - Should not throw
        var result = await mediator.Send(query);
        result.ShouldBe("Result: 42");
    }

    /// <summary>
    /// Tests that statistics tracking works correctly for notifications.
    /// </summary>
    [Fact]
    public async Task Publish_WithStatistics_TracksNotifications()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        var services = new ServiceCollection();
        services.AddSingleton(statistics);
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking();
        }, Array.Empty<Assembly>());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var notification = new TestNotification { Message = "test" };

        // Act
        await mediator.Publish(notification);
        statistics.ReportStatistics();

        // Assert
        renderer.Messages.ShouldContain("Notifications: 1");
    }

    #endregion
}
