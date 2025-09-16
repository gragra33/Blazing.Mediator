using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace ECommerce.Api.Application.Middleware;

/// <summary>
/// Middleware that logs all notifications published through the mediator.
/// This middleware provides centralized logging and monitoring of all domain events.
/// </summary>
public class NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware> logger)
    : INotificationMiddleware
{
    /// <summary>
    /// Processes a notification, logging it before and after execution.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <param name="notification">The notification to process.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            logger.LogInformation("🔔 NOTIFICATION PUBLISHING: {NotificationName}", notificationName);
            logger.LogDebug("   Notification Data: {@Notification}", notification);
            logger.LogDebug("   Started At: {StartTime:yyyy-MM-dd HH:mm:ss.fff} UTC", startTime);

            // Call the next middleware in the pipeline
            await next(notification, cancellationToken);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            logger.LogInformation("✅ NOTIFICATION COMPLETED: {NotificationName}", notificationName);
            logger.LogDebug("   Duration: {Duration:F2}ms", duration.TotalMilliseconds);
            logger.LogDebug("   Completed At: {EndTime:yyyy-MM-dd HH:mm:ss.fff} UTC", endTime);
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            logger.LogError(ex, "❌ NOTIFICATION FAILED: {NotificationName}", notificationName);
            logger.LogError("   Duration: {Duration:F2}ms", duration.TotalMilliseconds);
            logger.LogError("   Failed At: {EndTime:yyyy-MM-dd HH:mm:ss.fff} UTC", endTime);
            logger.LogError("   Error: {ErrorMessage}", ex.Message);

            // Re-throw the exception to maintain the error flow
            throw;
        }
    }
}

/// <summary>
/// Middleware that tracks notification metrics and performance.
/// This middleware provides performance monitoring and metrics collection for notifications.
/// </summary>
public class NotificationMetricsMiddleware : INotificationMiddleware
{
    private readonly ILogger<NotificationMetricsMiddleware> _logger;
    private static readonly Dictionary<string, NotificationMetrics> _metrics = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the NotificationMetricsMiddleware.
    /// </summary>
    /// <param name="logger">The logger instance for logging metrics.</param>
    public NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes a notification, tracking metrics and performance.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <param name="notification">The notification to process.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            // Call the next middleware in the pipeline
            await next(notification, cancellationToken);

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Update metrics
            UpdateMetrics(notificationName, duration.TotalMilliseconds, true);

            // Log metrics every 10 notifications
            if (GetTotalCount(notificationName) % 10 == 0)
            {
                LogMetrics(notificationName);
            }
        }
        catch (Exception)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Update metrics with failure
            UpdateMetrics(notificationName, duration.TotalMilliseconds, false);

            // Re-throw the exception to maintain the error flow
            throw;
        }
    }

    /// <summary>
    /// Updates the metrics for a notification.
    /// </summary>
    /// <param name="notificationName">The name of the notification.</param>
    /// <param name="durationMs">The duration in milliseconds.</param>
    /// <param name="success">Whether the notification was successful.</param>
    private void UpdateMetrics(string notificationName, double durationMs, bool success)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(notificationName))
            {
                _metrics[notificationName] = new NotificationMetrics();
            }

            var metrics = _metrics[notificationName];
            metrics.TotalCount++;
            metrics.TotalDurationMs += durationMs;

            if (success)
            {
                metrics.SuccessCount++;
            }
            else
            {
                metrics.FailureCount++;
            }

            if (durationMs > metrics.MaxDurationMs)
            {
                metrics.MaxDurationMs = durationMs;
            }

            if (durationMs < metrics.MinDurationMs || metrics.MinDurationMs == 0)
            {
                metrics.MinDurationMs = durationMs;
            }
        }
    }

    /// <summary>
    /// Gets the total count for a notification.
    /// </summary>
    /// <param name="notificationName">The name of the notification.</param>
    /// <returns>The total count.</returns>
    private int GetTotalCount(string notificationName)
    {
        lock (_lock)
        {
            return _metrics.ContainsKey(notificationName) ? _metrics[notificationName].TotalCount : 0;
        }
    }

    /// <summary>
    /// Logs the metrics for a notification.
    /// </summary>
    /// <param name="notificationName">The name of the notification.</param>
    private void LogMetrics(string notificationName)
    {
        lock (_lock)
        {
            if (_metrics.ContainsKey(notificationName))
            {
                var metrics = _metrics[notificationName];
                var avgDuration = metrics.TotalCount > 0 ? metrics.TotalDurationMs / metrics.TotalCount : 0;
                var successRate = metrics.TotalCount > 0 ? (metrics.SuccessCount * 100.0) / metrics.TotalCount : 0;

                _logger.LogInformation("📊 NOTIFICATION METRICS: {NotificationName}", notificationName);
                _logger.LogInformation("   Total Count: {TotalCount}", metrics.TotalCount);
                _logger.LogInformation("   Success Count: {SuccessCount}", metrics.SuccessCount);
                _logger.LogInformation("   Failure Count: {FailureCount}", metrics.FailureCount);
                _logger.LogInformation("   Success Rate: {SuccessRate:F1}%", successRate);
                _logger.LogInformation("   Average Duration: {AvgDuration:F2}ms", avgDuration);
                _logger.LogInformation("   Min Duration: {MinDuration:F2}ms", metrics.MinDurationMs);
                _logger.LogInformation("   Max Duration: {MaxDuration:F2}ms", metrics.MaxDurationMs);
            }
        }
    }

    /// <summary>
    /// Gets the current metrics for all notifications.
    /// </summary>
    /// <returns>A dictionary of notification metrics.</returns>
    public static Dictionary<string, NotificationMetrics> GetAllMetrics()
    {
        lock (_lock)
        {
            return new Dictionary<string, NotificationMetrics>(_metrics);
        }
    }

    /// <summary>
    /// Resets all metrics.
    /// </summary>
    public static void ResetMetrics()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }
    }

    /// <summary>
    /// Metrics data for a notification.
    /// </summary>
    public class NotificationMetrics
    {
        /// <summary>
        /// Gets or sets the total count of notifications processed.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the count of successful notifications.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Gets or sets the count of failed notifications.
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Gets or sets the total duration in milliseconds.
        /// </summary>
        public double TotalDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the minimum duration in milliseconds.
        /// </summary>
        public double MinDurationMs { get; set; }

        /// <summary>
        /// Gets or sets the maximum duration in milliseconds.
        /// </summary>
        public double MaxDurationMs { get; set; }
    }
}
