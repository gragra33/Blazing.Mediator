using System.Diagnostics;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Collects and reports statistics about mediator usage, including query and command analysis.
/// </summary>
public sealed class MediatorStatistics : IDisposable
{
    private readonly ConcurrentDictionary<string, long> _queryCounts = new();
    private readonly ConcurrentDictionary<string, long> _commandCounts = new();
    private readonly ConcurrentDictionary<string, long> _notificationCounts = new();
    private readonly IStatisticsRenderer _renderer;
    private readonly StatisticsOptions _options;
    private readonly ILogger<MediatorStatistics>? _logger;

    // Performance counters (only used when EnablePerformanceCounters is true)
    private readonly ConcurrentDictionary<string, List<long>> _executionTimes = new();
    private readonly ConcurrentDictionary<string, long> _totalExecutions = new();
    private readonly ConcurrentDictionary<string, long> _totalExecutionTime = new();
    private readonly ConcurrentDictionary<string, long> _failedExecutions = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastExecutionTimes = new();

    // Memory usage tracking (when performance counters enabled)
    private long _totalMemoryAllocated;
    private readonly object _memoryLock = new();

    // Middleware metrics (only used when EnableMiddlewareMetrics is true)
    private readonly ConcurrentDictionary<string, long> _middlewareExecutionCounts = new();
    private readonly ConcurrentDictionary<string, long> _middlewareExecutionTimes = new();
    private readonly ConcurrentDictionary<string, long> _middlewareFailures = new();

    // Detailed analysis data (only used when EnableDetailedAnalysis is true)
    private readonly ConcurrentDictionary<string, List<DateTime>> _executionPatterns = new();
    private readonly ConcurrentDictionary<string, long> _hourlyExecutionCounts = new();

    // Metrics retention and cleanup
    private readonly ConcurrentDictionary<string, DateTime> _metricTimestamps = new();
    private readonly Timer? _cleanupTimer;
    private readonly object _cleanupLock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the MediatorStatistics class.
    /// </summary>
    /// <param name="renderer">The renderer to use for statistics output.</param>
    /// <param name="options">The statistics tracking options. If null, uses default options.</param>
    /// <param name="logger">Optional logger for debug-level logging of analysis operations.</param>
    public MediatorStatistics(IStatisticsRenderer renderer, StatisticsOptions? options = null, ILogger<MediatorStatistics>? logger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _options = options ?? new StatisticsOptions();
        _logger = logger;

        // Initialize cleanup timer if retention period is configured
        if (_options.MetricsRetentionPeriod > TimeSpan.Zero)
        {
            var cleanupIntervalMs = (int)_options.CleanupInterval.TotalMilliseconds;
            _cleanupTimer = new Timer(PerformCleanup, null, cleanupIntervalMs, cleanupIntervalMs);
        }
    }

    /// <summary>
    /// Increments the count for a specific query type.
    /// </summary>
    /// <param name="queryType">The name of the query type.</param>
    public void IncrementQuery(string queryType)
    {
        if (!_options.EnableRequestMetrics || string.IsNullOrEmpty(queryType))
        {
            return;
        }

        _queryCounts.AddOrUpdate(queryType, 1, (_, count) => count + 1);
        UpdateMetricTimestamp(queryType);
    }

    /// <summary>
    /// Increments the count for a specific command type.
    /// </summary>
    /// <param name="commandType">The name of the command type.</param>
    public void IncrementCommand(string commandType)
    {
        if (!_options.EnableRequestMetrics || string.IsNullOrEmpty(commandType))
        {
            return;
        }

        _commandCounts.AddOrUpdate(commandType, 1, (_, count) => count + 1);
        UpdateMetricTimestamp(commandType);
    }

    /// <summary>
    /// Increments the count for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The name of the notification type.</param>
    public void IncrementNotification(string notificationType)
    {
        if (!_options.EnableNotificationMetrics || string.IsNullOrEmpty(notificationType))
        {
            return;
        }

        _notificationCounts.AddOrUpdate(notificationType, 1, (_, count) => count + 1);
        UpdateMetricTimestamp(notificationType);
    }

    /// <summary>
    /// Records the execution time for a specific request type when performance counters are enabled.
    /// </summary>
    /// <param name="requestType">The name of the request type.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="successful">Whether the execution was successful.</param>
    public void RecordExecutionTime(string requestType, long executionTimeMs, bool successful = true)
    {
        if (!_options.EnablePerformanceCounters || string.IsNullOrEmpty(requestType))
        {
            return;
        }

        // Update execution times for percentile calculations
        _executionTimes.AddOrUpdate(requestType, [executionTimeMs], (_, times) =>
        {
            lock (times)
            {
                times.Add(executionTimeMs);

                // Keep only the last N entries to prevent unbounded growth
                if (times.Count > 1000)
                {
                    times.RemoveAt(0);
                }

                return times;
            }
        });

        // Update aggregated metrics
        _totalExecutions.AddOrUpdate(requestType, 1, (_, count) => count + 1);
        _totalExecutionTime.AddOrUpdate(requestType, executionTimeMs, (_, total) => total + executionTimeMs);
        _lastExecutionTimes[requestType] = DateTime.UtcNow;

        if (!successful)
        {
            _failedExecutions.AddOrUpdate(requestType, 1, (_, count) => count + 1);
        }
    }

    /// <summary>
    /// Records memory allocation for performance tracking when performance counters are enabled.
    /// </summary>
    /// <param name="bytesAllocated">The number of bytes allocated.</param>
    public void RecordMemoryAllocation(long bytesAllocated)
    {
        if (!_options.EnablePerformanceCounters)
        {
            return;
        }

        lock (_memoryLock)
        {
            _totalMemoryAllocated += bytesAllocated;
        }
    }

    /// <summary>
    /// Records middleware execution metrics when middleware metrics are enabled.
    /// </summary>
    /// <param name="middlewareType">The middleware type name.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="successful">Whether the middleware executed successfully.</param>
    public void RecordMiddlewareExecution(string middlewareType, long executionTimeMs, bool successful = true)
    {
        if (!_options.EnableMiddlewareMetrics || string.IsNullOrEmpty(middlewareType))
        {
            return;
        }

        _middlewareExecutionCounts.AddOrUpdate(middlewareType, 1, (_, count) => count + 1);
        _middlewareExecutionTimes.AddOrUpdate(middlewareType, executionTimeMs, (_, total) => total + executionTimeMs);

        if (!successful)
        {
            _middlewareFailures.AddOrUpdate(middlewareType, 1, (_, count) => count + 1);
        }

        UpdateMetricTimestamp($"middleware_{middlewareType}");
    }

    /// <summary>
    /// Records detailed execution pattern when detailed analysis is enabled.
    /// </summary>
    /// <param name="requestType">The request type name.</param>
    /// <param name="executionTime">The execution timestamp.</param>
    public void RecordExecutionPattern(string requestType, DateTime executionTime)
    {
        if (!_options.EnableDetailedAnalysis || string.IsNullOrEmpty(requestType))
        {
            return;
        }

        // Check max tracked request types limit
        if (_executionPatterns.Count >= _options.MaxTrackedRequestTypes &&
            !_executionPatterns.ContainsKey(requestType))
        {
            return; // Don't track new request types if we've hit the limit
        }

        _executionPatterns.AddOrUpdate(requestType, [executionTime], (_, patterns) =>
        {
            lock (patterns)
            {
                patterns.Add(executionTime);

                // Keep only recent patterns to prevent unbounded growth
                if (patterns.Count > 10000)
                {
                    patterns.RemoveAt(0);
                }

                return patterns;
            }
        });

        // Track hourly patterns
        var hourKey = $"{requestType}_{executionTime:yyyy-MM-dd-HH}";
        _hourlyExecutionCounts.AddOrUpdate(hourKey, 1, (_, count) => count + 1);

        UpdateMetricTimestamp($"pattern_{requestType}");
    }

    /// <summary>
    /// Gets performance metrics for a specific request type.
    /// </summary>
    /// <param name="requestType">The name of the request type.</param>
    /// <returns>Performance metrics if available, null otherwise.</returns>
    public PerformanceMetrics? GetPerformanceMetrics(string requestType)
    {
        if (!_options.EnablePerformanceCounters || string.IsNullOrEmpty(requestType))
        {
            return null;
        }

        if (!_totalExecutions.TryGetValue(requestType, out var totalExecutions) || totalExecutions == 0)
        {
            return null;
        }

        _totalExecutionTime.TryGetValue(requestType, out var totalTime);
        _failedExecutions.TryGetValue(requestType, out var failures);
        _lastExecutionTimes.TryGetValue(requestType, out var lastExecution);

        var averageTime = totalExecutions > 0 ? (double)totalTime / totalExecutions : 0;
        var successRate = totalExecutions > 0 ? (double)(totalExecutions - failures) / totalExecutions * 100 : 0;

        // Calculate percentiles if we have execution time data
        double p50 = 0, p95 = 0, p99 = 0;
        if (_executionTimes.TryGetValue(requestType, out var times) && times.Count > 0)
        {
            lock (times)
            {
                var sortedTimes = times.OrderBy(t => t).ToArray();
                p50 = GetPercentile(sortedTimes, 0.5);
                p95 = GetPercentile(sortedTimes, 0.95);
                p99 = GetPercentile(sortedTimes, 0.99);
            }
        }

        return new PerformanceMetrics(
            requestType,
            totalExecutions,
            failures,
            averageTime,
            successRate,
            lastExecution,
            p50,
            p95,
            p99
        );
    }

    /// <summary>
    /// Gets overall performance summary when performance counters are enabled.
    /// </summary>
    /// <returns>Performance summary with overall metrics.</returns>
    public PerformanceSummary? GetPerformanceSummary()
    {
        if (!_options.EnablePerformanceCounters)
        {
            return null;
        }

        var totalRequests = _totalExecutions.Values.Sum();
        var totalFailures = _failedExecutions.Values.Sum();
        var totalTime = _totalExecutionTime.Values.Sum();

        long totalMemory;
        lock (_memoryLock)
        {
            totalMemory = _totalMemoryAllocated;
        }

        var averageTime = totalRequests > 0 ? (double)totalTime / totalRequests : 0;
        var overallSuccessRate = totalRequests > 0 ? (double)(totalRequests - totalFailures) / totalRequests * 100 : 0;

        return new PerformanceSummary(
            totalRequests,
            totalFailures,
            averageTime,
            overallSuccessRate,
            totalMemory,
            _totalExecutions.Count
        );
    }

    /// <summary>
    /// Calculates the specified percentile from a sorted array of values.
    /// </summary>
    /// <param name="sortedValues">Array of sorted values.</param>
    /// <param name="percentile">Percentile to calculate (0.0 to 1.0).</param>
    /// <returns>The percentile value.</returns>
    private static double GetPercentile(long[] sortedValues, double percentile)
    {
        if (sortedValues.Length == 0) return 0;
        if (sortedValues.Length == 1) return sortedValues[0];

        var index = percentile * (sortedValues.Length - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper)
        {
            return sortedValues[lower];
        }

        var weight = index - lower;
        return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
    }

    /// <summary>
    /// Reports the current statistics using the configured renderer.
    /// </summary>
    public void ReportStatistics()
    {
        _renderer.Render("Mediator Statistics:");
        _renderer.Render($"Queries: {_queryCounts.Count}");
        _renderer.Render($"Commands: {_commandCounts.Count}");
        _renderer.Render($"Notifications: {_notificationCounts.Count}");

        // Include performance metrics if enabled
        if (_options.EnablePerformanceCounters)
        {
            var summary = GetPerformanceSummary();
            if (summary.HasValue)
            {
                var summaryValue = summary.Value;
                _renderer.Render("");
                _renderer.Render("Performance Summary:");
                _renderer.Render($"Total Requests: {summaryValue.TotalRequests:N0}");
                _renderer.Render($"Failed Requests: {summaryValue.TotalFailures:N0}");
                _renderer.Render($"Success Rate: {summaryValue.OverallSuccessRate:F1}%");
                _renderer.Render($"Average Execution Time: {summaryValue.AverageExecutionTimeMs:F1}ms");
                _renderer.Render($"Total Memory Allocated: {summaryValue.TotalMemoryAllocatedBytes:N0} bytes");
                _renderer.Render($"Unique Request Types: {summaryValue.UniqueRequestTypes}");
            }
        }
    }

    /// <summary>
    /// Performs cleanup of expired metrics data based on the retention period.
    /// </summary>
    /// <param name="state">Timer callback state (unused).</param>
    private void PerformCleanup(object? state)
    {
        if (_disposed || _options.MetricsRetentionPeriod <= TimeSpan.Zero)
        {
            return;
        }

        lock (_cleanupLock)
        {
            if (_disposed)
            {
                return;
            }

            var cutoffTime = DateTime.UtcNow - _options.MetricsRetentionPeriod;
            var keysToRemove = new List<string>();

            // Find expired metric entries
            foreach (var kvp in _metricTimestamps)
            {
                if (kvp.Value < cutoffTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            // Remove expired metrics
            foreach (var key in keysToRemove)
            {
                RemoveMetricEntry(key);
            }

            // Also limit the number of entries if configured
            if (_options.MaxTrackedRequestTypes > 0)
            {
                LimitMetricEntries();
            }
        }
    }

    /// <summary>
    /// Removes a metric entry from all tracking dictionaries.
    /// </summary>
    /// <param name="key">The metric key to remove.</param>
    private void RemoveMetricEntry(string key)
    {
        _queryCounts.TryRemove(key, out _);
        _commandCounts.TryRemove(key, out _);
        _notificationCounts.TryRemove(key, out _);
        _metricTimestamps.TryRemove(key, out _);

        if (_options.EnablePerformanceCounters)
        {
            _executionTimes.TryRemove(key, out _);
            _totalExecutions.TryRemove(key, out _);
            _totalExecutionTime.TryRemove(key, out _);
            _failedExecutions.TryRemove(key, out _);
            _lastExecutionTimes.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Limits the number of metric entries to prevent unbounded growth.
    /// </summary>
    private void LimitMetricEntries()
    {
        if (_metricTimestamps.Count <= _options.MaxTrackedRequestTypes)
        {
            return;
        }

        // Remove oldest entries
        var sortedEntries = _metricTimestamps
            .OrderBy(kvp => kvp.Value)
            .Take(_metricTimestamps.Count - _options.MaxTrackedRequestTypes)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in sortedEntries)
        {
            RemoveMetricEntry(key);
        }
    }

    /// <summary>
    /// Updates the timestamp for a metric entry to track when it was last accessed.
    /// </summary>
    /// <param name="key">The metric key.</param>
    private void UpdateMetricTimestamp(string key)
    {
        if (_options.MetricsRetentionPeriod > TimeSpan.Zero)
        {
            _metricTimestamps[key] = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Disposes of the resources used by the MediatorStatistics instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_cleanupLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cleanupTimer?.Dispose();
        }
    }

    /// <summary>
    /// Analyzes all registered queries in the application and returns detailed information grouped by assembly and namespace.
    /// The level of detail depends on the EnableDetailedAnalysis option in StatisticsOptions.
    /// </summary>
    /// <param name="serviceProvider">Service provider to scan for registered query types.</param>
    /// <param name="isDetailed">If specified, overrides the EnableDetailedAnalysis option. If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Read-only list of query analysis information grouped by assembly with namespace.</returns>
    public IReadOnlyList<QueryCommandAnalysis> AnalyzeQueries(IServiceProvider serviceProvider, bool? isDetailed = null)
    {
        // Use the parameter if provided, otherwise use the options setting
        bool useDetailedAnalysis = isDetailed ?? _options.EnableDetailedAnalysis;

        // Look for IQuery<T> implementations first
        var queryTypes = FindTypesImplementingInterface(typeof(IQuery<>));

        // Also include IRequest<T> types that look like queries (contain "Query" in name)
        var requestWithResponseTypes = FindTypesImplementingInterface(typeof(IRequest<>))
            .Where(t => t.Name.Contains("Query", StringComparison.OrdinalIgnoreCase));

        var allQueryTypes = queryTypes.Concat(requestWithResponseTypes).Distinct().ToList();

        var results = CreateAnalysisResults(allQueryTypes, serviceProvider, true, useDetailedAnalysis);

        return results;
    }

    /// <summary>
    /// Analyzes all registered commands in the application and returns detailed information grouped by assembly and namespace.
    /// The level of detail depends on the EnableDetailedAnalysis option in StatisticsOptions.
    /// </summary>
    /// <param name="serviceProvider">Service provider to scan for registered command types.</param>
    /// <param name="isDetailed">If specified, overrides the EnableDetailedAnalysis option. If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Read-only list of command analysis information grouped by assembly with namespace.</returns>
    public IReadOnlyList<QueryCommandAnalysis> AnalyzeCommands(IServiceProvider serviceProvider, bool? isDetailed = null)
    {
        // Use the parameter if provided, otherwise use the options setting
        bool useDetailedAnalysis = isDetailed ?? _options.EnableDetailedAnalysis;

        // Look for ICommand and ICommand<T> implementations first
        var commandTypes = FindTypesImplementingInterface(typeof(ICommand))
            .Concat(FindTypesImplementingInterface(typeof(ICommand<>)))
            .Distinct()
            .ToList();

        // Also include IRequest types (void commands) that look like commands
        var voidRequestTypes = FindTypesImplementingInterface(typeof(IRequest))
            .Where(t => !t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))) // Exclude IRequest<T>
            .Where(t => t.Name.Contains("Command", StringComparison.OrdinalIgnoreCase));

        // Include IRequest<T> types that look like commands (contain "Command" in name)
        var requestWithResponseTypes = FindTypesImplementingInterface(typeof(IRequest<>))
            .Where(t => t.Name.Contains("Command", StringComparison.OrdinalIgnoreCase));

        var allCommandTypes = commandTypes.Concat(voidRequestTypes).Concat(requestWithResponseTypes).Distinct().ToList();

        var results = CreateAnalysisResults(allCommandTypes, serviceProvider, false, useDetailedAnalysis);

        return results;
    }

    /// <summary>
    /// Analyzes all registered notifications in the application and returns detailed information about handlers and subscribers.
    /// </summary>
    /// <param name="serviceProvider">Service provider to scan for registered notification types.</param>
    /// <param name="isDetailed">Whether to return detailed analysis or compact view.</param>
    /// <returns>Read-only list of notification analysis information.</returns>
    public IReadOnlyList<NotificationAnalysis> AnalyzeNotifications(IServiceProvider serviceProvider, bool? isDetailed = null)
    {
        // Use the parameter if provided, otherwise use the options setting
        bool useDetailedAnalysis = isDetailed ?? _options.EnableDetailedAnalysis;

        // Look for INotification implementations
        var notificationTypes = FindTypesImplementingInterface(typeof(INotification));

        var results = CreateNotificationAnalysisResults(notificationTypes, serviceProvider, useDetailedAnalysis);

        return results;
    }

    /// <summary>
    /// Finds all types in loaded assemblies that implement the specified interface.
    /// </summary>
    private static List<Type> FindTypesImplementingInterface(Type interfaceType)
    {
        var types = new List<Type>();

        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                var assemblyTypes = assembly.GetTypes()
                    .Where(t => t is { IsAbstract: false, IsInterface: false, IsClass: true })
                    .ToList();

                types.AddRange(assemblyTypes.Where(type => ImplementsInterface(type, interfaceType)));
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded
            }
        }

        return types;
    }

    /// <summary>
    /// Checks if a type implements the specified interface (including generic interfaces).
    /// </summary>
    private static bool ImplementsInterface(Type type, Type interfaceType)
    {
        if (interfaceType.IsGenericTypeDefinition)
        {
            return type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);
        }

        return interfaceType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Finds registered handlers for a specific request type.
    /// </summary>
    private static List<Type> FindHandlersForRequestType(Type requestType, IServiceProvider serviceProvider)
    {
        var handlers = new List<Type>();

        try
        {
            // Determine the handler interface type based on request type
            Type? handlerInterfaceType = null;

            // Check for IQuery<T> -> IRequestHandler<IQuery<T>, T>
            var queryInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

            if (queryInterface != null)
            {
                var responseType = queryInterface.GetGenericArguments()[0];
                handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            }

            // Check for ICommand -> IRequestHandler<ICommand>
            if (handlerInterfaceType == null && typeof(ICommand).IsAssignableFrom(requestType))
            {
                handlerInterfaceType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            }

            // Check for ICommand<T> -> IRequestHandler<ICommand<T>, T>
            if (handlerInterfaceType == null)
            {
                var commandInterface = requestType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

                if (commandInterface != null)
                {
                    var responseType = commandInterface.GetGenericArguments()[0];
                    handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                }
            }

            // Check for IRequest -> IRequestHandler<IRequest>
            if (handlerInterfaceType == null && typeof(IRequest).IsAssignableFrom(requestType) && !requestType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            {
                handlerInterfaceType = typeof(IRequestHandler<>).MakeGenericType(requestType);
            }

            // Check for IRequest<T> -> IRequestHandler<IRequest<T>, T>
            if (handlerInterfaceType == null)
            {
                var requestInterface = requestType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

                if (requestInterface != null)
                {
                    var responseType = requestInterface.GetGenericArguments()[0];
                    handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                }
            }

            if (handlerInterfaceType != null)
            {
                // Try to get all registered services for this handler interface
                var handlerServices = serviceProvider.GetServices(handlerInterfaceType);
                handlers.AddRange(handlerServices.Select(h => h?.GetType()).Distinct()!);
            }
        }
        catch (Exception ex)
        {
            // For debugging: log the exception details (in production this would be logged)
            Debug.WriteLine($"Error finding handlers for {requestType.Name}: {ex.Message}");
        }

        return handlers;
    }

    /// <summary>
    /// Creates notification analysis results from a collection of notification types.
    /// </summary>
    private static IReadOnlyList<NotificationAnalysis> CreateNotificationAnalysisResults(IEnumerable<Type> types, IServiceProvider serviceProvider, bool isDetailed)
    {
        var analysisResults = new List<NotificationAnalysis>();

        foreach (var type in types.OrderBy(t => t.Assembly.GetName().Name).ThenBy(t => t.Namespace ?? "Unknown").ThenBy(t => t.Name))
        {
            var className = type.Name;
            var typeParameters = string.Empty;

            // Handle generic types
            if (type.IsGenericType)
            {
                // Remove generic suffix from class name
                var backtickIndex = className.IndexOf('`');
                if (backtickIndex > 0)
                {
                    className = className[..backtickIndex];
                }

                // Get type parameters
                var genericArgs = type.GetGenericArguments();
                typeParameters = $"<{string.Join(", ", genericArgs.Select(t => t.Name))}>";
            }

            // Determine primary interface
            string primaryInterface = typeof(INotification).IsAssignableFrom(type) ? "INotification" : "Unknown";

            // Find automatic handlers (INotificationHandler<T>)
            var handlers = FindNotificationHandlers(type, serviceProvider);
            HandlerStatus handlerStatus;
            string handlerDetails;

            // Determine handler status
            switch (handlers.Count)
            {
                case 0:
                    handlerStatus = HandlerStatus.Missing;
                    handlerDetails = isDetailed ? "No handlers registered" : "No handlers";
                    break;
                case 1:
                    handlerStatus = HandlerStatus.Single;
                    handlerDetails = isDetailed ? handlers[0].Name : "Handler found";
                    break;
                default:
                    handlerStatus = HandlerStatus.Multiple;
                    handlerDetails = isDetailed
                        ? $"{handlers.Count} handlers: {string.Join(", ", handlers.Select(h => h.Name))}"
                        : $"{handlers.Count} handlers";
                    break;
            }

            // Estimate manual subscribers (this is approximate since subscribers can be registered dynamically)
            var (subscriberStatus, subscriberDetails, estimatedSubscribers) = EstimateSubscribers(type, serviceProvider, isDetailed);

            analysisResults.Add(new NotificationAnalysis(
                Type: type,
                ClassName: className,
                TypeParameters: isDetailed ? typeParameters : string.Empty,
                Assembly: type.Assembly.GetName().Name ?? "Unknown",
                Namespace: type.Namespace ?? "Unknown",
                PrimaryInterface: primaryInterface,
                HandlerStatus: handlerStatus,
                HandlerDetails: handlerDetails,
                Handlers: isDetailed ? handlers : [],
                SubscriberStatus: subscriberStatus,
                SubscriberDetails: subscriberDetails,
                EstimatedSubscribers: estimatedSubscribers
            ));
        }

        return analysisResults;
    }

    /// <summary>
    /// Finds registered automatic notification handlers for a specific notification type.
    /// </summary>
    private static List<Type> FindNotificationHandlers(Type notificationType, IServiceProvider serviceProvider)
    {
        var handlers = new List<Type>();

        try
        {
            // Look for INotificationHandler<T> implementations
            var handlerInterfaceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            
            // Try to get all registered services for this handler interface
            var handlerServices = serviceProvider.GetServices(handlerInterfaceType);
            handlers.AddRange(handlerServices.Where(h => h != null).Select(h => h!.GetType()).Distinct());
        }
        catch (Exception ex)
        {
            // For debugging: log the exception details
            Debug.WriteLine($"Error finding handlers for notification {notificationType.Name}: {ex.Message}");
        }

        return handlers;
    }

    /// <summary>
    /// Estimates the number and status of manual subscribers for a notification type.
    /// Note: This is approximate since subscribers can be registered dynamically at runtime.
    /// </summary>
    private static (SubscriberStatus Status, string Details, int EstimatedCount) EstimateSubscribers(Type notificationType, IServiceProvider serviceProvider, bool isDetailed)
    {
        try
        {
            // Try to get the mediator service to check for registered subscribers
            var mediator = serviceProvider.GetService<IMediator>();
            if (mediator is Mediator concreteMediator)
            {
                // Use reflection to access internal subscriber tracking if available
                // Note: This is a best-effort estimation and may not be completely accurate
                var subscriberField = typeof(Mediator).GetField("_notificationSubscribers", BindingFlags.NonPublic | BindingFlags.Instance);
                if (subscriberField?.GetValue(concreteMediator) is IDictionary<string, object> subscribersDict)
                {
                    // Check if this notification type has any subscribers
                    var typeKey = notificationType.FullName ?? notificationType.Name;
                    var hasSubscribers = false;
                    var estimatedCount = 0;

                    // Iterate through the subscribers dictionary to find matches
                    foreach (var key in subscribersDict.Keys)
                    {
                        if (key?.ToString()?.Contains(typeKey) == true)
                        {
                            hasSubscribers = true;
                            estimatedCount++;
                        }
                    }

                    if (hasSubscribers)
                    {
                        var details = isDetailed 
                            ? $"Estimated {estimatedCount} subscriber(s) (dynamic registration may affect accuracy)"
                            : $"{estimatedCount} subscriber(s)";
                        return (SubscriberStatus.Present, details, estimatedCount);
                    }
                }
            }

            // If we can't determine, return unknown status
            var unknownDetails = isDetailed 
                ? "Cannot determine subscriber status (may be registered dynamically)"
                : "Unknown";
            return (SubscriberStatus.Unknown, unknownDetails, 0);
        }
        catch
        {
            // If there's any error, return none
            var noneDetails = isDetailed 
                ? "No subscribers found (or unable to detect)"
                : "No subscribers";
            return (SubscriberStatus.None, noneDetails, 0);
        }
    }

    /// <summary>
    /// Creates analysis results from a collection of types, grouped by assembly and namespace.
    /// </summary>
    private static IReadOnlyList<QueryCommandAnalysis> CreateAnalysisResults(IEnumerable<Type> types, IServiceProvider serviceProvider, bool isQuery, bool isDetailed)
    {
        var analysisResults = new List<QueryCommandAnalysis>();

        foreach (var type in types.OrderBy(t => t.Assembly.GetName().Name).ThenBy(t => t.Namespace ?? "Unknown").ThenBy(t => t.Name))
        {
            var className = type.Name;
            var typeParameters = string.Empty;
            Type? responseType = null;
            string primaryInterface;
            bool isResultType = false;

            // Handle generic types
            if (type.IsGenericType)
            {
                // Remove generic suffix from class name
                var backtickIndex = className.IndexOf('`');
                if (backtickIndex > 0)
                {
                    className = className[..backtickIndex];
                }

                // Get type parameters
                var genericArgs = type.GetGenericArguments();
                typeParameters = $"<{string.Join(", ", genericArgs.Select(t => t.Name))}>";
            }

            // Determine primary interface and response type based on priority
            var interfaces = type.GetInterfaces();

            if (isQuery)
            {
                // For queries, prioritize IQuery<T> > IRequest<T>
                var queryInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
                if (queryInterface != null)
                {
                    primaryInterface = $"IQuery<{queryInterface.GetGenericArguments()[0].Name}>";
                    responseType = queryInterface.GetGenericArguments()[0];
                }
                else
                {
                    var requestInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                    if (requestInterface != null)
                    {
                        primaryInterface = $"IRequest<{requestInterface.GetGenericArguments()[0].Name}>";
                        responseType = requestInterface.GetGenericArguments()[0];
                    }
                    else
                    {
                        primaryInterface = "IRequest";
                    }
                }
            }
            else
            {
                // For commands, prioritize ICommand > ICommand<T> > IRequest > IRequest<T>
                if (interfaces.Any(i => i == typeof(ICommand)))
                {
                    primaryInterface = "ICommand";
                }
                else
                {
                    var commandInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
                    if (commandInterface != null)
                    {
                        primaryInterface = $"ICommand<{commandInterface.GetGenericArguments()[0].Name}>";
                        responseType = commandInterface.GetGenericArguments()[0];
                    }
                    else if (interfaces.Any(i => i == typeof(IRequest)))
                    {
                        primaryInterface = "IRequest";
                    }
                    else
                    {
                        var requestInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                        if (requestInterface != null)
                        {
                            primaryInterface = $"IRequest<{requestInterface.GetGenericArguments()[0].Name}>";
                            responseType = requestInterface.GetGenericArguments()[0];
                        }
                        else
                        {
                            primaryInterface = "Unknown";
                        }
                    }
                }
            }

            // ALWAYS discover handlers using the same logic, regardless of detail level
            var handlers = FindHandlersForRequestType(type, serviceProvider);
            HandlerStatus handlerStatus;
            string handlerDetails;

            // Determine handler status based on discovered handlers
            switch (handlers.Count)
            {
                case 0:
                    handlerStatus = HandlerStatus.Missing;
                    handlerDetails = isDetailed ? "No handler registered" : "No handler";
                    break;
                case 1:
                    handlerStatus = HandlerStatus.Single;
                    handlerDetails = isDetailed ? handlers[0].Name : "Handler found";
                    break;
                default:
                    handlerStatus = HandlerStatus.Multiple;
                    handlerDetails = isDetailed
                        ? $"{handlers.Count} handlers: {string.Join(", ", handlers.Select(h => h.Name))}"
                        : $"{handlers.Count} handlers";
                    break;
            }

            // Check if response type implements IResult (common in ASP.NET Core) - only if detailed
            if (isDetailed && responseType != null)
            {
                // Check for IResult interface (Microsoft.AspNetCore.Http.IResult)
                isResultType = responseType.GetInterfaces().Any(i => i.Name == "IResult") ||
                              responseType.Name.Contains("Result") ||
                              responseType.FullName?.Contains("Microsoft.AspNetCore.Http.IResult") == true;
            }

            analysisResults.Add(new QueryCommandAnalysis(
                Type: type,
                ClassName: className,
                TypeParameters: isDetailed ? typeParameters : string.Empty, // Only include type parameters in detailed mode
                Assembly: type.Assembly.GetName().Name ?? "Unknown",
                Namespace: type.Namespace ?? "Unknown",
                ResponseType: responseType,
                PrimaryInterface: primaryInterface,
                IsResultType: isResultType,
                HandlerStatus: handlerStatus,
                HandlerDetails: handlerDetails,
                Handlers: isDetailed ? handlers : [] // Only include handler list in detailed mode
            ));
        }

        return analysisResults;
    }
}