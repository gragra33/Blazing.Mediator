namespace Example.Common.Analysis;

/// <summary>
/// Service helper that provides common analysis functionality for sample applications.
/// Intelligently displays middleware pipeline, constraint analysis, and mediator statistics
/// based on what the application is actually using.
/// </summary>
public class ExampleAnalysisService(
    IMediatorTypeCatalog catalog,
    MediatorStatistics mediatorStatistics,
    ISubscriberTracker? subscriberTracker = null)
{
    /// <summary>
    /// Displays pre-execution analysis including middleware pipeline and constraint analysis.
    /// </summary>
    public void DisplayPreExecutionAnalysis()
    {
        // Test constraint display
        ConstraintTestRunner.TestConstraintDisplay(catalog, mediatorStatistics);

        // Display registered middleware
        DisplayRegisteredMiddleware();

        // Display comprehensive mediator analysis
        var hasRequests = HasRequestsOrCommandsOrQueries();
        var hasNotifications = HasNotifications();
        
        MediatorAnalysisHelper.DisplayComprehensiveAnalysis(
            mediatorStatistics,
            catalog,
            subscriberTracker,
            showRequestAnalysis: hasRequests,
            showNotificationAnalysis: hasNotifications);
    }

    /// <summary>
    /// Displays post-execution analysis with detailed statistics.
    /// </summary>
    public void DisplayPostExecutionAnalysis()
    {
        Console.WriteLine("=== EXECUTION STATISTICS ===");
        mediatorStatistics.ReportStatistics();
        Console.WriteLine("=============================");
        Console.WriteLine();

        // Show detailed statistics analysis
        MediatorAnalysisHelper.DisplayDetailedStatisticsAnalysis(mediatorStatistics);
    }

    /// <summary>
    /// Displays the registered middleware pipeline.
    /// </summary>
    public void DisplayRegisteredMiddleware()
    {
        var middlewareAnalysis = MiddlewarePipelineAnalyzer.AnalyzeMiddleware(mediatorStatistics.AnalyzeRequestMiddleware(catalog));

        Console.WriteLine("Registered middleware:");
        foreach (var middleware in middlewareAnalysis)
        {
            // Clean up the class name to remove backticks
            var cleanClassName = middleware.ClassName;
            if (cleanClassName.Contains('`'))
            {
                var backtickIndex = cleanClassName.IndexOf('`');
                cleanClassName = cleanClassName[..backtickIndex];
            }

            // Clean up type parameters to remove backticks
            var cleanTypeParameters = middleware.TypeParameters;
            if (cleanTypeParameters.Contains('`'))
            {
                cleanTypeParameters = System.Text.RegularExpressions.Regex.Replace(
                    cleanTypeParameters, @"`\d+", "");
            }

            Console.WriteLine($"  - [{middleware.OrderDisplay}] {cleanClassName}{cleanTypeParameters}");
            if (!string.IsNullOrEmpty(middleware.GenericConstraints))
            {
                // Also clean constraints
                var cleanConstraints = middleware.GenericConstraints;
                if (cleanConstraints.Contains('`'))
                {
                    cleanConstraints = System.Text.RegularExpressions.Regex.Replace(
                        cleanConstraints, @"`\d+", "");
                }
                Console.WriteLine($"        - Constraints: {cleanConstraints}");
            }
        }

        // Show notification middleware — ensures notification-only projects display their pipeline.
        var notifMiddlewareAnalysis = MiddlewarePipelineAnalyzer.AnalyzeMiddleware(
            mediatorStatistics.AnalyzeNotificationMiddleware(catalog));
        if (notifMiddlewareAnalysis.Any())
        {
            if (!middlewareAnalysis.Any()) Console.WriteLine("Registered notification middleware:");
            foreach (var middleware in notifMiddlewareAnalysis)
            {
                var cleanClassName2 = middleware.ClassName;
                if (cleanClassName2.Contains('`'))
                    cleanClassName2 = cleanClassName2[..cleanClassName2.IndexOf('`')];
                var cleanTypeParameters2 = middleware.TypeParameters;
                if (cleanTypeParameters2.Contains('`'))
                    cleanTypeParameters2 = System.Text.RegularExpressions.Regex.Replace(cleanTypeParameters2, @"`\d+", "");

                Console.WriteLine($"  - [{middleware.OrderDisplay}] {cleanClassName2}{cleanTypeParameters2}");
                if (!string.IsNullOrEmpty(middleware.GenericConstraints))
                {
                    var cleanConstraints2 = middleware.GenericConstraints;
                    if (cleanConstraints2.Contains('`'))
                        cleanConstraints2 = System.Text.RegularExpressions.Regex.Replace(cleanConstraints2, @"`\d+", "");
                    Console.WriteLine($"        - Constraints: {cleanConstraints2}");
                }
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Checks if the application has request, command, or query handlers.
    /// </summary>
    private bool HasRequestsOrCommandsOrQueries()
    {
        try
        {
            var queries = mediatorStatistics.AnalyzeQueries(catalog, isDetailed: false);
            var commands = mediatorStatistics.AnalyzeCommands(catalog, isDetailed: false);
            return queries.Any() || commands.Any();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the application has notification handlers.
    /// </summary>
    private bool HasNotifications()
    {
        try
        {
            var notifications = mediatorStatistics.AnalyzeNotifications(catalog, isDetailed: false);
            return notifications.Any();
        }
        catch
        {
            return false;
        }
    }
}