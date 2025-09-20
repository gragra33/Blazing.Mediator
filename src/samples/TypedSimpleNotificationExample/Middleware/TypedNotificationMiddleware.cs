namespace TypedSimpleNotificationExample.Middleware;

/// <summary>
/// General notification logging middleware for all notifications.
/// This middleware logs all notification processing.
/// </summary>
public class GeneralNotificationLoggingMiddleware(ILogger<GeneralNotificationLoggingMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        logger.LogInformation("* Processing notification: {NotificationName}", notificationName);

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("- Notification completed: {NotificationName} in {Duration:F1}ms",
                notificationName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "! Notification failed: {NotificationName} after {Duration:F1}ms",
                notificationName, duration.TotalMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Order-specific notification middleware.
/// This middleware ONLY processes notifications that implement IOrderNotification.
/// </summary>
public class OrderNotificationMiddleware(ILogger<OrderNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 50;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process order notifications
        if (notification is IOrderNotification orderNotification)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogInformation("* Processing ORDER notification: {NotificationName} for Order {OrderId}",
                notificationName, orderNotification.OrderId);
        }

        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Customer-specific notification middleware.
/// This middleware ONLY processes notifications that implement ICustomerNotification.
/// </summary>
public class CustomerNotificationMiddleware(ILogger<CustomerNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 60;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process customer notifications
        if (notification is ICustomerNotification customerNotification)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogInformation("* Processing CUSTOMER notification: {NotificationName} for {CustomerName}",
                notificationName, customerNotification.CustomerName);
        }

        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Inventory-specific notification middleware.
/// This middleware ONLY processes notifications that implement IInventoryNotification.
/// </summary>
public class InventoryNotificationMiddleware(ILogger<InventoryNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 70;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process inventory notifications
        if (notification is IInventoryNotification inventoryNotification)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogInformation("* Processing INVENTORY notification: {NotificationName} for Product {ProductId} (Qty: {Quantity})",
                notificationName, inventoryNotification.ProductId, inventoryNotification.Quantity);
        }

        await next(notification, cancellationToken);
    }
}

/// <summary>
/// Error handling middleware for notifications.
/// This middleware handles exceptions in notification processing.
/// </summary>
public class NotificationErrorHandlingMiddleware(ILogger<NotificationErrorHandlingMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => int.MinValue; // Execute first

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        try
        {
            await next(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogError(ex, "! Error in notification pipeline for {NotificationName}: {Message}",
                notificationName, ex.Message);

            // Don't rethrow - we want notifications to be resilient
            // In a real system, you might want to store failed notifications for retry
        }
    }
}

/// <summary>
/// Metrics tracking middleware for notifications.
/// This middleware tracks notification performance metrics.
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger)
    : INotificationMiddleware
{
    private static readonly Dictionary<string, NotificationMetrics> _metrics = new();
    private static readonly object _lock = new();

    public int Order => 100;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration, success: true);
        }
        catch (Exception)
        {
            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration, success: false);
            throw;
        }
    }

    private void UpdateMetrics(string notificationName, TimeSpan duration, bool success)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(notificationName))
            {
                _metrics[notificationName] = new NotificationMetrics();
            }

            var metrics = _metrics[notificationName];
            metrics.TotalCount++;
            metrics.TotalDuration += duration;

            if (success)
                metrics.SuccessCount++;
            else
                metrics.FailureCount++;

            // Log metrics periodically
            if (metrics.TotalCount % 5 == 0) // Every 5 notifications
            {
                logger.LogInformation("* Metrics for {NotificationName}: " +
                    "Total: {Total}, Success: {Success}, Failures: {Failures}, Avg: {AvgDuration:F1}ms",
                    notificationName, metrics.TotalCount, metrics.SuccessCount, metrics.FailureCount,
                    metrics.AverageDuration.TotalMilliseconds);
            }
        }
    }

    public static Dictionary<string, NotificationMetrics> GetMetrics()
    {
        lock (_lock)
        {
            return new Dictionary<string, NotificationMetrics>(_metrics);
        }
    }
}

/// <summary>
/// Represents notification metrics.
/// </summary>
public class NotificationMetrics
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration => TotalCount > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / TotalCount) : TimeSpan.Zero;
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
}