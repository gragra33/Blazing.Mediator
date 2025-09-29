namespace Example.Common.Analysis;

/// <summary>
/// Comprehensive mediator analysis helper that provides detailed statistics analysis.
/// </summary>
public static class MediatorAnalysisHelper
{
    /// <summary>
    /// Displays comprehensive mediator analysis including requests, queries, commands, and notifications.
    /// </summary>
    /// <param name="mediatorStatistics">The mediator statistics instance.</param>
    /// <param name="serviceProvider">The service provider to resolve services from.</param>
    /// <param name="showRequestAnalysis">Whether to show request analysis (for general request samples).</param>
    /// <param name="showNotificationAnalysis">Whether to show notification analysis (for notification samples).</param>
    public static void DisplayComprehensiveAnalysis(
        MediatorStatistics mediatorStatistics, 
        IServiceProvider serviceProvider,
        bool showRequestAnalysis = true,
        bool showNotificationAnalysis = false)
    {
        Console.WriteLine("=== COMPREHENSIVE MEDIATOR ANALYSIS ===");
        Console.WriteLine();

        if (showRequestAnalysis)
        {
            // Show compact mode first for requests
            Console.WriteLine("REQUEST/QUERY/COMMAND ANALYSIS:");
            Console.WriteLine("=======================================");
            Console.WriteLine();
            
            Console.WriteLine("COMPACT MODE (isDetailed: false):");
            Console.WriteLine("======================================");
            var compactQueries = mediatorStatistics.AnalyzeQueries(serviceProvider, isDetailed: false);
            var compactCommands = mediatorStatistics.AnalyzeCommands(serviceProvider, isDetailed: false);

            DisplayAnalysisResults("QUERIES", compactQueries, isDetailed: false);
            DisplayAnalysisResults("COMMANDS", compactCommands, isDetailed: false);

            Console.WriteLine();
            Console.WriteLine("LEGEND: + = Handler found, ! = No handler, # = Multiple handlers");
            Console.WriteLine();

            // Show detailed mode
            Console.WriteLine("DETAILED MODE (isDetailed: true - Default):");
            Console.WriteLine("============================================");
            var detailedQueries = mediatorStatistics.AnalyzeQueries(serviceProvider, isDetailed: true);
            var detailedCommands = mediatorStatistics.AnalyzeCommands(serviceProvider, isDetailed: true);

            DisplayAnalysisResults("QUERIES", detailedQueries, isDetailed: true);
            DisplayAnalysisResults("COMMANDS", detailedCommands, isDetailed: true);

            Console.WriteLine();
            Console.WriteLine("LEGEND:");
            Console.WriteLine("  + = Handler found (Single)    ! = No handler (Missing)    # = Multiple handlers");
            Console.WriteLine("  | = Property details          ?? = Additional information");
            Console.WriteLine("===============================================");
            Console.WriteLine();
        }

        if (showNotificationAnalysis)
        {
            // Show enhanced notification analysis with pattern detection
            Console.WriteLine("NOTIFICATION ANALYSIS:");
            Console.WriteLine("======================");
            Console.WriteLine();
            
            Console.WriteLine("COMPACT MODE (isDetailed: false):");
            Console.WriteLine("======================================");
            var compactNotifications = mediatorStatistics.AnalyzeNotifications(serviceProvider, isDetailed: false);
            DisplayEnhancedNotificationAnalysisResults("NOTIFICATIONS", compactNotifications, isDetailed: false);

            Console.WriteLine();
            Console.WriteLine("LEGEND: + = Handler(s) found, ! = No handlers, @ = Subscribers active, * = Hybrid pattern, ~ = No processors");
            Console.WriteLine();

            Console.WriteLine("DETAILED MODE (isDetailed: true - Default):");
            Console.WriteLine("============================================");
            var detailedNotifications = mediatorStatistics.AnalyzeNotifications(serviceProvider, isDetailed: true);
            DisplayEnhancedNotificationAnalysisResults("NOTIFICATIONS", detailedNotifications, isDetailed: true);

            Console.WriteLine();
            Console.WriteLine("ENHANCED LEGEND:");
            Console.WriteLine("  + = Handler(s) found (Automatic)    ! = No handlers/subscribers");
            Console.WriteLine("  @ = Active subscribers (Manual)     * = Hybrid (both patterns)");
            Console.WriteLine("  # = Multiple handlers               ~ = No processors detected");
            Console.WriteLine("  | = Property details                ?? = Additional information");
            Console.WriteLine("===============================================");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays detailed statistics analysis showing all available metrics and counters.
    /// </summary>
    /// <param name="mediatorStatistics">The mediator statistics instance.</param>
    public static void DisplayDetailedStatisticsAnalysis(MediatorStatistics mediatorStatistics)
    {
        Console.WriteLine("=== DETAILED MEDIATOR STATISTICS ANALYSIS ===");
        Console.WriteLine();

        // Get performance summary
        var performanceSummary = mediatorStatistics.GetPerformanceSummary();

        // Display system-wide summary
        Console.WriteLine("SYSTEM-WIDE SUMMARY:");
        Console.WriteLine("--------------------");
        
        if (performanceSummary.HasValue)
        {
            var summary = performanceSummary.Value;
            Console.WriteLine($"|- Total Operations: {summary.TotalOperations:N0}");
            Console.WriteLine($"|- Successful: {summary.TotalOperations - summary.TotalFailures:N0}");
            Console.WriteLine($"|- Errors: {summary.TotalFailures:N0}");
            Console.WriteLine($"|- Overall Success Rate: {summary.OverallSuccessRate:F1}%");
            Console.WriteLine($"|- Average Response Time: {summary.AverageExecutionTimeMs:F2} ms");
            Console.WriteLine($"|- Total Memory Allocated: {summary.TotalMemoryAllocatedBytes:N0} bytes");
            Console.WriteLine($"`- Unique Operation Types: {summary.UniqueOperationTypes:N0}");
        }
        else
        {
            Console.WriteLine("|- Total Operations: 0");
            Console.WriteLine("|- Total Requests: 0");
            Console.WriteLine("|- Successful: 0");
            Console.WriteLine("|- Errors: 0");
            Console.WriteLine("|- Overall Success Rate: N/A");
            Console.WriteLine($"|- Average Response Time: N/A");
            Console.WriteLine($"`- Unique Request Types: 0");
        }
        
        Console.WriteLine();

        Console.WriteLine("PERFORMANCE INSIGHTS:");
        Console.WriteLine("--------------------");
        
        if (performanceSummary.HasValue)
        {
            var summary = performanceSummary.Value;
            if (summary.TotalOperations > 0)
            {
                Console.WriteLine($"|- System is processing requests");
                Console.WriteLine($"|- Average processing time: {summary.AverageExecutionTimeMs:F2} ms");
                Console.WriteLine($"`- Memory efficiency: {(summary.TotalMemoryAllocatedBytes / Math.Max(summary.TotalOperations, 1)):N0} bytes per request");
            }
            else
            {
                Console.WriteLine("`- No requests processed yet");
            }
        }
        else
        {
            Console.WriteLine("`- (Performance counters not enabled)");
        }

        Console.WriteLine();
        Console.WriteLine("===============================================");
        Console.WriteLine();
    }

    /// <summary>
    /// Helper method to display analysis results for requests/queries/commands.
    /// </summary>
    private static void DisplayAnalysisResults(string type, IReadOnlyList<QueryCommandAnalysis> results, bool isDetailed)
    {
        Console.WriteLine($"* {results.Count} {type} DISCOVERED:");

        if (results.Count == 0)
        {
            Console.WriteLine("  (None found)");
            return;
        }

        var groupedResults = results.GroupBy(r => r.Assembly)
            .OrderBy(g => g.Key);

        foreach (var assemblyGroup in groupedResults)
        {
            Console.WriteLine($"  * Assembly: {assemblyGroup.Key}");

            var namespaceGroups = assemblyGroup.GroupBy(r => r.Namespace)
                .OrderBy(g => g.Key);

            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"    * Namespace: {namespaceGroup.Key}");

                var orderedItems = namespaceGroup.OrderBy(r => r.ClassName);

                foreach (var item in orderedItems)
                {
                    var statusIcon = item.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    };

                    // Use clean type name without backticks
                    var cleanClassName = item.ClassName;
                    if (cleanClassName.Contains('`'))
                    {
                        var backtickIndex = cleanClassName.IndexOf('`');
                        cleanClassName = cleanClassName[..backtickIndex];
                    }

                    Console.WriteLine($"      {statusIcon} {cleanClassName} : {item.PrimaryInterface}");

                    if (isDetailed)
                    {
                        // Clean type name for detailed display
                        var cleanTypeName = item.Type?.FullName ?? cleanClassName;
                        if (cleanTypeName.Contains('`'))
                        {
                            var backtickIndex = cleanTypeName.IndexOf('`');
                            cleanTypeName = cleanTypeName[..backtickIndex];
                        }

                        Console.WriteLine($"        | Type:        {cleanTypeName}");
                        Console.WriteLine($"        | Returns:     {item.ResponseType}");
                        
                        // Clean handler names
                        var cleanHandlerNames = item.Handlers.Any() ? 
                            string.Join(", ", item.Handlers.Select(h => {
                                var name = h.Name.Replace("Handler", "");
                                if (name.Contains('`'))
                                {
                                    var backtickIndex = name.IndexOf('`');
                                    name = name[..backtickIndex];
                                }
                                return name;
                            })) : 
                            "None";
                        
                        Console.WriteLine($"        | Handler:     {cleanHandlerNames}");
                        Console.WriteLine($"        | Status:      {item.HandlerStatus}");
                        Console.WriteLine($"        | Assembly:    {item.Assembly}");
                        Console.WriteLine($"        | Namespace:   {item.Namespace}");
                        Console.WriteLine($"        | Handler(s):  {item.Handlers.Count} registered");
                        Console.WriteLine($"        `- Result Type: {(item.IsResultType ? "YES (IResult)" : "NO (standard type)")}");
                    }
                }
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Helper method to display enhanced analysis results for notifications with pattern detection.
    /// </summary>
    private static void DisplayEnhancedNotificationAnalysisResults(string type, IReadOnlyList<NotificationAnalysis> results, bool isDetailed)
    {
        Console.WriteLine($"* {results.Count} {type} DISCOVERED:");

        if (results.Count == 0)
        {
            Console.WriteLine("  (None found)");
            return;
        }

        var groupedResults = results.GroupBy(r => r.AssemblyName)
            .OrderBy(g => g.Key);

        foreach (var assemblyGroup in groupedResults)
        {
            Console.WriteLine($"  * Assembly: {assemblyGroup.Key}");

            var namespaceGroups = assemblyGroup.GroupBy(r => r.Namespace)
                .OrderBy(g => g.Key);

            foreach (var namespaceGroup in namespaceGroups)
            {
                Console.WriteLine($"    * Namespace: {namespaceGroup.Key}");

                var orderedItems = namespaceGroup.OrderBy(r => r.TypeName);

                foreach (var item in orderedItems)
                {
                    // Enhanced status icon based on pattern and counts
                    var statusIcon = GetEnhancedNotificationStatusIcon(item);
                    var processorInfo = GetNotificationProcessorInfo(item);

                    // Use clean type name without backticks
                    var cleanTypeName = item.TypeName;
                    if (cleanTypeName.Contains('`'))
                    {
                        var backtickIndex = cleanTypeName.IndexOf('`');
                        cleanTypeName = cleanTypeName[..backtickIndex];
                    }

                    Console.WriteLine($"      {statusIcon} {cleanTypeName} : {item.PrimaryInterface} {processorInfo}");

                    if (isDetailed)
                    {
                        // Show pattern information
                        var patternText = item.Pattern.ToString() switch
                        {
                            "AutomaticHandlers" => "Automatic Handlers",
                            "ManualSubscribers" => "Manual Subscribers", 
                            "Hybrid" => "Hybrid (Handlers + Subscribers)",
                            "None" => "No Processors",
                            _ => item.Pattern.ToString()
                        };

                        Console.WriteLine($"        | Type:         {cleanTypeName}");
                        Console.WriteLine($"        | Pattern:      {patternText}");
                        
                        // Show handler information if available
                        if (item.HandlerCount > 0)
                        {
                            // Clean handler name too
                            var cleanHandlerName = item.HandlerName;
                            if (cleanHandlerName.Contains('`'))
                            {
                                var backtickIndex = cleanHandlerName.IndexOf('`');
                                cleanHandlerName = cleanHandlerName[..backtickIndex];
                            }
                            Console.WriteLine($"        | Handlers:     {item.HandlerCount} registered ({cleanHandlerName})");
                        }
                        
                        // Show subscriber information if available
                        if (item.ActiveSubscriberCount > 0)
                        {
                            var subscriberList = item.SubscriberTypes.Any() ? 
                                string.Join(", ", item.SubscriberTypes.Select(st => {
                                    // Clean subscriber type names
                                    if (st.Contains('`'))
                                    {
                                        var backtickIndex = st.IndexOf('`');
                                        return st[..backtickIndex];
                                    }
                                    return st;
                                })) : 
                                $"{item.ActiveSubscriberCount} active";
                            Console.WriteLine($"        | Subscribers:  {item.ActiveSubscriberCount} active ({subscriberList})");
                        }
                        
                        Console.WriteLine($"        | Assembly:     {item.AssemblyName}");
                        Console.WriteLine($"        | Namespace:    {item.Namespace}");
                        
                        // Enhanced broadcast detection
                        var broadcastStatus = item.SupportsBroadcast ? 
                            "YES (multiple processors)" : 
                            "NO (single processor)";
                        Console.WriteLine($"        `- Broadcast:   {broadcastStatus}");
                    }
                }
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Gets the enhanced status icon for notifications based on pattern and processor counts.
    /// </summary>
    private static string GetEnhancedNotificationStatusIcon(NotificationAnalysis item)
    {
        return item.Pattern.ToString() switch
        {
            "AutomaticHandlers" => item.HandlerCount switch
            {
                0 => "!", // No handlers
                1 => "+", // Single handler
                _ => "#"  // Multiple handlers
            },
            "ManualSubscribers" => item.ActiveSubscriberCount > 0 ? "@" : "!",
            "Hybrid" => "*",
            "None" => "~",
            _ => "?"
        };
    }

    /// <summary>
    /// Gets processor information text for notifications.
    /// </summary>
    private static string GetNotificationProcessorInfo(NotificationAnalysis item)
    {
        return item.Pattern.ToString() switch
        {
            "AutomaticHandlers" => $"({item.HandlerCount} handler{(item.HandlerCount == 1 ? "" : "s")})",
            "ManualSubscribers" => $"({item.ActiveSubscriberCount} subscriber{(item.ActiveSubscriberCount == 1 ? "" : "s")})",
            "Hybrid" => $"({item.HandlerCount} handler{(item.HandlerCount == 1 ? "" : "s")} + {item.ActiveSubscriberCount} subscriber{(item.ActiveSubscriberCount == 1 ? "" : "s")})",
            "None" => "(0 processors)",
            _ => "(unknown pattern)"
        };
    }

    /// <summary>
    /// Helper method to display legacy analysis results for notifications (kept for backward compatibility).
    /// </summary>
    private static void DisplayNotificationAnalysisResults(string type, IReadOnlyList<NotificationAnalysis> results, bool isDetailed)
    {
        // This method is kept for backward compatibility but delegates to the enhanced version
        DisplayEnhancedNotificationAnalysisResults(type, results, isDetailed);
    }

    /// <summary>
    /// Categorizes request types for better organization.
    /// </summary>
    private static string GetRequestCategory(string requestName)
    {
        if (requestName.Contains("Query", StringComparison.OrdinalIgnoreCase))
            return "QUERY";
        else if (requestName.Contains("Command", StringComparison.OrdinalIgnoreCase))
            return "COMMAND";
        else if (requestName.Contains("Notification", StringComparison.OrdinalIgnoreCase))
            return "NOTIFICATION";
        else
            return "REQUEST";
    }

    /// <summary>
    /// Extracts a clean request type name from the full type name.
    /// </summary>
    private static string ExtractRequestTypeName(string fullTypeName)
    {
        // Extract just the class name from full type name
        var parts = fullTypeName.Split('.');
        return parts.LastOrDefault() ?? fullTypeName;
    }
}