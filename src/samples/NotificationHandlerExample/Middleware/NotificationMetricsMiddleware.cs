namespace NotificationHandlerExample.Middleware;

/// <summary>
/// Notification metrics middleware that tracks processing statistics and performance.
/// Demonstrates metrics collection and monitoring in the notification pipeline.
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger) 
    : INotificationMiddleware
{
    private static readonly Dictionary<string, NotificationMetrics> _metrics = new();
    private static readonly object _lock = new();

    public int Order => 300; // Execute after validation but before audit

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        logger.LogInformation("[METRICS] Collecting metrics for: {NotificationType}", notificationType);

        try
        {
            // Execute the next middleware/handlers
            await next(notification, cancellationToken);
            
            stopwatch.Stop();
            RecordSuccess(notificationType, stopwatch.ElapsedMilliseconds);
            
            logger.LogInformation("[+] Metrics recorded: {NotificationType} succeeded in {ElapsedMs}ms", 
                notificationType, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            RecordFailure(notificationType, stopwatch.ElapsedMilliseconds, ex);
            
            logger.LogError("[-] Metrics recorded: {NotificationType} failed in {ElapsedMs}ms", 
                notificationType, stopwatch.ElapsedMilliseconds);
            
            throw; // Re-throw to maintain error handling
        }

        // Log current metrics periodically
        LogMetricsSummary(notificationType);
    }

    private static void RecordSuccess(string notificationType, long elapsedMilliseconds)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(notificationType, out var metrics))
            {
                metrics = new NotificationMetrics(notificationType);
                _metrics[notificationType] = metrics;
            }

            metrics.RecordSuccess(elapsedMilliseconds);
        }
    }

    private static void RecordFailure(string notificationType, long elapsedMilliseconds, Exception exception)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(notificationType, out var metrics))
            {
                metrics = new NotificationMetrics(notificationType);
                _metrics[notificationType] = metrics;
            }

            metrics.RecordFailure(elapsedMilliseconds, exception);
        }
    }

    private void LogMetricsSummary(string notificationType)
    {
        lock (_lock)
        {
            if (_metrics.TryGetValue(notificationType, out var metrics) && 
                (metrics.TotalCount % 5 == 0)) // Log every 5th notification
            {
                logger.LogInformation("[STATS] METRICS SUMMARY for {NotificationType}:", notificationType);
                logger.LogInformation("   Total Processed: {TotalCount}", metrics.TotalCount);
                logger.LogInformation("   Success Rate: {SuccessRate:P2}", metrics.SuccessRate);
                logger.LogInformation("   Avg Duration: {AvgDuration:F2}ms", metrics.AverageDuration);
                logger.LogInformation("   Min Duration: {MinDuration}ms", metrics.MinDuration);
                logger.LogInformation("   Max Duration: {MaxDuration}ms", metrics.MaxDuration);
                
                if (metrics.FailureCount > 0)
                {
                    logger.LogWarning("   Failures: {FailureCount}", metrics.FailureCount);
                    logger.LogWarning("   Common Errors: {CommonErrors}", 
                        string.Join(", ", metrics.GetTopErrors().Take(3)));
                }
            }
        }
    }

    /// <summary>
    /// Gets current metrics for all notification types (for testing/debugging)
    /// </summary>
    internal static IReadOnlyDictionary<string, NotificationMetrics> GetAllMetrics()
    {
        lock (_lock)
        {
            return _metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }

    internal class NotificationMetrics
    {
        private readonly List<long> _durations = new();
        private readonly Dictionary<string, int> _errorCounts = new();

        public string NotificationType { get; }
        public int TotalCount { get; private set; }
        public int SuccessCount { get; private set; }
        public int FailureCount { get; private set; }
        public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount : 0;
        public double AverageDuration => _durations.Count > 0 ? _durations.Average() : 0;
        public long MinDuration => _durations.Count > 0 ? _durations.Min() : 0;
        public long MaxDuration => _durations.Count > 0 ? _durations.Max() : 0;

        public NotificationMetrics(string notificationType)
        {
            NotificationType = notificationType;
        }

        public void RecordSuccess(long durationMs)
        {
            TotalCount++;
            SuccessCount++;
            _durations.Add(durationMs);
        }

        public void RecordFailure(long durationMs, Exception exception)
        {
            TotalCount++;
            FailureCount++;
            _durations.Add(durationMs);

            var errorType = exception.GetType().Name;
            _errorCounts[errorType] = _errorCounts.GetValueOrDefault(errorType, 0) + 1;
        }

        public IEnumerable<string> GetTopErrors()
        {
            return _errorCounts
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => $"{kvp.Key}({kvp.Value})");
        }
    }
}