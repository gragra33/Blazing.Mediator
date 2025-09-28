using System.Diagnostics;
using static Blazing.Mediator.Pipeline.MiddlewarePipelineBuilder;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Collects and reports statistics about mediator usage, including query and command analysis.
/// </summary>
public sealed class MediatorStatistics : IDisposable
{
    private const string QueryNamePattern = "Query";
    private const string CommandNamePattern = "Command";
    private const string ResultNamePattern = "Result";
    private const string PipelineNamespacePart = "Pipeline";
    private const string PlaceholderSuffix = "Placeholder";
    private const string SimplePrefix = "Simple";
    private const string BlazingMediatorAssembly = "Blazing.Mediator";
    private const string AspNetCoreIResultFullName = "Microsoft.AspNetCore.Http.IResult";
    private const string IResultInterfaceName = "IResult";
    private const string NoHandlersRegistered = "No handlers registered";
    private const string NoHandlers = "No handlers";
    private const string HandlerFound = "Handler found";
    private const string IQueryGeneric = "IQuery<{0}>";
    private const string IRequestGeneric = "IRequest<{0}>";
    private const string IRequest = "IRequest";
    private const string ICommand = "ICommand";
    private const string ICommandGeneric = "ICommand<{0}>";
    private const string PatternPrefix = "pattern_";
    private const string RequestTypeLabel = "RequestType";
    private const string HandlerTypeLabel = "HandlerType";
    private const string ExceptionTypeLabel = "ExceptionType";
    private const string PipelineStageLabel = "PipelineStage";
    private const string NoHandlersMessage = "No handlers registered for request type: ";
    private const string MultipleHandlersMessage = "Multiple handlers registered for request type: ";
    private const string ExceptionMessage = "Exception occurred while handling request of type: ";
    private const string PipelineExceptionMessage = "Exception occurred in pipeline stage: ";
    private const string StreamingExceptionMessage = "Exception occurred in streaming handler for request type: ";
    private const string NotificationExceptionMessage = "Exception occurred in notification handler for request type: ";

    private readonly ConcurrentDictionary<string, long> _queryCounts = new();
    private readonly ConcurrentDictionary<string, long> _commandCounts = new();
    private readonly ConcurrentDictionary<string, long> _notificationCounts = new();
    private readonly IStatisticsRenderer _renderer;
    private readonly StatisticsOptions _options;
    private readonly MediatorLogger? _mediatorLogger;
    
    // Performance counters (only used when EnablePerformanceCounters is true)
    private readonly ConcurrentDictionary<string, List<long>> _executionTimes = new();
    private readonly ConcurrentDictionary<string, long> _totalExecutions = new();
    private readonly ConcurrentDictionary<string, long> _totalExecutionTime = new();
    private readonly ConcurrentDictionary<string, long> _failedExecutions = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastExecutionTimes = new();

    // Memory usage tracking (when performance counters enabled)
    private long _totalMemoryAllocated;
    private readonly Lock _memoryLock = new();

    // Notification performance counters (only used when EnablePerformanceCounters is true)
    private readonly ConcurrentDictionary<string, List<long>> _notificationExecutionTimes = new();
    private readonly ConcurrentDictionary<string, long> _totalNotificationExecutions = new();
    private readonly ConcurrentDictionary<string, long> _totalNotificationExecutionTime = new();
    private readonly ConcurrentDictionary<string, long> _failedNotificationExecutions = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastNotificationExecutionTimes = new();

    // Notification memory usage tracking (when performance counters enabled)
    private long _totalNotificationMemoryAllocated;
    private readonly Lock _notificationMemoryLock = new();

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
    private readonly Lock _cleanupLock = new();
    private bool _disposed;

    // Static cache to avoid repeated assembly scanning
    private static readonly ConcurrentDictionary<Type, List<Type>> _typeCache = new();

    /// <summary>
    /// Initializes a new instance of the MediatorStatistics class.
    /// </summary>
    /// <param name="renderer">The renderer to use for statistics output.</param>
    /// <param name="options">The statistics tracking options. If null, uses default options.</param>
    /// <param name="mediatorLogger">Optional MediatorLogger for debug-level logging of analysis operations.</param>
    public MediatorStatistics(IStatisticsRenderer renderer, StatisticsOptions? options = null, MediatorLogger? mediatorLogger = null)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _options = options ?? new StatisticsOptions();
        _mediatorLogger = mediatorLogger;

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
        
        LogQueryIncrementedImpl(queryType);
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
        
        LogCommandIncrementedImpl(commandType);
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
        
        LogNotificationIncrementedImpl(notificationType);
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
        
        LogExecutionTimeRecordedImpl(requestType, executionTimeMs, successful);
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
        
        LogMemoryAllocationRecordedImpl(bytesAllocated);
    }

    /// <summary>
    /// Records the execution time for a specific notification type when performance counters are enabled.
    /// </summary>
    /// <param name="notificationType">The name of the notification type.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="successful">Whether the execution was successful.</param>
    public void RecordNotificationExecutionTime(string notificationType, long executionTimeMs, bool successful = true)
    {
        if (!_options.EnablePerformanceCounters || string.IsNullOrEmpty(notificationType))
        {
            return;
        }

        // Update execution times for percentile calculations
        _notificationExecutionTimes.AddOrUpdate(notificationType, [executionTimeMs], (_, times) =>
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
        _totalNotificationExecutions.AddOrUpdate(notificationType, 1, (_, count) => count + 1);
        _totalNotificationExecutionTime.AddOrUpdate(notificationType, executionTimeMs, (_, total) => total + executionTimeMs);
        _lastNotificationExecutionTimes[notificationType] = DateTime.UtcNow;

        if (!successful)
        {
            _failedNotificationExecutions.AddOrUpdate(notificationType, 1, (_, count) => count + 1);
        }
        
        LogNotificationExecutionTimeRecordedImpl(notificationType, executionTimeMs, successful);
    }

    /// <summary>
    /// Records memory allocation for notification processing when performance counters are enabled.
    /// </summary>
    /// <param name="bytesAllocated">The number of bytes allocated for notification processing.</param>
    public void RecordNotificationMemoryAllocation(long bytesAllocated)
    {
        if (!_options.EnablePerformanceCounters)
        {
            return;
        }

        lock (_notificationMemoryLock)
        {
            _totalNotificationMemoryAllocated += bytesAllocated;
        }
        
        LogNotificationMemoryAllocationRecordedImpl(bytesAllocated);
    }

    /// <summary>
    /// Reports the current statistics using the configured renderer with enhanced separate performance sections.
    /// Shows actual total executions, not just unique type counts.
    /// </summary>
    public void ReportStatistics()
    {
        LogStatisticsReportStartedImpl();
        
        // Show total executions, not just unique types
        _renderer.Render("Mediator Statistics:");
        _renderer.Render($"Queries: {_queryCounts.Values.Sum()}"); // Total executions
        _renderer.Render($"Commands: {_commandCounts.Values.Sum()}"); // Total executions  
        _renderer.Render($"Notifications: {_notificationCounts.Values.Sum()}"); // Total executions

        // Include enhanced performance metrics if enabled
        if (_options.EnablePerformanceCounters)
        {
            // Overall Performance Summary
            var overallSummary = GetPerformanceSummary();
            if (overallSummary.HasValue)
            {
                var summaryValue = overallSummary.Value;
                _renderer.Render("");
                _renderer.Render("Overall Performance Summary:");
                _renderer.Render($"Total Operations: {summaryValue.TotalOperations:N0}");
                _renderer.Render($"Failed Operations: {summaryValue.TotalFailures:N0}");
                _renderer.Render($"Success Rate: {summaryValue.OverallSuccessRate:F1}%");
                _renderer.Render($"Average Execution Time: {summaryValue.AverageExecutionTimeMs:F1}ms");
                _renderer.Render($"Total Memory Allocated: {summaryValue.TotalMemoryAllocatedBytes:N0} bytes");
                _renderer.Render($"Unique Operation Types: {summaryValue.UniqueOperationTypes}");
            }

            // Request Performance Summary (filtered to requests only)
            _renderer.Render("");
            var requestSummary = GetRequestPerformanceSummary();
            if (requestSummary.HasValue && requestSummary.Value.TotalOperations > 0)
            {
                var requestSummaryValue = requestSummary.Value;
                _renderer.Render("Request Performance Summary:");
                _renderer.Render($"Request Operations: {requestSummaryValue.TotalOperations:N0}");
                _renderer.Render($"Request Failures: {requestSummaryValue.TotalFailures:N0}");
                _renderer.Render($"Request Success Rate: {requestSummaryValue.OverallSuccessRate:F1}%");
                _renderer.Render($"Request Avg Time: {requestSummaryValue.AverageExecutionTimeMs:F1}ms");
                _renderer.Render($"Request Operation Types: {requestSummaryValue.UniqueOperationTypes}");
            }
            else
            {
                _renderer.Render("Request Performance Summary: No request operations recorded");
            }

            // Notification Performance Summary (filtered to notifications only)
            _renderer.Render("");
            var notificationSummary = GetNotificationPerformanceSummary();
            if (notificationSummary.HasValue && notificationSummary.Value.TotalOperations > 0)
            {
                var notificationSummaryValue = notificationSummary.Value;
                _renderer.Render("Notification Performance Summary:");
                _renderer.Render($"Notification Operations: {notificationSummaryValue.TotalOperations:N0}");
                _renderer.Render($"Notification Failures: {notificationSummaryValue.TotalFailures:N0}");
                _renderer.Render($"Notification Success Rate: {notificationSummaryValue.OverallSuccessRate:F1}%");
                _renderer.Render($"Notification Avg Time: {notificationSummaryValue.AverageExecutionTimeMs:F1}ms");
                _renderer.Render($"Notification Operation Types: {notificationSummaryValue.UniqueOperationTypes}");
            }
            else
            {
                _renderer.Render("Notification Performance Summary: No notification operations recorded");
            }
        }
        
        LogStatisticsReportCompletedImpl();
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

            LogCleanupStartedImpl(keysToRemove.Count);

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
            
            LogCleanupCompletedImpl(keysToRemove.Count);
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
            
            LogStatisticsDisposedImpl();
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
        LogAnalysisStartedImpl("queries");
        
        // Use the parameter if provided, otherwise use the options setting
        bool useDetailedAnalysis = isDetailed ?? _options.EnableDetailedAnalysis;

        // Look for IQuery<T> implementations first
        var queryTypes = FindTypesImplementingInterface(typeof(IQuery<>));

        // Also include IRequest<T> types that look like queries (contain "Query" in name)
        var requestWithResponseTypes = FindTypesImplementingInterface(typeof(IRequest<>))
            .Where(t => t.Name.Contains(QueryNamePattern, StringComparison.OrdinalIgnoreCase));

        var allQueryTypes = queryTypes.Concat(requestWithResponseTypes).Distinct().ToList();

        var results = CreateAnalysisResults(allQueryTypes, serviceProvider, true, useDetailedAnalysis);

        LogAnalysisCompletedImpl("queries", results.Count);
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
        LogAnalysisStartedImpl("commands");
        
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
            .Where(t => t.Name.Contains(CommandNamePattern, StringComparison.OrdinalIgnoreCase));

        // Include IRequest<T> types that look like commands (contain "Command" in name)
        var requestWithResponseTypes = FindTypesImplementingInterface(typeof(IRequest<>))
            .Where(t => t.Name.Contains(CommandNamePattern, StringComparison.OrdinalIgnoreCase));

        var allCommandTypes = commandTypes.Concat(voidRequestTypes).Concat(requestWithResponseTypes).Distinct().ToList();

        var results = CreateAnalysisResults(allCommandTypes, serviceProvider, false, useDetailedAnalysis);

        LogAnalysisCompletedImpl("commands", results.Count);
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
        LogAnalysisStartedImpl("notifications");
        
        // Use the parameter if provided, otherwise use the options setting
        bool useDetailedAnalysis = isDetailed ?? _options.EnableDetailedAnalysis;

        // Look for INotification implementations
        var notificationTypes = FindTypesImplementingInterface(typeof(INotification));

        var results = CreateNotificationAnalysisResults(notificationTypes, serviceProvider, useDetailedAnalysis);

        LogAnalysisCompletedImpl("notifications", results.Count);
        return results;
    }

    /// <summary>
    /// Finds all types in loaded assemblies that implement the specified interface.
    /// Uses optimized caching and filtering to avoid repeated expensive assembly scanning operations.
    /// </summary>
    private static List<Type> FindTypesImplementingInterface(Type interfaceType)
    {
        // Use a static cache to avoid repeated assembly scanning
        // This significantly improves performance for multiple analysis calls
        return _typeCache.GetOrAdd(interfaceType, static iType =>
        {
            var types = new List<Type>();

            try 
            {
                // Get all loaded assemblies with optimized filtering
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    // Filter out system assemblies early to reduce scanning overhead
                    .Where(a => 
                    {
                        var name = a.FullName ?? "";
                        return !name.StartsWith("System.") && 
                               !name.StartsWith("Microsoft.") && 
                               !name.StartsWith("netstandard") &&
                               !name.StartsWith("mscorlib");
                    })
                    .ToArray(); // Materialize to avoid re-evaluation

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var assemblyTypes = assembly.GetTypes()
                            .Where(t => t is { IsAbstract: false, IsInterface: false, IsClass: true })
                            .Where(ShouldIncludeTypeInAnalysis) // Filter out internal implementation details
                            .Where(type => ImplementsInterface(type, iType))
                            .ToArray(); // Materialize to avoid re-evaluation

                        types.AddRange(assemblyTypes);
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // Skip assemblies that can't be loaded
                        continue;
                    }
                }
            }
            catch
            {
                // If scanning fails entirely, return empty list instead of crashing
                return [];
            }

            return types;
        });
    }

    /// <summary>
    /// Determines whether a type should be included in statistics analysis.
    /// Filters out internal implementation details from the Blazing.Mediator assembly.
    /// </summary>
    private static bool ShouldIncludeTypeInAnalysis(Type type)
    {
        // Always include types from user assemblies (non-Blazing.Mediator assemblies)
        var assemblyName = type.Assembly.GetName().Name;
        if (assemblyName != BlazingMediatorAssembly)
        {
            return true;
        }

        // For types in the Blazing.Mediator assembly, exclude internal implementation details
        var typeName = type.Name;
        var typeFullName = type.FullName ?? string.Empty;

        // Exclude internal placeholder types used for constraint satisfaction
        if (typeName is nameof(InternalCommandPlaceholder) or nameof(InternalRequestPlaceholder))
        {
            return false;
        }

        // Exclude types that are clearly internal implementation details
        if (typeFullName.Contains(PipelineNamespacePart + ".") && (typeName.StartsWith(SimplePrefix) || typeName.EndsWith(PlaceholderSuffix)))
        {
            return false;
        }

        // Exclude nested private/internal classes
        if (type is { IsNested: true, IsNestedPublic: false })
        {
            return false;
        }

        // Include other public types from Blazing.Mediator (like test types in unit test assemblies)
        return type.IsPublic;
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
    /// Enhanced to support both automatic handlers and manual subscribers pattern detection.
    /// </summary>
    private static IReadOnlyList<NotificationAnalysis> CreateNotificationAnalysisResults(IEnumerable<Type> types, IServiceProvider serviceProvider, bool isDetailed)
    {
        var analysisResults = new List<NotificationAnalysis>();

        // Try to get pattern detector and subscriber tracker from DI (will be null if not registered)
        var subscriberTracker = serviceProvider.GetService<ISubscriberTracker>();

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

            // Enhanced subscriber tracking using SubscriberTracker if available
            var (subscriberStatus, subscriberDetails, subscriberCount) = subscriberTracker != null 
                ? GetEnhancedSubscriberStatus(type, subscriberTracker, isDetailed)
                : EstimateSubscribers(type, serviceProvider, isDetailed);

            analysisResults.Add(new NotificationAnalysis(
                Type: type,
                ClassName: className,
                TypeParameters: isDetailed ? typeParameters : string.Empty,
                Assembly: type.Assembly.GetName().Name ?? "Unknown",
                Namespace: type.Namespace ?? "Unknown",
                PrimaryInterface: primaryInterface,
                HandlerStatus: handlerStatus,
                HandlerDetails: handlerDetails,
                Handlers: handlers, // Always include handlers for accurate status determination in both modes
                SubscriberStatus: subscriberStatus,
                SubscriberDetails: subscriberDetails,
                EstimatedSubscribers: subscriberCount
            ));
        }

        return analysisResults;
    }

    /// <summary>
    /// Gets the subscriber status for a notification type using the enhanced tracking system.
    /// </summary>
    private static (SubscriberStatus Status, string Details, int Count) GetEnhancedSubscriberStatus(
        Type notificationType, 
        ISubscriberTracker subscriberTracker, 
        bool isDetailed)
    {
        try
        {
            var activeSubscribers = subscriberTracker.GetActiveSubscribers(notificationType);
            var subscriberCount = activeSubscribers.Count;

            if (subscriberCount == 0)
            {
                var noneDetails = isDetailed 
                    ? "No active subscribers detected"
                    : "No subscribers";
                return (SubscriberStatus.None, noneDetails, 0);
            }

            // Get subscriber type names for detailed reporting
            var subscriberTypes = activeSubscribers
                .Select(s => s.SubscriberType.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();

            var details = isDetailed
                ? $"{subscriberCount} active ({string.Join(", ", subscriberTypes)})"
                : $"{subscriberCount} subscriber{(subscriberCount == 1 ? "" : "s")}";
                
            return (SubscriberStatus.Present, details, subscriberCount);
        }
        catch
        {
            // Fallback to unknown status on any error
            var unknownDetails = isDetailed 
                ? "Cannot determine subscriber status (tracking error)"
                : "Unknown";
            return (SubscriberStatus.Unknown, unknownDetails, 0);
        }
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
                        if (key.Contains(typeKey))
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

            // Enhanced interface detection with priority for custom domain interfaces
            var interfaces = type.GetInterfaces();
            (primaryInterface, responseType) = DetectPrimaryInterface(interfaces, isQuery);

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
                isResultType = responseType.GetInterfaces().Any(i => i.Name == IResultInterfaceName) ||
                              responseType.Name.Contains(ResultNamePattern) ||
                              responseType.FullName?.Contains(AspNetCoreIResultFullName) == true;
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
                Handlers: handlers // Always include handlers for accurate status determination in both modes
            ));
        }

        return analysisResults;
    }

    /// <summary>
    /// Detects the most specific primary interface for a type, prioritizing custom domain interfaces over built-in Blazing.Mediator interfaces.
    /// This enables better analysis of domain-driven design patterns where custom interfaces like ICustomerRequest, IProductRequest, etc. are used.
    /// </summary>
    /// <param name="interfaces">All interfaces implemented by the type.</param>
    /// <param name="isQuery">Whether this is being analyzed as a query (true) or command (false).</param>
    /// <returns>A tuple containing the primary interface name and response type (if applicable).</returns>
    private static (string PrimaryInterface, Type? ResponseType) DetectPrimaryInterface(Type[] interfaces, bool isQuery)
    {
        // Separate built-in Blazing.Mediator interfaces from custom domain interfaces
        var customInterfaces = new List<Type>();
        var builtInInterfaces = new List<Type>();

        foreach (var iface in interfaces)
        {
            if (IsBuiltInBlazingMediatorInterface(iface))
            {
                builtInInterfaces.Add(iface);
            }
            else if (IsCustomDomainInterface(iface))
            {
                customInterfaces.Add(iface);
            }
        }

        // First, try to find the most specific custom domain interface
        if (customInterfaces.Count > 0)
        {
            var (customInterface, responseType) = FindMostSpecificInterface(customInterfaces, isQuery);
            if (customInterface != null)
            {
                return (FormatInterfaceName(customInterface), responseType);
            }
        }

        // Fall back to built-in interfaces using the original priority logic
        return DetectBuiltInPrimaryInterface(builtInInterfaces, isQuery);
    }

    /// <summary>
    /// Determines if an interface is a built-in Blazing.Mediator interface.
    /// </summary>
    /// <param name="interfaceType">The interface type to check.</param>
    /// <returns>True if it's a built-in interface, false otherwise.</returns>
    private static bool IsBuiltInBlazingMediatorInterface(Type interfaceType)
    {
        // Check for exact matches first
        if (interfaceType == typeof(IRequest) || interfaceType == typeof(ICommand))
        {
            return true;
        }

        // Check for generic interface definitions
        if (interfaceType.IsGenericType)
        {
            var genericDefinition = interfaceType.GetGenericTypeDefinition();
            return genericDefinition == typeof(IRequest<>) ||
                   genericDefinition == typeof(IQuery<>) ||
                   genericDefinition == typeof(ICommand<>);
        }

        return false;
    }

    /// <summary>
    /// Determines if an interface is a custom domain interface (extends built-in interfaces).
    /// Custom domain interfaces are those that inherit from IRequest, IQuery, or ICommand but are not the built-in types themselves.
    /// </summary>
    /// <param name="interfaceType">The interface type to check.</param>
    /// <returns>True if it's a custom domain interface, false otherwise.</returns>
    private static bool IsCustomDomainInterface(Type interfaceType)
    {
        // Must be an interface and not a built-in type
        if (!interfaceType.IsInterface || IsBuiltInBlazingMediatorInterface(interfaceType))
        {
            return false;
        }

        // Check if it extends any of the built-in interfaces
        var allInterfaces = interfaceType.GetInterfaces();
        
        return allInterfaces.Any(i => 
            i == typeof(IRequest) || 
            i == typeof(ICommand) ||
            (i.IsGenericType && (
                i.GetGenericTypeDefinition() == typeof(IRequest<>) ||
                i.GetGenericTypeDefinition() == typeof(IQuery<>) ||
                i.GetGenericTypeDefinition() == typeof(ICommand<>)
            ))
        );
    }

    /// <summary>
    /// Finds the most specific interface from a collection of custom interfaces.
    /// Prioritizes interfaces based on query/command context and inheritance depth.
    /// </summary>
    /// <param name="customInterfaces">Collection of custom domain interfaces.</param>
    /// <param name="isQuery">Whether this is being analyzed as a query.</param>
    /// <returns>The most specific interface and its response type (if applicable).</returns>
    private static (Type? Interface, Type? ResponseType) FindMostSpecificInterface(List<Type> customInterfaces, bool isQuery)
    {
        if (customInterfaces.Count == 0) return (null, null);

        // If there's only one custom interface, use it
        if (customInterfaces.Count == 1)
        {
            var singleInterface = customInterfaces[0];
            var responseType = GetResponseTypeFromInterface(singleInterface);
            return (singleInterface, responseType);
        }

        // Multiple custom interfaces - find the most specific one
        Type? bestInterface = null;
        Type? bestResponseType = null;
        int maxSpecificity = -1;

        foreach (var iface in customInterfaces)
        {
            var specificity = CalculateInterfaceSpecificity(iface, isQuery);
            if (specificity > maxSpecificity)
            {
                maxSpecificity = specificity;
                bestInterface = iface;
                bestResponseType = GetResponseTypeFromInterface(iface);
            }
        }

        return (bestInterface, bestResponseType);
    }

    /// <summary>
    /// Calculates a specificity score for an interface to help determine priority.
    /// Higher scores indicate more specific interfaces that should be prioritized in analysis.
    /// This uses only generic characteristics, not hardcoded domain patterns.
    /// </summary>
    /// <param name="interfaceType">The interface to score.</param>
    /// <param name="isQuery">Whether this is being analyzed as a query.</param>
    /// <returns>A specificity score where higher values are more specific.</returns>
    private static int CalculateInterfaceSpecificity(Type interfaceType, bool isQuery)
    {
        int score = 0;

        // Base score for being a custom interface
        score += 100;

        // Bonus for having generic parameters (more specific than non-generic)
        if (interfaceType.IsGenericType)
        {
            score += 20;
        }

        // Bonus based on inheritance depth (more derived interfaces are more specific)
        // This naturally prioritizes more specialized interfaces
        score += interfaceType.GetInterfaces().Length * 5;

        // Bonus for longer interface names (typically more specific)
        // This helps distinguish between IRequest and ICustomerRequest
        score += Math.Min(interfaceType.Name.Length * 2, 50); // Cap at 50 points

        // Bonus for interfaces that don't directly match built-in patterns
        // This helps prioritize custom interfaces over built-in ones
        var interfaceName = interfaceType.Name;
        if (!interfaceName.Equals("IRequest") && 
            !interfaceName.Equals("ICommand") && 
            !interfaceName.StartsWith("IRequest`") &&
            !interfaceName.StartsWith("ICommand`") &&
            !interfaceName.StartsWith("IQuery`"))
        {
            score += 25;
        }

        return score;
    }

    /// <summary>
    /// Extracts the response type from an interface if it has one.
    /// </summary>
    /// <param name="interfaceType">The interface to examine.</param>
    /// <returns>The response type if the interface is generic, null otherwise.</returns>
    private static Type? GetResponseTypeFromInterface(Type interfaceType)
    {
        if (!interfaceType.IsGenericType) return null;

        // For custom interfaces that extend IRequest<T>, IQuery<T>, or ICommand<T>
        var genericArgs = interfaceType.GetGenericArguments();
        return genericArgs.Length > 0 ? genericArgs[0] : null;
    }

    /// <summary>
    /// Formats an interface name for display, including generic parameters.
    /// </summary>
    /// <param name="interfaceType">The interface type to format.</param>
    /// <returns>A formatted string representation of the interface.</returns>
    private static string FormatInterfaceName(Type interfaceType)
    {
        if (!interfaceType.IsGenericType)
        {
            return interfaceType.Name;
        }

        var name = interfaceType.Name;
        var backtickIndex = name.IndexOf('`');
        if (backtickIndex > 0)
        {
            name = name[..backtickIndex];
        }

        var genericArgs = interfaceType.GetGenericArguments();
        var argNames = string.Join(", ", genericArgs.Select(t => t.Name));
        return $"{name}<{argNames}>";
    }

    /// <summary>
    /// Detects the primary interface using the original built-in interface priority logic.
    /// This serves as a fallback when no custom domain interfaces are found.
    /// </summary>
    /// <param name="builtInInterfaces">Collection of built-in Blazing.Mediator interfaces.</param>
    /// <param name="isQuery">Whether this is being analyzed as a query.</param>
    /// <returns>The primary interface name and response type.</returns>
    private static (string PrimaryInterface, Type? ResponseType) DetectBuiltInPrimaryInterface(List<Type> builtInInterfaces, bool isQuery)
    {
        Type? responseType = null;
        string primaryInterface;

        if (isQuery)
        {
            // For queries, prioritize IQuery<T> > IRequest<T>
            var queryInterface = builtInInterfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
            if (queryInterface != null)
            {
                primaryInterface = $"IQuery<{queryInterface.GetGenericArguments()[0].Name}>";
                responseType = queryInterface.GetGenericArguments()[0];
            }
            else
            {
                var requestInterface = builtInInterfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
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
            if (builtInInterfaces.Any(i => i == typeof(ICommand)))
            {
                primaryInterface = "ICommand";
            }
            else
            {
                var commandInterface = builtInInterfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
                if (commandInterface != null)
                {
                    primaryInterface = $"ICommand<{commandInterface.GetGenericArguments()[0].Name}>";
                    responseType = commandInterface.GetGenericArguments()[0];
                }
                else if (builtInInterfaces.Any(i => i == typeof(IRequest)))
                {
                    primaryInterface = "IRequest";
                }
                else
                {
                    var requestInterface = builtInInterfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
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

        return (primaryInterface, responseType);
    }

    #region Source Generated Logging Methods

    private void LogQueryIncrementedImpl(string queryType)
    {
        _mediatorLogger?.QueryIncremented(queryType);
    }

    private void LogCommandIncrementedImpl(string commandType)
    {
        _mediatorLogger?.CommandIncremented(commandType);
    }

    private void LogNotificationIncrementedImpl(string notificationType)
    {
        _mediatorLogger?.NotificationIncremented(notificationType);
    }

    private void LogExecutionTimeRecordedImpl(string requestType, long executionTimeMs, bool successful)
    {
        _mediatorLogger?.ExecutionTimeRecorded(requestType, executionTimeMs, successful);
    }

    private void LogMemoryAllocationRecordedImpl(long bytesAllocated)
    {
        _mediatorLogger?.MemoryAllocationRecorded(bytesAllocated);
    }

    private void LogNotificationExecutionTimeRecordedImpl(string notificationType, long executionTimeMs, bool successful = true)
    {
        // Reuse existing logging infrastructure for notifications
        _mediatorLogger?.ExecutionTimeRecorded($"Notification:{notificationType}", executionTimeMs, successful);
    }

    private void LogNotificationMemoryAllocationRecordedImpl(long bytesAllocated)
    {
        // Reuse existing memory allocation logging for notifications
        _mediatorLogger?.MemoryAllocationRecorded(bytesAllocated);
    }

    private void LogMiddlewareExecutionRecordedImpl(string middlewareType, long executionTimeMs, bool successful)
    {
        _mediatorLogger?.MiddlewareExecutionRecorded(middlewareType, executionTimeMs, successful);
    }

    private void LogExecutionPatternRecordedImpl(string requestType, DateTime executionTime)
    {
        _mediatorLogger?.ExecutionPatternRecorded(requestType, executionTime);
    }

    private void LogStatisticsReportStartedImpl()
    {
        _mediatorLogger?.StatisticsReportStarted();
    }

    private void LogStatisticsReportCompletedImpl()
    {
        _mediatorLogger?.StatisticsReportCompleted();
    }

    private void LogAnalysisStartedImpl(string analysisType)
    {
        // Get the logger and logging options from the MediatorLogger
        if (_mediatorLogger?.Logger != null)
        {
            var logger = _mediatorLogger.Logger;
            var loggingOptions = _mediatorLogger.LoggingOptions;

            if (analysisType == "queries")
            {
                logger.AnalyzeQueriesStarted(loggingOptions, "Statistics Analysis");
            }
            else if (analysisType == "commands")
            {
                logger.AnalyzeCommandsStarted(loggingOptions, "Statistics Analysis");
            }
        }
            
        _mediatorLogger?.AnalysisStarted(analysisType);
    }

    private void LogAnalysisCompletedImpl(string analysisType, int resultCount)
    {
        // Get the logger and logging options from the MediatorLogger
        if (_mediatorLogger?.Logger != null)
        {
            var logger = _mediatorLogger.Logger;
            var loggingOptions = _mediatorLogger.LoggingOptions;

            if (analysisType == "queries")
            {
                logger.AnalyzeQueriesCompleted(loggingOptions, resultCount, _options.EnableDetailedAnalysis);
            }
            else if (analysisType == "commands")
            {
                logger.AnalyzeCommandsCompleted(loggingOptions, resultCount, _options.EnableDetailedAnalysis);
            }
        }
        
        _mediatorLogger?.AnalysisCompleted(analysisType, resultCount);
    }

    private void LogCleanupStartedImpl(int expiredEntries)
    {
        _mediatorLogger?.CleanupStarted(expiredEntries);
    }

    private void LogCleanupCompletedImpl(int removedEntries)
    {
        _mediatorLogger?.CleanupCompleted(removedEntries);
    }

    private void LogStatisticsDisposedImpl()
    {
        _mediatorLogger?.StatisticsDisposed();
    }

    #endregion

    /// <summary>
    /// Gets performance metrics for a specific operation type (request or notification).
    /// </summary>
    /// <param name="operationType">The name of the operation type (e.g., "GetUserQuery" or "Notification:OrderCreated").</param>
    /// <returns>Performance metrics if available, null otherwise.</returns>
    public PerformanceMetrics? GetPerformanceMetrics(string operationType)
    {
        if (!_options.EnablePerformanceCounters || string.IsNullOrEmpty(operationType))
        {
            return null;
        }

        if (!_totalExecutions.TryGetValue(operationType, out var totalExecutions) || totalExecutions == 0)
        {
            return null;
        }

        _totalExecutionTime.TryGetValue(operationType, out var totalTime);
        _failedExecutions.TryGetValue(operationType, out var failures);
        _lastExecutionTimes.TryGetValue(operationType, out var lastExecution);

        var averageTime = totalExecutions > 0 ? (double)totalTime / totalExecutions : 0;
        var successRate = totalExecutions > 0 ? (double)(totalExecutions - failures) / totalExecutions * 100 : 0;

        // Calculate percentiles if we have execution time data
        double p50 = 0, p95 = 0, p99 = 0;
        if (_executionTimes.TryGetValue(operationType, out var times) && times.Count > 0)
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
            operationType,
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
    /// Gets overall performance summary for all operations (requests and notifications) when performance counters are enabled.
    /// </summary>
    /// <returns>Performance summary with overall metrics.</returns>
    public PerformanceSummary? GetPerformanceSummary()
    {
        if (!_options.EnablePerformanceCounters)
        {
            return null;
        }

        var totalOperations = _totalExecutions.Values.Sum();
        var totalFailures = _failedExecutions.Values.Sum();
        var totalTime = _totalExecutionTime.Values.Sum();

        long totalMemory;
        lock (_memoryLock)
        {
            totalMemory = _totalMemoryAllocated;
        }

        var averageTime = totalOperations > 0 ? (double)totalTime / totalOperations : 0;
        var overallSuccessRate = totalOperations > 0 ? (double)(totalOperations - totalFailures) / totalOperations * 100 : 0;

        return new PerformanceSummary(
            totalOperations,
            totalFailures,
            averageTime,
            overallSuccessRate,
            totalMemory,
            _totalExecutions.Count
        );
    }

    /// <summary>
    /// Gets performance summary filtered to show only request operations (non-notification).
    /// </summary>
    /// <returns>Performance summary for request operations only.</returns>
    public PerformanceSummary? GetRequestPerformanceSummary()
    {
        if (!_options.EnablePerformanceCounters)
        {
            return null;
        }

        // Filter to request-only metrics (exclude notification-prefixed keys)
        var requestMetrics = _totalExecutions
            .Where(kvp => !IsNotificationType(kvp.Key))
            .ToList();

        var totalRequests = requestMetrics.Sum(kvp => kvp.Value);
        var totalFailures = requestMetrics.Sum(kvp => _failedExecutions.GetValueOrDefault(kvp.Key, 0));
        var totalTime = requestMetrics.Sum(kvp => _totalExecutionTime.GetValueOrDefault(kvp.Key, 0));

        long totalMemory;
        lock (_memoryLock)
        {
            totalMemory = _totalMemoryAllocated; // Note: Memory is shared between requests/notifications
        }

        var averageTime = totalRequests > 0 ? (double)totalTime / totalRequests : 0;
        var overallSuccessRate = totalRequests > 0 ? (double)(totalRequests - totalFailures) / totalRequests * 100 : 0;

        return new PerformanceSummary(
            totalRequests,
            totalFailures,
            averageTime,
            overallSuccessRate,
            totalMemory,
            requestMetrics.Count
        );
    }

    /// <summary>
    /// Gets performance summary filtered to show only notification operations.
    /// </summary>
    /// <returns>Performance summary for notification operations only.</returns>
    public PerformanceSummary? GetNotificationPerformanceSummary()
    {
        if (!_options.EnablePerformanceCounters)
        {
            return null;
        }

        // Filter to notification-only metrics (include only notification-prefixed keys)
        var notificationMetrics = _totalExecutions
            .Where(kvp => IsNotificationType(kvp.Key))
            .ToList();

        var totalNotifications = notificationMetrics.Sum(kvp => kvp.Value);
        var totalFailures = notificationMetrics.Sum(kvp => _failedExecutions.GetValueOrDefault(kvp.Key, 0));
        var totalTime = notificationMetrics.Sum(kvp => _totalExecutionTime.GetValueOrDefault(kvp.Key, 0));

        // For notifications, we could track separate memory, but for simplicity, we'll share memory tracking
        long totalMemory;
        lock (_memoryLock)
        {
            totalMemory = _totalMemoryAllocated; // Note: Memory is shared between requests/notifications
        }

        var averageTime = totalNotifications > 0 ? (double)totalTime / totalNotifications : 0;
        var overallSuccessRate = totalNotifications > 0 ? (double)(totalNotifications - totalFailures) / totalNotifications * 100 : 0;

        return new PerformanceSummary(
            totalNotifications,
            totalFailures,
            averageTime,
            overallSuccessRate,
            totalMemory,
            notificationMetrics.Count
        );
    }

    /// <summary>
    /// Gets performance metrics for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The name of the notification type (without "Notification:" prefix).</param>
    /// <returns>Performance metrics for the notification type if available, null otherwise.</returns>
    public PerformanceMetrics? GetNotificationPerformanceMetrics(string notificationType)
    {
        if (string.IsNullOrEmpty(notificationType))
        {
            return null;
        }

        // Convert notification type to internal key format
        var internalKey = $"Notification:{notificationType}";
        return GetPerformanceMetrics(internalKey);
    }

    /// <summary>
    /// Determines if an operation type name represents a notification.
    /// </summary>
    /// <param name="operationType">The operation type name to check.</param>
    /// <returns>True if the operation type represents a notification, false otherwise.</returns>
    private static bool IsNotificationType(string operationType)
    {
        return operationType.StartsWith("Notification:", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the display type for an operation (Request or Notification).
    /// </summary>
    /// <param name="operationType">The operation type name.</param>
    /// <returns>"Request" or "Notification" based on the operation type.</returns>
    private static string GetOperationType(string operationType)
    {
        return IsNotificationType(operationType) ? "Notification" : "Request";
    }

    /// <summary>
    /// Records detailed execution pattern when detailed analysis is enabled.
    /// </summary>
    /// <param name="operationType">The operation type name (request or notification).</param>
    /// <param name="executionTime">The execution timestamp.</param>
    public void RecordExecutionPattern(string operationType, DateTime executionTime)
    {
        if (!_options.EnableDetailedAnalysis || string.IsNullOrEmpty(operationType))
        {
            return;
        }

        // Check max tracked request types limit
        if (_executionPatterns.Count >= _options.MaxTrackedRequestTypes &&
            !_executionPatterns.ContainsKey(operationType))
        {
            return; // Don't track new operation types if we've hit the limit
        }

        _executionPatterns.AddOrUpdate(operationType, [executionTime], (_, patterns) =>
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
        var hourKey = $"{operationType}_{executionTime:yyyy-MM-dd-HH}";
        _hourlyExecutionCounts.AddOrUpdate(hourKey, 1, (_, count) => count + 1);

        UpdateMetricTimestamp($"{PatternPrefix}{operationType}");
        
        LogExecutionPatternRecordedImpl(operationType, executionTime);
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
        
        LogMiddlewareExecutionRecordedImpl(middlewareType, executionTimeMs, successful);
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
}

// Extension method to help with concurrent collections
public static class CollectionExtensions
{
    public static ConcurrentHashSet<T> ToConcurrentHashSet<T>(this IEnumerable<T> source)
    {
        var hashSet = new ConcurrentHashSet<T>();
        foreach (var item in source)
        {
            hashSet.Add(item);
        }
        return hashSet;
    }
}

// Simple thread-safe hash set implementation
public class ConcurrentHashSet<T> : IEnumerable<T>
{
    private readonly HashSet<T> _hashSet = new();
    private readonly Lock _lock = new();

    public void Add(T item)
    {
        lock (_lock)
        {
            _hashSet.Add(item);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _hashSet.Clear();
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _hashSet.Count;
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (_lock)
        {
            return _hashSet.ToList().GetEnumerator();
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}