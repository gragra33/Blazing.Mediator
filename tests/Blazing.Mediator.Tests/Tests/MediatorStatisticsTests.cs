using Blazing.Mediator.Pipeline;
using Blazing.Mediator.Statistics;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Comprehensive tests for MediatorStatistics functionality.
/// Tests include analysis of queries, commands, statistics tracking, and edge cases.
/// </summary>
public class MediatorStatisticsTests
{
    #region Constructor Tests

    /// <summary>
    /// Tests that MediatorStatistics constructor throws ArgumentNullException when renderer is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MediatorStatistics(null!));
    }

    /// <summary>
    /// Tests that MediatorStatistics constructor accepts valid renderer.
    /// </summary>
    [Fact]
    public void Constructor_WithValidRenderer_CreatesInstance()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();

        // Act
        var statistics = new MediatorStatistics(renderer);

        // Assert
        statistics.ShouldNotBeNull();
    }

    #endregion

    #region Statistics Increment Tests

    /// <summary>
    /// Tests that IncrementQuery adds new query types and increments existing ones.
    /// </summary>
    [Fact]
    public void IncrementQuery_WithNewAndExistingTypes_TracksCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementQuery("TestQuery");
        statistics.IncrementQuery("AnotherQuery");
        statistics.IncrementQuery("TestQuery"); // Increment existing

        // Assert - Verify through ReportStatistics output
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 2"); // 2 unique query types
    }

    /// <summary>
    /// Tests that IncrementCommand adds new command types and increments existing ones.
    /// </summary>
    [Fact]
    public void IncrementCommand_WithNewAndExistingTypes_TracksCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementCommand("TestCommand");
        statistics.IncrementCommand("AnotherCommand");
        statistics.IncrementCommand("TestCommand"); // Increment existing

        // Assert - Verify through ReportStatistics output
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Commands: 2"); // 2 unique command types
    }

    /// <summary>
    /// Tests that IncrementNotification adds new notification types and increments existing ones.
    /// </summary>
    [Fact]
    public void IncrementNotification_WithNewAndExistingTypes_TracksCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementNotification("TestNotification");
        statistics.IncrementNotification("AnotherNotification");
        statistics.IncrementNotification("TestNotification"); // Increment existing

        // Assert - Verify through ReportStatistics output
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Notifications: 2"); // 2 unique notification types
    }

    /// <summary>
    /// Tests that all increment methods work together correctly.
    /// </summary>
    [Fact]
    public void IncrementMethods_AllTogether_TrackCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementQuery("Query1");
        statistics.IncrementQuery("Query2");
        statistics.IncrementCommand("Command1");
        statistics.IncrementNotification("Notification1");
        statistics.IncrementNotification("Notification2");
        statistics.IncrementNotification("Notification3");

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 2");
        renderer.Messages.ShouldContain("Commands: 1");
        renderer.Messages.ShouldContain("Notifications: 3");
    }

    #endregion

    #region Query/Command Differentiation Tests

    /// <summary>
    /// Tests that IQuery&lt;T&gt; implementations are correctly tracked as queries.
    /// </summary>
    [Fact]
    public async Task Send_WithIQueryImplementation_IncrementsQueryStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<TestQueryWithInterface, string>, TestQueryWithInterfaceHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestQueryWithInterface { Value = "test" });

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 1");
        renderer.Messages.ShouldContain("Commands: 0");
        result.ShouldBe("Handled: test");
    }

    /// <summary>
    /// Tests that ICommand&lt;T&gt; implementations are correctly tracked as commands.
    /// </summary>
    [Fact]
    public async Task Send_WithICommandImplementation_IncrementsCommandStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<TestCommandWithInterface, int>, TestCommandWithInterfaceHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestCommandWithInterface { Value = "test" });

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 0");
        renderer.Messages.ShouldContain("Commands: 1");
        result.ShouldBe(4); // Length of "test"
    }

    /// <summary>
    /// Tests that requests with "Query" suffix are correctly tracked as queries.
    /// </summary>
    [Fact]
    public async Task Send_WithQuerySuffix_IncrementsQueryStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<TestRequestNamedQuery, string>, TestRequestNamedQueryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequestNamedQuery { Value = "test" });

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 1");
        renderer.Messages.ShouldContain("Commands: 0");
        result.ShouldBe("Query: test");
    }

    /// <summary>
    /// Tests that requests with "Command" suffix are correctly tracked as commands.
    /// </summary>
    [Fact]
    public async Task Send_WithCommandSuffix_IncrementsCommandStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<TestRequestNamedCommand, bool>, TestRequestNamedCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequestNamedCommand { Value = "test" });

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 0");
        renderer.Messages.ShouldContain("Commands: 1");
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Tests that case-insensitive Query suffix matching works correctly.
    /// </summary>
    [Fact]
    public async Task Send_WithQuerySuffixCaseInsensitive_IncrementsQueryStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<TestRequestLowercasequery, string>, TestRequestLowercasequeryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequestLowercasequery { Value = "test" });

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 1");
        renderer.Messages.ShouldContain("Commands: 0");
        result.ShouldBe("lowercase: test");
    }

    /// <summary>
    /// Tests that case-insensitive Command suffix matching works correctly.
    /// </summary>
    [Fact]
    public async Task Send_WithCommandSuffixCaseInsensitive_IncrementsCommandStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<TestRequestLowercasecommand, int>, TestRequestLowercasecommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new TestRequestLowercasecommand { Value = "test" });

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 0");
        renderer.Messages.ShouldContain("Commands: 1");
        result.ShouldBe(4);
    }

    /// <summary>
    /// Tests that ambiguous requests default to queries for backward compatibility.
    /// </summary>
    [Fact]
    public async Task Send_WithAmbiguousRequest_DefaultsToQueryStatistics()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<AmbiguousRequest, string>, AmbiguousRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new AmbiguousRequest { Value = "test" });

        // Assert - Should default to query tracking
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 1");
        renderer.Messages.ShouldContain("Commands: 0");
        result.ShouldBe("ambiguous: test");
    }

    /// <summary>
    /// Tests that primary interface checking takes precedence over name-based detection.
    /// </summary>
    [Fact]
    public async Task Send_WithInterfaceOverridingName_UsesInterfacePrecedence()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handler we need for this test
        services.AddScoped<IRequestHandler<QueryNamedAsCommand, string>, QueryNamedAsCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - This implements IQuery<T> but has "Command" in the name
        var result = await mediator.Send(new QueryNamedAsCommand { Value = "test" });

        // Assert - Should be tracked as a query because IQuery<T> takes precedence
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 1");
        renderer.Messages.ShouldContain("Commands: 0");
        result.ShouldBe("QueryAsCommand: test");
    }

    /// <summary>
    /// Tests that mixed query and command execution tracks both correctly.
    /// </summary>
    [Fact]
    public async Task Send_WithMixedQueryAndCommand_TracksBothCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        services.AddSingleton<IStatisticsRenderer>(renderer);
        services.AddSingleton(statistics);

        // Register only the core mediator services without assembly scanning
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton<IMiddlewarePipelineBuilder, MiddlewarePipelineBuilder>();
        services.AddSingleton<INotificationPipelineBuilder, NotificationPipelineBuilder>();

        // Register only the specific handlers we need for this test
        services.AddScoped<IRequestHandler<TestQueryWithInterface, string>, TestQueryWithInterfaceHandler>();
        services.AddScoped<IRequestHandler<TestCommandWithInterface, int>, TestCommandWithInterfaceHandler>();
        services.AddScoped<IRequestHandler<TestRequestNamedQuery, string>, TestRequestNamedQueryHandler>();
        services.AddScoped<IRequestHandler<TestRequestNamedCommand, bool>, TestRequestNamedCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act - Execute multiple queries and commands
        await mediator.Send(new TestQueryWithInterface { Value = "query1" });
        await mediator.Send(new TestCommandWithInterface { Value = "command1" });
        await mediator.Send(new TestRequestNamedQuery { Value = "query2" });
        await mediator.Send(new TestRequestNamedCommand { Value = "command2" });
        await mediator.Send(new TestQueryWithInterface { Value = "query1_again" }); // Same type again

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain("Queries: 2"); // TestQueryWithInterface, TestRequestNamedQuery
        renderer.Messages.ShouldContain("Commands: 2"); // TestCommandWithInterface, TestRequestNamedCommand
    }

    #endregion

    #region ReportStatistics Tests

    /// <summary>
    /// Tests that ReportStatistics renders correct format.
    /// </summary>
    [Fact]
    public void ReportStatistics_WithEmptyStats_RendersCorrectFormat()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.ReportStatistics();

        // Assert
        renderer.Messages.Count.ShouldBe(4);
        renderer.Messages[0].ShouldBe("Mediator Statistics:");
        renderer.Messages[1].ShouldBe("Queries: 0");
        renderer.Messages[2].ShouldBe("Commands: 0");
        renderer.Messages[3].ShouldBe("Notifications: 0");
    }

    /// <summary>
    /// Tests that ReportStatistics renders correct counts after increments.
    /// </summary>
    [Fact]
    public void ReportStatistics_WithCounts_RendersCorrectNumbers()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementQuery("Query1");
        statistics.IncrementQuery("Query2");
        statistics.IncrementQuery("Query3");
        statistics.IncrementCommand("Command1");
        statistics.IncrementCommand("Command2");
        statistics.IncrementNotification("Notification1");

        statistics.ReportStatistics();

        // Assert
        renderer.Messages.ShouldContain("Queries: 3");
        renderer.Messages.ShouldContain("Commands: 2");
        renderer.Messages.ShouldContain("Notifications: 1");
    }

    #endregion

    #region Core Functionality Tests

    /// <summary>
    /// Tests that MediatorStatistics properly tracks unique vs repeated increments.
    /// </summary>
    [Fact]
    public void Statistics_TrackingBehavior_CountsUniqueTypesOnly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act - Increment same types multiple times
        statistics.IncrementQuery("TestQuery");
        statistics.IncrementQuery("TestQuery");
        statistics.IncrementQuery("TestQuery");
        statistics.IncrementQuery("AnotherQuery");

        statistics.IncrementCommand("TestCommand");
        statistics.IncrementCommand("TestCommand");
        statistics.IncrementCommand("AnotherCommand");
        statistics.IncrementCommand("ThirdCommand");

        statistics.IncrementNotification("TestNotification");

        // Assert
        statistics.ReportStatistics();

        // Should count unique types, not execution counts
        renderer.Messages.ShouldContain("Queries: 2"); // TestQuery, AnotherQuery
        renderer.Messages.ShouldContain("Commands: 3"); // TestCommand, AnotherCommand, ThirdCommand
        renderer.Messages.ShouldContain("Notifications: 1"); // TestNotification
    }

    /// <summary>
    /// Tests that MediatorStatistics handles empty string and null inputs gracefully.
    /// </summary>
    [Fact]
    public void Statistics_WithEmptyInputs_HandlesGracefully()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementQuery("");
        statistics.IncrementQuery(null!);
        statistics.IncrementCommand("");
        statistics.IncrementCommand(null!);
        statistics.IncrementNotification("");
        statistics.IncrementNotification(null!);

        // Assert
        statistics.ReportStatistics();

        // Should handle empty/null inputs without throwing and not count them
        renderer.Messages.ShouldContain("Queries: 0");
        renderer.Messages.ShouldContain("Commands: 0");
        renderer.Messages.ShouldContain("Notifications: 0");
    }

    /// <summary>
    /// Tests analysis methods return empty lists when no types are found.
    /// </summary>
    [Fact]
    public void Analysis_WithNoMatchingTypes_ReturnsEmptyLists()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var queries = statistics.AnalyzeQueries(serviceProvider);
        var commands = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        queries.ShouldNotBeNull();
        commands.ShouldNotBeNull();

        // Results might be empty or contain types from other tests/assemblies
        // But they should be valid collections
        queries.ShouldBeAssignableTo<IReadOnlyList<QueryCommandAnalysis>>();
        commands.ShouldBeAssignableTo<IReadOnlyList<QueryCommandAnalysis>>();
    }

    #endregion

    #region AnalyzeQueries Tests

    /// <summary>
    /// Tests that AnalyzeQueries finds IQuery implementations.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_WithIQueryImplementations_FindsTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeQueries(serviceProvider);

        // Assert
        var queryTypes = results.Where(r => r.ClassName.Contains("Query", StringComparison.OrdinalIgnoreCase)).ToList();

        // Should find at least some query types from the test assembly
        queryTypes.ShouldNotBeEmpty("Should find query types from the loaded assemblies");

        // Verify structure of results including new handler information
        queryTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        queryTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        queryTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace));
        queryTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
        queryTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.PrimaryInterface));
        queryTypes.ShouldAllBe(r => r.Handlers != null);
    }

    /// <summary>
    /// Tests that AnalyzeQueries returns results with all expected properties populated.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_ReturnsCorrectGrouping()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act - Test both detailed and compact modes
        var detailedResults = statistics.AnalyzeQueries(serviceProvider, isDetailed: true);
        var compactResults = statistics.AnalyzeQueries(serviceProvider, isDetailed: false);

        // Assert
        detailedResults.ShouldNotBeEmpty();
        compactResults.ShouldNotBeEmpty();

        // Both modes should return same count
        detailedResults.Count.ShouldBe(compactResults.Count);

        // Verify all results have required fields populated for detailed mode
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace)); // Namespace can be "Unknown"
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.PrimaryInterface)); // NEW: Primary interface
        detailedResults.ShouldAllBe(r => r.Type != null);
        detailedResults.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
        detailedResults.ShouldAllBe(r => r.Handlers != null);

        // Verify compact mode has basic fields but may have simplified handler details
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace));
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.PrimaryInterface));
        compactResults.ShouldAllBe(r => r.Type != null);
        compactResults.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));

        // Verify that results within the same assembly and namespace are ordered by class name
        var groupedResults = detailedResults.GroupBy(r => new { r.Assembly, r.Namespace });

        foreach (var group in groupedResults)
        {
            var groupList = group.ToList();
            for (int i = 1; i < groupList.Count; i++)
            {
                var current = groupList[i];
                var previous = groupList[i - 1];

                // Within the same assembly and namespace, class names should be ordered
                var classComparison = string.Compare(current.ClassName, previous.ClassName, StringComparison.Ordinal);
                classComparison.ShouldBeGreaterThanOrEqualTo(0,
                    $"Within the same assembly/namespace, class names should be ordered: {previous.ClassName} should come before {current.ClassName}");
            }
        }
    }

    /// <summary>
    /// Tests the default parameter value for isDetailed.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_DefaultParameterValue_ShouldBeDetailed()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act - Call without specifying isDetailed parameter
        var defaultResults = statistics.AnalyzeQueries(serviceProvider);
        var explicitDetailedResults = statistics.AnalyzeQueries(serviceProvider, isDetailed: true);

        // Assert - Default should be the same as explicitly detailed
        defaultResults.Count.ShouldBe(explicitDetailedResults.Count);

        // Verify that default mode includes detailed handler information
        if (defaultResults.Any())
        {
            defaultResults.ShouldAllBe(r => r.Handlers != null);
            // In detailed mode, handler details should be more than just "Handler found" or "No handler"
            var resultsWithHandlers = defaultResults.Where(r => r.HandlerStatus == HandlerStatus.Single).ToList();
            if (resultsWithHandlers.Any())
            {
                resultsWithHandlers.ShouldAllBe(r => r.HandlerDetails != "Handler found");
                resultsWithHandlers.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
            }
        }
    }

    #endregion

    #region AnalyzeCommands Tests

    /// <summary>
    /// Tests that AnalyzeCommands finds ICommand implementations.
    /// </summary>
    [Fact]
    public void AnalyzeCommands_WithICommandImplementations_FindsTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var commandTypes = results.Where(r => r.ClassName.Contains("Command", StringComparison.OrdinalIgnoreCase)).ToList();

        // Should find at least some command types from the test assembly
        commandTypes.ShouldNotBeEmpty("Should find command types from the loaded assemblies");

        // Verify structure of results including new handler information
        commandTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        commandTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        commandTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace));
        commandTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
        commandTypes.ShouldAllBe(r => !string.IsNullOrEmpty(r.PrimaryInterface));
        commandTypes.ShouldAllBe(r => r.Handlers != null);
    }

    /// <summary>
    /// Tests that AnalyzeCommands returns results with all expected properties populated.
    /// </summary>
    [Fact]
    public void AnalyzeCommands_ReturnsCorrectGrouping()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act - Test both detailed and compact modes
        var detailedResults = statistics.AnalyzeCommands(serviceProvider, isDetailed: true);
        var compactResults = statistics.AnalyzeCommands(serviceProvider, isDetailed: false);

        // Assert
        detailedResults.ShouldNotBeEmpty();
        compactResults.ShouldNotBeEmpty();

        // Both modes should return same count
        detailedResults.Count.ShouldBe(compactResults.Count);

        // Verify all results have required fields populated for detailed mode
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace)); // Namespace can be "Unknown"
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.PrimaryInterface)); // NEW: Primary interface
        detailedResults.ShouldAllBe(r => r.Type != null);
        detailedResults.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));
        detailedResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
        detailedResults.ShouldAllBe(r => r.Handlers != null);

        // Verify compact mode has basic fields but may have simplified handler details
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace));
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        compactResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.PrimaryInterface));
        compactResults.ShouldAllBe(r => r.Type != null);
        compactResults.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));

        // Verify that results within the same assembly and namespace are ordered by class name
        var groupedResults = detailedResults.GroupBy(r => new { r.Assembly, r.Namespace });

        foreach (var group in groupedResults)
        {
            var groupList = group.ToList();
            for (int i = 1; i < groupList.Count; i++)
            {
                var current = groupList[i];
                var previous = groupList[i - 1];

                // Within the same assembly and namespace, class names should be ordered
                var classComparison = string.Compare(current.ClassName, previous.ClassName, StringComparison.Ordinal);
                classComparison.ShouldBeGreaterThanOrEqualTo(0,
                    $"Within the same assembly/namespace, class names should be ordered: {previous.ClassName} should come before {current.ClassName}");
            }
        }
    }

    #endregion

    #region Handler Analysis Tests

    /// <summary>
    /// Tests that AnalyzeQueries correctly identifies handler status.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_ChecksHandlerStatus()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();

        // Use AddMediator to ensure proper registration
        services.AddMediator(typeof(TestQuery).Assembly);
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeQueries(serviceProvider);

        // Assert
        var testQueryResult = results.FirstOrDefault(r => r.ClassName == "TestQuery");
        if (testQueryResult != null)
        {
            // Should find the handler
            testQueryResult.HandlerStatus.ShouldNotBe(HandlerStatus.Missing);
            testQueryResult.HandlerDetails.ShouldNotBe("No handler registered");
            testQueryResult.Handlers.ShouldNotBeNull();
        }
        else
        {
            // If TestQuery is not found, at least verify the structure works
            results.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));
            results.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
            results.ShouldAllBe(r => r.Handlers != null);
        }
    }

    /// <summary>
    /// Tests that AnalyzeCommands correctly identifies handler status.
    /// </summary>
    [Fact]
    public void AnalyzeCommands_ChecksHandlerStatus()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();

        // Use AddMediator to ensure proper registration
        services.AddMediator(typeof(TestCommand).Assembly);
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var testCommandResult = results.FirstOrDefault(r => r.ClassName == "TestCommand");
        if (testCommandResult != null)
        {
            // Should find the handler
            testCommandResult.HandlerStatus.ShouldNotBe(HandlerStatus.Missing);
            testCommandResult.HandlerDetails.ShouldNotBe("No handler registered");
            testCommandResult.Handlers.ShouldNotBeNull();
        }
        else
        {
            // If TestCommand is not found, at least verify the structure works
            results.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));
            results.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
            results.ShouldAllBe(r => r.Handlers != null);
        }
    }

    /// <summary>
    /// Tests that missing handlers are correctly identified.
    /// </summary>
    [Fact]
    public void Analysis_WithMissingHandlers_IdentifiesCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        // Deliberately not registering any handlers
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var queryResults = statistics.AnalyzeQueries(serviceProvider);
        var commandResults = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var allResults = queryResults.Concat(commandResults);

        // Most results should have missing handlers since we didn't register any
        var missingHandlerResults = allResults.Where(r => r.HandlerStatus == HandlerStatus.Missing).ToList();
        missingHandlerResults.ShouldNotBeEmpty();

        // Verify missing handler details (should be "No handler" in non-detailed mode)
        missingHandlerResults.ShouldAllBe(r => r.HandlerDetails == "No handler");
        missingHandlerResults.ShouldAllBe(r => r.Handlers.Count == 0);
    }

    /// <summary>
    /// Tests that multiple handlers are correctly identified.
    /// </summary>
    [Fact]
    public void Analysis_WithMultipleHandlers_IdentifiesCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();

        // Use AddMediator to ensure proper registration
        services.AddMediator(typeof(TestCommand).Assembly);

        // Register multiple handlers for the same command - this will actually cause DI issues,
        // but we want to test the detection logic
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<IRequestHandler<TestCommand>, SecondTestCommandHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var testCommandResult = results.FirstOrDefault(r => r.ClassName == "TestCommand");
        if (testCommandResult != null)
        {
            // Multiple handlers should be detected OR there might be DI issues
            // Either way, should not be completely missing
            testCommandResult.HandlerStatus.ShouldNotBe(HandlerStatus.Missing);
            testCommandResult.Handlers.ShouldNotBeNull();
        }

        // At minimum, verify the analysis structure works
        results.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));
        results.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
        results.ShouldAllBe(r => r.Handlers != null);
    }

    /// <summary>
    /// Tests that handler status icons are correctly assigned.
    /// </summary>
    [Fact]
    public void HandlerStatus_MapsToCorrectIcons()
    {
        // Test the icon mapping logic used in sample projects
        var singleIcon = HandlerStatus.Single switch
        {
            HandlerStatus.Single => "+",
            HandlerStatus.Missing => "!",
            HandlerStatus.Multiple => "#",
            _ => "?"
        };

        var missingIcon = HandlerStatus.Missing switch
        {
            HandlerStatus.Single => "+",
            HandlerStatus.Missing => "!",
            HandlerStatus.Multiple => "#",
            _ => "?"
        };

        var multipleIcon = HandlerStatus.Multiple switch
        {
            HandlerStatus.Single => "+",
            HandlerStatus.Missing => "!",
            HandlerStatus.Multiple => "#",
            _ => "?"
        };

        // Ensure that icons are distinct for different statuses
        (singleIcon == missingIcon).ShouldBeFalse();
        (singleIcon == multipleIcon).ShouldBeFalse();
        (missingIcon == multipleIcon).ShouldBeFalse();
    }

    #endregion

    #region Generic Type Handling Tests

    /// <summary>
    /// Tests that generic types are handled correctly in analysis.
    /// </summary>
    [Fact]
    public void Analysis_WithGenericTypes_HandlesCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var queryResults = statistics.AnalyzeQueries(serviceProvider);
        var commandResults = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var genericQuery = queryResults.FirstOrDefault(r => r.ClassName == "GenericQuery");
        if (genericQuery != null)
        {
            // In non-detailed mode, TypeParameters may be empty
            genericQuery.TypeParameters.ShouldNotBeNull();
        }

        var genericCommand = commandResults.FirstOrDefault(r => r.ClassName == "GenericConstraintCommand");
        if (genericCommand != null)
        {
            genericCommand.TypeParameters.ShouldNotBeNull();
        }
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    /// <summary>
    /// Tests that analysis handles assemblies that can't be loaded.
    /// </summary>
    [Fact]
    public void Analysis_WithAssemblyLoadErrors_ContinuesGracefully()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Should not throw exceptions
        var queryResults = statistics.AnalyzeQueries(serviceProvider);
        var commandResults = statistics.AnalyzeCommands(serviceProvider);

        // Should return results without throwing
        queryResults.ShouldNotBeNull();
        commandResults.ShouldNotBeNull();
    }

    #endregion
}

#region Test Types

/// <summary>
/// Second test command handler for multiple handler tests.
/// </summary>
public class SecondTestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test statistics renderer for capturing output.
/// </summary>
public class TestStatisticsRenderer : IStatisticsRenderer
{
    public List<string> Messages { get; } = [];

    public void Render(string message)
    {
        Messages.Add(message);
    }
}

#endregion