namespace Blazing.Mediator.Statistics;

/// <summary>
/// Extension methods for rendering MediatorStatistics analysis results.
/// Provides enhanced display capabilities for notification, query, and command analysis.
/// </summary>
public static class MediatorStatisticsRenderer
{
    /// <summary>
    /// Renders notification analysis results to the console with enhanced pattern-aware formatting.
    /// </summary>
    /// <param name="statistics">The MediatorStatistics instance.</param>
    /// <param name="serviceProvider">Service provider for analysis.</param>
    /// <param name="isDetailed">Whether to show detailed or compact view.</param>
    public static void RenderNotificationAnalysis(this MediatorStatistics statistics, IServiceProvider serviceProvider, bool? isDetailed = null)
    {
        var notifications = statistics.AnalyzeNotifications(serviceProvider, isDetailed);
        var detailedMode = isDetailed ?? true; // Default to detailed mode

        Console.WriteLine();
        Console.WriteLine("===============================================");
        Console.WriteLine($"NOTIFICATION ANALYSIS ({(detailedMode ? "DETAILED" : "COMPACT")} MODE)");
        Console.WriteLine("===============================================");

        if (notifications.Count == 0)
        {
            Console.WriteLine("No notifications found in the application.");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"* {notifications.Count} NOTIFICATION{(notifications.Count == 1 ? "" : "S")} DISCOVERED:");

        var groupedByAssembly = notifications
            .GroupBy(n => n.Assembly)
            .OrderBy(g => g.Key);

        foreach (var assemblyGroup in groupedByAssembly)
        {
            Console.WriteLine($"  * Assembly: {assemblyGroup.Key}");

            var groupedByNamespace = assemblyGroup
                .GroupBy(n => n.Namespace)
                .OrderBy(g => g.Key);

            foreach (var namespaceGroup in groupedByNamespace)
            {
                Console.WriteLine($"    * Namespace: {namespaceGroup.Key}");

                foreach (var notification in namespaceGroup.OrderBy(n => n.ClassName))
                {
                    var statusIcon = GetNotificationStatusIcon(notification);
                    var typeName = $"{notification.ClassName}{notification.TypeParameters}";
                    var interfaceInfo = notification.PrimaryInterface;

                    Console.WriteLine($"      {statusIcon} {typeName} : {interfaceInfo}");

                    if (detailedMode)
                    {
                        RenderDetailedNotificationInfo(notification);
                    }
                }
            }
        }

        Console.WriteLine();
        RenderNotificationLegend(detailedMode);
        Console.WriteLine("===============================================");
        Console.WriteLine();
    }

    /// <summary>
    /// Gets the appropriate status icon for a notification based on its pattern and handler/subscriber status.
    /// </summary>
    private static string GetNotificationStatusIcon(NotificationAnalysis notification)
    {
        return notification.Pattern switch
        {
            NotificationPattern.AutomaticHandlers => notification.HandlerStatus switch
            {
                HandlerStatus.Single => "+",
                HandlerStatus.Multiple => "#",
                HandlerStatus.Missing => "!",
                _ => "?"
            },
            NotificationPattern.ManualSubscribers => notification.SubscriberStatus switch
            {
                SubscriberStatus.Present => "@",
                SubscriberStatus.None => "~",
                SubscriberStatus.Unknown => "?",
                _ => "?"
            },
            NotificationPattern.Hybrid => (notification.HandlerStatus != HandlerStatus.Missing || 
                                          notification.SubscriberStatus == SubscriberStatus.Present) ? "*" : "!",
            NotificationPattern.None => "!",
            _ => "?"
        };
    }

    /// <summary>
    /// Renders detailed information for a notification including handlers, subscribers, and pattern info.
    /// </summary>
    private static void RenderDetailedNotificationInfo(NotificationAnalysis notification)
    {
        Console.WriteLine($"        | Type:           {notification.Type.FullName}");
        Console.WriteLine($"        | Pattern:        {GetPatternDisplayName(notification.Pattern)}");
        Console.WriteLine($"        | Assembly:       {notification.Assembly}");
        Console.WriteLine($"        | Namespace:      {notification.Namespace}");

        // Handler information
        if (notification.HandlerStatus != HandlerStatus.Missing)
        {
            Console.WriteLine($"        | Handlers:       {notification.HandlerCount} registered ({notification.HandlerDetails})");
        }
        else
        {
            Console.WriteLine($"        | Handlers:       {notification.HandlerDetails}");
        }

        // Subscriber information
        if (notification.SubscriberStatus == SubscriberStatus.Present)
        {
            Console.WriteLine($"        | Subscribers:    {notification.ActiveSubscriberCount} active ({notification.SubscriberDetails})");
            if (notification.SubscriberTypes.Any())
            {
                Console.WriteLine($"        | Subscriber Types: {string.Join(", ", notification.SubscriberTypes)}");
            }
        }
        else
        {
            Console.WriteLine($"        | Subscribers:    {notification.SubscriberDetails}");
        }

        // Broadcasting capability
        var broadcastCapability = notification.SupportsBroadcast ? "YES" : "NO";
        var broadcastReason = notification.Pattern switch
        {
            NotificationPattern.AutomaticHandlers => notification.HandlerCount > 1 ? "multiple handlers" : "single handler",
            NotificationPattern.ManualSubscribers => notification.ActiveSubscriberCount > 1 ? "multiple subscribers" : "single/no subscribers",
            NotificationPattern.Hybrid => (notification.HandlerCount > 1 || notification.ActiveSubscriberCount > 1) ? "multiple processors" : "single/no processors",
            NotificationPattern.None => "no processors",
            _ => "unknown"
        };

        Console.WriteLine($"        `- Broadcast:     {broadcastCapability} ({broadcastReason})");
        Console.WriteLine();
    }

    /// <summary>
    /// Gets a human-readable display name for a notification pattern.
    /// </summary>
    private static string GetPatternDisplayName(NotificationPattern pattern)
    {
        return pattern switch
        {
            NotificationPattern.AutomaticHandlers => "Automatic Handlers",
            NotificationPattern.ManualSubscribers => "Manual Subscribers",
            NotificationPattern.Hybrid => "Hybrid (Handlers + Subscribers)",
            NotificationPattern.None => "No Processors",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Renders the legend explaining notification status icons and patterns.
    /// </summary>
    private static void RenderNotificationLegend(bool isDetailed)
    {
        Console.WriteLine("NOTIFICATION PATTERN LEGEND:");
        Console.WriteLine("  + = Single handler (Automatic Handlers)");
        Console.WriteLine("  # = Multiple handlers (Automatic Handlers)");
        Console.WriteLine("  @ = Active subscribers (Manual Subscribers)");
        Console.WriteLine("  * = Hybrid pattern (both handlers and subscribers)");
        Console.WriteLine("  ! = No handlers or subscribers");
        Console.WriteLine("  ~ = No active subscribers (Manual Subscribers)");
        Console.WriteLine("  ? = Unknown or unable to determine");

        if (isDetailed)
        {
            Console.WriteLine();
            Console.WriteLine("DETAILED VIEW LEGEND:");
            Console.WriteLine("  | = Property details");
            Console.WriteLine("  `- = Broadcast capability information");
        }

        Console.WriteLine();
        Console.WriteLine("NOTIFICATION PATTERNS:");
        Console.WriteLine("  • Automatic Handlers: INotificationHandler<T> registered in DI");
        Console.WriteLine("  • Manual Subscribers: INotificationSubscriber<T> with runtime registration");
        Console.WriteLine("  • Hybrid: Both automatic handlers and manual subscribers");
        Console.WriteLine("  • Broadcast: Multiple processors can handle the same notification");
    }

    /// <summary>
    /// Renders a comprehensive analysis of all mediator types (queries, commands, notifications).
    /// </summary>
    /// <param name="statistics">The MediatorStatistics instance.</param>
    /// <param name="serviceProvider">Service provider for analysis.</param>
    /// <param name="isDetailed">Whether to show detailed or compact view.</param>
    public static void RenderComprehensiveAnalysis(this MediatorStatistics statistics, IServiceProvider serviceProvider, bool? isDetailed = null)
    {
        // Render queries
        var queries = statistics.AnalyzeQueries(serviceProvider, isDetailed);
        if (queries.Any())
        {
            Console.WriteLine($"?? QUERIES ({queries.Count} found):");
            foreach (var query in queries.Take(5)) // Show first 5
            {
                var icon = query.HandlerStatus switch
                {
                    HandlerStatus.Single => "+",
                    HandlerStatus.Multiple => "#",
                    HandlerStatus.Missing => "!",
                    _ => "?"
                };
                Console.WriteLine($"  {icon} {query.ClassName} ? {query.HandlerDetails}");
            }
            if (queries.Count > 5) Console.WriteLine($"  ... and {queries.Count - 5} more");
            Console.WriteLine();
        }

        // Render commands
        var commands = statistics.AnalyzeCommands(serviceProvider, isDetailed);
        if (commands.Any())
        {
            Console.WriteLine($"? COMMANDS ({commands.Count} found):");
            foreach (var command in commands.Take(5)) // Show first 5
            {
                var icon = command.HandlerStatus switch
                {
                    HandlerStatus.Single => "+",
                    HandlerStatus.Multiple => "#",
                    HandlerStatus.Missing => "!",
                    _ => "?"
                };
                Console.WriteLine($"  {icon} {command.ClassName} ? {command.HandlerDetails}");
            }
            if (commands.Count > 5) Console.WriteLine($"  ... and {commands.Count - 5} more");
            Console.WriteLine();
        }

        // Render notifications with enhanced pattern support
        statistics.RenderNotificationAnalysis(serviceProvider, isDetailed);
    }
}