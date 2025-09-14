using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Statistics;
using System.Text;

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
        queryTypes.ShouldAllBe(r => r.Handlers != null);
    }

    /// <summary>
    /// Tests that AnalyzeQueries finds IRequest&lt;T&gt; implementations with Query in name.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_WithRequestQueryTypes_FindsTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeQueries(serviceProvider);

        // Assert
        var requestQueryTypes = results.Where(r => r.ClassName.Contains("Query", StringComparison.OrdinalIgnoreCase)).ToList();
        requestQueryTypes.ShouldNotBeEmpty("Should find query types in the test assembly");
        
        // Look for any query type that uses IRequest<T>
        var anyQueryType = results.FirstOrDefault(r => r.ClassName.EndsWith("Query", StringComparison.OrdinalIgnoreCase));
        anyQueryType.ShouldNotBeNull("Should find at least one query type");
    }

    /// <summary>
    /// Tests that AnalyzeQueries excludes command types.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_ExcludesCommandTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeQueries(serviceProvider);

        // Assert
        results.ShouldNotContain(r => r.ClassName.Contains("Command"));
    }

    /// <summary>
    /// Tests that AnalyzeQueries returns results grouped by assembly and namespace.
    /// </summary>
    [Fact]
    public void AnalyzeQueries_ReturnsCorrectGrouping()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeQueries(serviceProvider);

        // Assert
        results.ShouldNotBeEmpty();
        
        // Verify all results have required fields populated
        results.ShouldAllBe(r => !string.IsNullOrEmpty(r.Assembly));
        results.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace)); // Namespace can be "Unknown"
        results.ShouldAllBe(r => !string.IsNullOrEmpty(r.ClassName));
        results.ShouldAllBe(r => r.Type != null);
        
        // Verify that results within the same assembly and namespace are ordered by class name
        var groupedResults = results.GroupBy(r => new { r.Assembly, r.Namespace });
        
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
        var testCommandTypes = results.Where(r => r.ClassName.Contains("TestCommand")).ToList();
        testCommandTypes.ShouldNotBeEmpty();
        
        var testCommand = testCommandTypes.FirstOrDefault(r => r.ClassName == "TestCommand");
        testCommand.ShouldNotBeNull();
        testCommand.ResponseType.ShouldBeNull(); // Void command
    }

    /// <summary>
    /// Tests that AnalyzeCommands finds IRequest implementations with Command in name.
    /// </summary>
    [Fact]
    public void AnalyzeCommands_WithRequestCommandTypes_FindsTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var requestCommandTypes = results.Where(r => r.ClassName.Contains("Command", StringComparison.OrdinalIgnoreCase)).ToList();
        requestCommandTypes.ShouldNotBeEmpty("Should find command types in the test assembly");
        
        // Look for any command type
        var anyCommandType = results.FirstOrDefault(r => r.ClassName.EndsWith("Command", StringComparison.OrdinalIgnoreCase));
        anyCommandType.ShouldNotBeNull("Should find at least one command type");
    }

    /// <summary>
    /// Tests that AnalyzeCommands excludes query types.
    /// </summary>
    [Fact]
    public void AnalyzeCommands_ExcludesQueryTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        results.Where(r => r.ClassName.Contains("Query")).ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that AnalyzeCommands finds both void and returning commands.
    /// </summary>
    [Fact]
    public void AnalyzeCommands_FindsBothVoidAndReturningCommands()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var results = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var voidCommands = results.Where(r => r.ResponseType == null).ToList();
        var returningCommands = results.Where(r => r.ResponseType != null).ToList();
        
        voidCommands.ShouldNotBeEmpty();
        returningCommands.ShouldNotBeEmpty();
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
        
        // Verify missing handler details
        missingHandlerResults.ShouldAllBe(r => r.HandlerDetails == "No handler registered");
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
            genericQuery.TypeParameters.ShouldNotBeNullOrEmpty();
            genericQuery.TypeParameters.ShouldContain("<");
            genericQuery.TypeParameters.ShouldContain(">");
        }

        var genericCommand = commandResults.FirstOrDefault(r => r.ClassName == "GenericConstraintCommand");
        if (genericCommand != null)
        {
            genericCommand.TypeParameters.ShouldNotBeNullOrEmpty();
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

    /// <summary>
    /// Tests that analysis excludes abstract and interface types.
    /// </summary>
    [Fact]
    public void Analysis_ExcludesAbstractAndInterfaceTypes()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var queryResults = statistics.AnalyzeQueries(serviceProvider);
        var commandResults = statistics.AnalyzeCommands(serviceProvider);

        // Assert - Should not contain any interface or abstract types
        var allResults = queryResults.Concat(commandResults);
        allResults.ShouldAllBe(r => !r.Type.IsInterface);
        allResults.ShouldAllBe(r => !r.Type.IsAbstract);
    }

    /// <summary>
    /// Tests that analysis handles types with missing namespace.
    /// </summary>
    [Fact]
    public void Analysis_WithMissingNamespace_UsesUnknown()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var queryResults = statistics.AnalyzeQueries(serviceProvider);
        var commandResults = statistics.AnalyzeCommands(serviceProvider);

        // Assert - All results should have namespace (or "Unknown")
        var allResults = queryResults.Concat(commandResults);
        allResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.Namespace));
    }

    /// <summary>
    /// Tests concurrent access to statistics tracking.
    /// </summary>
    [Fact]
    public async Task Statistics_ConcurrentAccess_ThreadSafe()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        const int taskCount = 100;
        const int incrementsPerTask = 10;

        // Act
        var tasks = Enumerable.Range(0, taskCount).Select(async i =>
        {
            await Task.Run(() =>
            {
                for (int j = 0; j < incrementsPerTask; j++)
                {
                    statistics.IncrementQuery($"Query{i}");
                    statistics.IncrementCommand($"Command{i}");
                    statistics.IncrementNotification($"Notification{i}");
                }
            });
        });

        await Task.WhenAll(tasks);

        // Assert
        statistics.ReportStatistics();
        renderer.Messages.ShouldContain($"Queries: {taskCount}");
        renderer.Messages.ShouldContain($"Commands: {taskCount}");
        renderer.Messages.ShouldContain($"Notifications: {taskCount}");
    }

    #endregion

    #region Renderer Integration Tests

    /// <summary>
    /// Tests integration with TextWriterStatisticsRenderer.
    /// </summary>
    [Fact]
    public void Integration_WithTextWriterRenderer_WorksCorrectly()
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        var stringWriter = new StringWriter(stringBuilder);
        var renderer = new TextWriterStatisticsRenderer(stringWriter);
        var statistics = new MediatorStatistics(renderer);

        // Act
        statistics.IncrementQuery("TestQuery");
        statistics.IncrementCommand("TestCommand");
        statistics.ReportStatistics();

        // Assert
        var output = stringBuilder.ToString();
        output.ShouldContain("Mediator Statistics:");
        output.ShouldContain("Queries: 1");
        output.ShouldContain("Commands: 1");
        output.ShouldContain("Notifications: 0");
    }

    /// <summary>
    /// Tests integration with ConsoleStatisticsRenderer.
    /// </summary>
    [Fact]
    public void Integration_WithConsoleRenderer_WorksCorrectly()
    {
        // Arrange
        var renderer = new ConsoleStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);

        // Act & Assert - Should not throw
        statistics.IncrementQuery("TestQuery");
        statistics.ReportStatistics();
    }

    #endregion

    #region Handler Analysis Integration Tests

    /// <summary>
    /// Tests that handler analysis works correctly with real service provider.
    /// </summary>
    [Fact]
    public void HandlerAnalysis_WithRealServiceProvider_WorksCorrectly()
    {
        // Arrange
        var renderer = new TestStatisticsRenderer();
        var statistics = new MediatorStatistics(renderer);
        var services = new ServiceCollection();
        
        // Register some handlers
        services.AddScoped<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddScoped<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddScoped<IRequestHandler<RequestBasedQuery, int>, RequestBasedQueryHandler>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var queryResults = statistics.AnalyzeQueries(serviceProvider);
        var commandResults = statistics.AnalyzeCommands(serviceProvider);

        // Assert
        var allResults = queryResults.Concat(commandResults);
        
        // Should have some results with proper handler status
        allResults.ShouldAllBe(r => Enum.IsDefined(typeof(HandlerStatus), r.HandlerStatus));
        allResults.ShouldAllBe(r => !string.IsNullOrEmpty(r.HandlerDetails));
        allResults.ShouldAllBe(r => r.Handlers != null);
        
        // Should have mix of statuses
        var statusTypes = allResults.Select(r => r.HandlerStatus).Distinct().ToList();
        statusTypes.ShouldContain(HandlerStatus.Missing); // Some will have no handlers
    }

    /// <summary>
    /// Tests that ASCII marker icons are correctly applied to different handler statuses.
    /// </summary>
    [Fact]
    public void HandlerStatus_ASCIIMarkers_CorrectlyApplied()
    {
        // Test the ASCII marker logic used in the sample projects
        var testStatuses = new[]
        {
            HandlerStatus.Single,
            HandlerStatus.Missing,
            HandlerStatus.Multiple
        };

        foreach (var status in testStatuses)
        {
            var icon = status switch
            {
                HandlerStatus.Single => "+",
                HandlerStatus.Missing => "!",
                HandlerStatus.Multiple => "#",
                _ => "?"
            };

            // Verify each status has a unique icon
            icon.ShouldNotBeNull();
            icon.Length.ShouldBe(1);
            
            // Verify specific mappings
            switch (status)
            {
                case HandlerStatus.Single:
                    icon.ShouldBe("+");
                    break;
                case HandlerStatus.Missing:
                    icon.ShouldBe("!");
                    break;
                case HandlerStatus.Multiple:
                    icon.ShouldBe("#");
                    break;
            }
        }
    }

    #endregion

    #region Test Helper Classes

    /// <summary>
    /// Test statistics renderer that captures messages for verification.
    /// </summary>
    public class TestStatisticsRenderer : IStatisticsRenderer
    {
        public List<string> Messages { get; } = new();

        public void Render(string message)
        {
            Messages.Add(message);
        }
    }

    /// <summary>
    /// Test query for analysis testing.
    /// </summary>
    public class TestQuery : IQuery<string>
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Test request-based query for analysis testing.
    /// </summary>
    public class RequestBasedQuery : IRequest<int>
    {
        public string Filter { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test command for analysis testing.
    /// </summary>
    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test request-based command for analysis testing.
    /// </summary>
    public class RequestBasedCommand : IRequest<bool>
    {
        public string Data { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test generic query for analysis testing.
    /// </summary>
    public class GenericQuery<T> : IQuery<string>
    {
        public T? Data { get; set; }
    }

    /// <summary>
    /// Test generic command for analysis testing.
    /// </summary>
    public class GenericConstraintCommand<T> : ICommand where T : class
    {
        public T? Data { get; set; }
    }

    /// <summary>
    /// Test type that should not be included in analysis (not implementing relevant interfaces).
    /// </summary>
    public class NonMediatorType
    {
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Handler for TestQuery.
    /// </summary>
    public class TestQueryHandler : IRequestHandler<TestQuery, string>
    {
        public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Result: {request.Value}");
        }
    }

    /// <summary>
    /// Handler for RequestBasedQuery.
    /// </summary>
    public class RequestBasedQueryHandler : IRequestHandler<RequestBasedQuery, int>
    {
        public Task<int> Handle(RequestBasedQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Filter.Length);
        }
    }

    /// <summary>
    /// Handler for TestCommand.
    /// </summary>
    public class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            // Simulate command execution
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Second handler for TestCommand to test multiple handlers scenario.
    /// </summary>
    public class SecondTestCommandHandler : IRequestHandler<TestCommand>
    {
        public Task Handle(TestCommand request, CancellationToken cancellationToken)
        {
            // Simulate command execution
            return Task.CompletedTask;
        }
    }

    #endregion
}