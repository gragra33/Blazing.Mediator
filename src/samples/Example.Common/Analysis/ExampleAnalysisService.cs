namespace Example.Common.Analysis;

/// <summary>
/// Service helper that provides common analysis functionality for sample applications.
/// Intelligently displays middleware pipeline, constraint analysis, and mediator statistics
/// based on what the application is actually using.
/// </summary>
public class ExampleAnalysisService(
    IMiddlewarePipelineInspector pipelineInspector,
    IServiceProvider serviceProvider,
    MediatorStatistics mediatorStatistics)
{
    /// <summary>
    /// Displays pre-execution analysis including middleware pipeline and constraint analysis.
    /// </summary>
    public void DisplayPreExecutionAnalysis()
    {
        // Test constraint display
        ConstraintTestRunner.TestConstraintDisplay(serviceProvider);

        // Display registered middleware
        DisplayRegisteredMiddleware();

        // Display comprehensive mediator analysis
        var hasRequests = HasRequestsOrCommandsOrQueries();
        var hasNotifications = HasNotifications();
        
        MediatorAnalysisHelper.DisplayComprehensiveAnalysis(
            mediatorStatistics, 
            serviceProvider, 
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
        var middlewareAnalysis = MiddlewarePipelineAnalyzer.AnalyzeMiddleware(pipelineInspector, serviceProvider);

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
        Console.WriteLine();
    }

    /// <summary>
    /// Checks if the application has request, command, or query handlers.
    /// </summary>
    private bool HasRequestsOrCommandsOrQueries()
    {
        try
        {
            var queries = mediatorStatistics.AnalyzeQueries(serviceProvider, isDetailed: false);
            var commands = mediatorStatistics.AnalyzeCommands(serviceProvider, isDetailed: false);
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
            var notifications = mediatorStatistics.AnalyzeNotifications(serviceProvider, isDetailed: false);
            return notifications.Any();
        }
        catch
        {
            return false;
        }
    }
}