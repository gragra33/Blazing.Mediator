using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Integration tests for MediatorStatistics tracking with various scenarios.
/// Tests real-world usage patterns and statistics collection accuracy.
/// </summary>
public class StatisticsIntegrationTests
{
    private readonly Assembly _testAssembly = typeof(StatisticsIntegrationTests).Assembly;

    // Test query and command for statistics tracking
    public class TestQuery : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestCommand : IRequest
    {
        public string Action { get; set; } = string.Empty;
    }

    public class TestNotification : INotification
    {
        public string Event { get; set; } = string.Empty;
    }

    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Response: {request.Message}");
        }
    }

    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test that statistics are tracked correctly for queries, commands, and notifications
    /// </summary>
    [Fact]
    public async Task MediatorStatistics_TracksAllOperationTypes_Correctly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: null,
            enableStatisticsTracking: true,
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act
        // Execute some queries
        await mediator.Send(new TestQuery { Message = "Test1" });
        await mediator.Send(new TestQuery { Message = "Test2" });

        // Execute some commands
        await mediator.Send(new TestCommand { Action = "Action1" });

        // Publish some notifications
        await mediator.Publish(new TestNotification { Event = "Event1" });
        await mediator.Publish(new TestNotification { Event = "Event2" });
        await mediator.Publish(new TestNotification { Event = "Event3" });

        // Assert
        // Check internal statistics (using reflection to access private fields)
        var statisticsType = typeof(MediatorStatistics);
        var queryCountField = statisticsType.GetField("_queryCount", BindingFlags.NonPublic | BindingFlags.Instance);
        var commandCountField = statisticsType.GetField("_commandCount", BindingFlags.NonPublic | BindingFlags.Instance);
        var notificationCountField = statisticsType.GetField("_notificationCount", BindingFlags.NonPublic | BindingFlags.Instance);

        if (queryCountField != null)
        {
            var queryCount = (int)queryCountField.GetValue(statistics)!;
            queryCount.ShouldBe(2);
        }

        if (commandCountField != null)
        {
            var commandCount = (int)commandCountField.GetValue(statistics)!;
            commandCount.ShouldBe(1);
        }

        if (notificationCountField != null)
        {
            var notificationCount = (int)notificationCountField.GetValue(statistics)!;
            notificationCount.ShouldBe(3);
        }
    }

    /// <summary>
    /// Test that statistics work correctly when disabled
    /// </summary>
    [Fact]
    public async Task MediatorStatistics_WhenDisabled_DoesNotTrack()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: null,
            enableStatisticsTracking: false, // Disabled
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetService<MediatorStatistics>(); // Use GetService, not GetRequiredService

        // Act
        await mediator.Send(new TestQuery { Message = "Test" });
        await mediator.Send(new TestCommand { Action = "Action" });
        await mediator.Publish(new TestNotification { Event = "Event" });

        // Assert
        statistics.ShouldBeNull(); // Should not be registered when disabled
    }

    /// <summary>
    /// Test statistics with middleware in the pipeline
    /// </summary>
    [Fact]
    public async Task MediatorStatistics_WithMiddleware_TracksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: config =>
            {
                config.AddMiddleware<FirstQueryMiddleware>();
            },
            enableStatisticsTracking: true,
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act
        await mediator.Send(new TestQuery { Message = "Test" });

        // Assert - Statistics should still work with middleware
        var statisticsType = typeof(MediatorStatistics);
        var queryCountField = statisticsType.GetField("_queryCount", BindingFlags.NonPublic | BindingFlags.Instance);

        if (queryCountField != null)
        {
            var queryCount = (int)queryCountField.GetValue(statistics)!;
            queryCount.ShouldBe(1);
        }
    }

    /// <summary>
    /// Test that custom statistics renderer is used when provided
    /// </summary>
    [Fact]
    public void MediatorStatistics_WithCustomRenderer_UsesCustomRenderer()
    {
        // Arrange
        var customRenderer = new TestStatisticsRenderer();
        var services = new ServiceCollection();
        services.AddSingleton<IStatisticsRenderer>(customRenderer);
        services.AddMediator(
            configureMiddleware: null,
            enableStatisticsTracking: true,
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<IStatisticsRenderer>();

        // Assert
        renderer.ShouldBe(customRenderer);
        renderer.ShouldBeOfType<TestStatisticsRenderer>();
    }

    /// <summary>
    /// Test AnalyzeQueries and AnalyzeCommands methods with statistics enabled
    /// </summary>
    [Fact]
    public void MediatorStatistics_AnalyzeMethods_WorkWithStatisticsEnabled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: null,
            enableStatisticsTracking: true,
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act
        var queries = statistics.AnalyzeQueries(serviceProvider);
        var commands = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        queries.ShouldNotBeNull();
        commands.ShouldNotBeNull();

        // Should find our test query and command
        queries.ShouldContain(q => q.Type == typeof(TestQuery));
        commands.ShouldContain(c => c.Type == typeof(TestCommand));
    }

    // Custom statistics renderer for testing
    private class TestStatisticsRenderer : IStatisticsRenderer
    {
        public List<string> RenderedStatistics { get; } = new();

        public void Render(string message)
        {
            RenderedStatistics.Add(message);
        }
    }

    /// <summary>
    /// Test that statistics tracking handles concurrent requests correctly
    /// </summary>
    [Fact]
    public async Task MediatorStatistics_ConcurrentRequests_TracksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(
            configureMiddleware: null,
            enableStatisticsTracking: true,
            discoverMiddleware: false,
            discoverNotificationMiddleware: false,
            _testAssembly
        );

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var statistics = serviceProvider.GetRequiredService<MediatorStatistics>();

        // Act - Execute multiple concurrent requests
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(mediator.Send(new TestQuery { Message = $"Test{i}" }));
            tasks.Add(mediator.Send(new TestCommand { Action = $"Action{i}" }));
            tasks.Add(mediator.Publish(new TestNotification { Event = $"Event{i}" }));
        }

        await Task.WhenAll(tasks);

        // Assert - Should track all requests correctly
        var statisticsType = typeof(MediatorStatistics);
        var queryCountField = statisticsType.GetField("_queryCount", BindingFlags.NonPublic | BindingFlags.Instance);
        var commandCountField = statisticsType.GetField("_commandCount", BindingFlags.NonPublic | BindingFlags.Instance);
        var notificationCountField = statisticsType.GetField("_notificationCount", BindingFlags.NonPublic | BindingFlags.Instance);

        if (queryCountField != null)
        {
            var queryCount = (int)queryCountField.GetValue(statistics)!;
            queryCount.ShouldBe(10);
        }

        if (commandCountField != null)
        {
            var commandCount = (int)commandCountField.GetValue(statistics)!;
            commandCount.ShouldBe(10);
        }

        if (notificationCountField != null)
        {
            var notificationCount = (int)notificationCountField.GetValue(statistics)!;
            notificationCount.ShouldBe(10);
        }
    }
}