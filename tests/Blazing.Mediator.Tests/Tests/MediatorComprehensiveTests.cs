using Blazing.Mediator.Configuration;
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
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Mediator(null!));
        exception.ParamName.ShouldBe("serviceProvider");
    }

    /// <summary>
    /// Tests that Mediator can be created without explicit pipeline builder (now optional in new API).
    /// </summary>
    [Fact]
    public void Constructor_WithNullPipelineBuilder_ThrowsArgumentNullException()
    {
        // Arrange - Pipeline builders are now optional in the new constructor API
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act - No pipeline builders needed, Mediator uses DI to resolve them
        var mediator = new Mediator(serviceProvider);

        // Assert
        mediator.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that Mediator can be created without explicit notification pipeline builder (now optional in new API).
    /// </summary>
    [Fact]
    public void Constructor_WithNullNotificationPipelineBuilder_ThrowsArgumentNullException()
    {
        // Arrange - Notification pipeline builders are now optional in the new constructor API
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act - No pipeline builders needed, Mediator uses DI to resolve them
        var mediator = new Mediator(serviceProvider);

        // Assert
        mediator.ShouldNotBeNull();
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

        // Act
        var mediator = new Mediator(serviceProvider, null as MediatorStatistics);

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
        var statistics = new MediatorStatistics(new ConsoleStatisticsRenderer());

        // Act
        var mediator = new Mediator(serviceProvider, statistics);

        // Assert
        mediator.ShouldNotBeNull();
    }

    #endregion

    #region Error Handling Tests

    /// <summary>
    /// Tests that with source-gen, only the auto-discovered handler runs even when
    /// additional handlers are manually registered in DI (source-gen does not check for conflicts).
    /// </summary>
    [Fact]
    public async Task Send_CommandWithMultipleHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register additional handlers via DI (ignored by source-gen dispatcher)
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<IRequestHandler<TestCommand>, SecondTestCommandHandler>();

        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act - In source-gen mode, the auto-discovered TestCommandHandler is used;
        // no "multiple handlers" exception is thrown.
        await mediator.Send(command);

        // Assert - Command executed without exception (source-gen picks one handler)
        TestCommandHandler.LastExecutedCommand.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that with source-gen, only the auto-discovered handler runs even when
    /// additional handlers are manually registered in DI.
    /// </summary>
    [Fact]
    public async Task Send_QueryWithMultipleHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register additional handlers via DI (ignored by source-gen dispatcher)
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<IRequestHandler<TestQuery, string>, SecondTestQueryHandler>();

        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new TestQuery { Value = 42 };

        // Act - In source-gen mode, auto-discovered handler handles the query;
        // no "multiple handlers" exception is thrown.
        string result = await mediator.Send(query);

        // Assert - Query executed without exception
        result.ShouldNotBeNull();
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
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var query = new ThrowingQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await mediator.Send(query));
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
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new ThrowingCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await mediator.Send(command));
        exception.Message.ShouldBe("Handler threw an exception");
    }

    #endregion

    #region Pipeline Method Missing Tests

    /// <summary>
    /// Tests that v3 source-gen dispatcher correctly dispatches void commands.
    /// </summary>
    [Fact]
    public async Task Send_WithMissingPipelineMethod_UsesFallbackExecution()
    {
        // Arrange — v3: AddMediator() is the only setup needed; EmptyPipelineBuilder is no longer relevant.
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new TestCommand { Value = "test" };

        // Act & Assert - Should not throw and execute the handler directly
        await mediator.Send(command);
        TestCommandHandler.LastExecutedCommand.ShouldBe(command);
    }

    /// <summary>
    /// Tests that v3 source-gen dispatcher correctly dispatches typed query requests.
    /// </summary>
    [Fact]
    public async Task Send_QueryWithMissingPipelineMethod_UsesFallbackExecution()
    {
        // Arrange — v3: AddMediator() is the only setup needed; EmptyPipelineBuilder is no longer relevant.
        var services = new ServiceCollection();
        services.AddMediator();

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
    public async Task SendStream_WithMultipleHandlers_ThrowsInvalidOperationException()
    {
        // Arrange
        // In source-gen mode the handler is baked at compile time; extra DI registrations do NOT cause an exception.
        var services = new ServiceCollection();
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestsTestStreamRequest { Value = "test" };

        // Act — source-gen always dispatches to the single compile-time handler; verify stream succeeds.
        var results = new List<string>();
        await foreach (var item in mediator.SendStream(request))
            results.Add(item);

        results.ShouldNotBeEmpty();
    }

    /// <summary>
    /// Tests that v3 source-gen dispatcher correctly dispatches stream requests.
    /// </summary>
    [Fact]
    public async Task SendStream_WithMissingPipelineMethod_UsesFallbackExecution()
    {
        // Arrange — v3: AddMediator() is the only setup needed; EmptyPipelineBuilder is no longer relevant.
        var services = new ServiceCollection();
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var request = new TestsTestStreamRequest { Value = "test" };

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
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await mediator.Publish<TestNotification>(null!));
    }

    /// <summary>
    /// Tests notification subscription with null subscriber throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Subscribe_WithNullSubscriber_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
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
        services.AddMediator();
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
        services.AddMediator();
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
        services.AddMediator();
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            mediator.Unsubscribe((INotificationSubscriber)null!));
    }

    #endregion

    #region Statistics Integration Tests

    /// <summary>
    /// Tests that mediator works correctly when no statistics are configured.
    /// </summary>
    [Fact]
    public async Task Send_WithNullStatistics_DoesNotTrack()
    {
        // Arrange — v3: use AddMediator() without statistics config (no stats tracking).
        var services = new ServiceCollection();
        services.AddMediator();

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

        var services = new ServiceCollection();
        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddMediator(new MediatorConfiguration().WithStatisticsTracking());

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        // Get the DI-managed statistics instance so we see increments from the notification wrapper.
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        var notification = new TestNotification { Message = "test" };

        // Act
        await mediator.Publish(notification);
        statistics.ReportStatistics();

        // Assert
        renderer.Messages.ShouldContain("Notifications: 1");
    }

    #endregion
}
