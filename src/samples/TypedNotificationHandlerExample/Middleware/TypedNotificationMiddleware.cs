namespace TypedNotificationHandlerExample.Middleware;

/// <summary>
/// General notification logging middleware for all notifications.
/// This middleware logs all notification processing with automatic handler discovery.
/// Order: 100 (runs first for all notifications)
/// </summary>
public class NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 100;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        var middlewareName = GetType().Name;
        
        logger.LogDebug(">> [{MiddlewareName}] Starting processing for {NotificationType}", middlewareName, notificationType);
        
        Console.WriteLine($">> [{middlewareName}] Processing {notificationType}");
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await next(notification, cancellationToken);
            stopwatch.Stop();
            
            logger.LogDebug("* [{MiddlewareName}] Completed {NotificationType} in {ElapsedMs}ms", 
                middlewareName, notificationType, stopwatch.ElapsedMilliseconds);
                
            Console.WriteLine($"* [{middlewareName}] Completed in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogError(ex, "* [{MiddlewareName}] Failed processing {NotificationType} after {ElapsedMs}ms", 
                middlewareName, notificationType, stopwatch.ElapsedMilliseconds);
                
            Console.WriteLine($"* [{middlewareName}] Failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// Type-constrained middleware for order-related notifications only.
/// This middleware runs only for notifications implementing IOrderNotification.
/// Order: 200 (runs after general logging for order notifications)
/// </summary>
public class OrderNotificationMiddleware(ILogger<OrderNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 200;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process order notifications
        if (notification is IOrderNotification orderNotification)
        {
            var notificationType = typeof(TNotification).Name;
            
            logger.LogDebug(">>> [OrderMiddleware] Processing order notification: {NotificationType} for Order #{OrderId}", 
                notificationType, orderNotification.OrderId);
                
            Console.WriteLine($">>> [OrderMiddleware] Processing Order #{orderNotification.OrderId} ({notificationType})");
            Console.WriteLine($"   Customer: {orderNotification.CustomerEmail}");
            
            // Simulate order-specific validation
            if (orderNotification.OrderId <= 0)
            {
                var error = $"Invalid Order ID: {orderNotification.OrderId}";
                logger.LogError("* [OrderMiddleware] {Error}", error);
                Console.WriteLine($"* [OrderMiddleware] {error}");
                throw new ArgumentException(error);
            }
            
            try
            {
                await next(notification, cancellationToken);
                
                logger.LogDebug("* [OrderMiddleware] Order notification processing completed for Order #{OrderId}", 
                    orderNotification.OrderId);
                    
                Console.WriteLine($"* [OrderMiddleware] Order processing completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "* [OrderMiddleware] Order notification processing failed for Order #{OrderId}", 
                    orderNotification.OrderId);
                
                Console.WriteLine($"* [OrderMiddleware] Order processing failed: {ex.Message}");
                throw;
            }
        }
        else
        {
            // Not an order notification, just continue
            await next(notification, cancellationToken);
        }
    }
}

/// <summary>
/// Type-constrained middleware for customer-related notifications only.
/// This middleware runs only for notifications implementing ICustomerNotification.
/// Order: 250 (runs after order middleware for customer notifications)
/// </summary>
public class CustomerNotificationMiddleware(ILogger<CustomerNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 250;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process customer notifications
        if (notification is ICustomerNotification customerNotification)
        {
            var notificationType = typeof(TNotification).Name;
            
            logger.LogDebug(">> [CustomerMiddleware] Processing customer notification: {NotificationType} for {CustomerEmail}", 
                notificationType, customerNotification.CustomerEmail);
                
            Console.WriteLine($">> [CustomerMiddleware] Processing customer event ({notificationType})");
            Console.WriteLine($"   Customer: {customerNotification.CustomerName} ({customerNotification.CustomerEmail})");
            
            // Simulate customer-specific validation
            if (string.IsNullOrWhiteSpace(customerNotification.CustomerEmail) || !customerNotification.CustomerEmail.Contains('@'))
            {
                var error = $"Invalid customer email: {customerNotification.CustomerEmail}";
                logger.LogError("* [CustomerMiddleware] {Error}", error);
                Console.WriteLine($"* [CustomerMiddleware] {error}");
                throw new ArgumentException(error);
            }
            
            try
            {
                await next(notification, cancellationToken);
                
                logger.LogDebug("* [CustomerMiddleware] Customer notification processing completed for {CustomerEmail}", 
                    customerNotification.CustomerEmail);
                    
                Console.WriteLine($"* [CustomerMiddleware] Customer processing completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "* [CustomerMiddleware] Customer notification processing failed for {CustomerEmail}", 
                    customerNotification.CustomerEmail);
                
                Console.WriteLine($"* [CustomerMiddleware] Customer processing failed: {ex.Message}");
                throw;
            }
        }
        else
        {
            // Not a customer notification, just continue
            await next(notification, cancellationToken);
        }
    }
}

/// <summary>
/// Type-constrained middleware for inventory-related notifications only.
/// This middleware runs only for notifications implementing IInventoryNotification.
/// Order: 300 (runs after customer middleware for inventory notifications)
/// </summary>
public class InventoryNotificationMiddleware(ILogger<InventoryNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 300;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process inventory notifications
        if (notification is IInventoryNotification inventoryNotification)
        {
            var notificationType = typeof(TNotification).Name;
            
            logger.LogDebug(">> [InventoryMiddleware] Processing inventory notification: {NotificationType} for {ProductId}", 
                notificationType, inventoryNotification.ProductId);
                
            Console.WriteLine($">> [InventoryMiddleware] Processing inventory event ({notificationType})");
            Console.WriteLine($"   Product: {inventoryNotification.ProductId} (Quantity: {inventoryNotification.Quantity})");
            
            // Simulate inventory-specific validation
            if (string.IsNullOrWhiteSpace(inventoryNotification.ProductId))
            {
                var error = $"Invalid product ID: {inventoryNotification.ProductId}";
                logger.LogError("* [InventoryMiddleware] {Error}", error);
                Console.WriteLine($"* [InventoryMiddleware] {error}");
                throw new ArgumentException(error);
            }
            
            if (inventoryNotification.Quantity < 0)
            {
                logger.LogWarning(">> [InventoryMiddleware] Negative inventory quantity detected: {Quantity} for {ProductId}", 
                    inventoryNotification.Quantity, inventoryNotification.ProductId);
                Console.WriteLine($">> [InventoryMiddleware] Warning: Negative quantity {inventoryNotification.Quantity}");
            }
            
            try
            {
                await next(notification, cancellationToken);
                
                logger.LogDebug("* [InventoryMiddleware] Inventory notification processing completed for {ProductId}", 
                    inventoryNotification.ProductId);
                    
                Console.WriteLine($"* [InventoryMiddleware] Inventory processing completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "* [InventoryMiddleware] Inventory notification processing failed for {ProductId}", 
                    inventoryNotification.ProductId);
                
                Console.WriteLine($"* [InventoryMiddleware] Inventory processing failed: {ex.Message}");
                throw;
            }
        }
        else
        {
            // Not an inventory notification, just continue
            await next(notification, cancellationToken);
        }
    }
}

/// <summary>
/// Performance monitoring middleware for all notifications.
/// This middleware tracks performance metrics for automatic handler discovery.
/// Order: 400 (runs last to capture total execution time)
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger)
    : INotificationMiddleware
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (long Count, long TotalMs, long MinMs, long MaxMs)> _metrics = new();

    public int Order => 400;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await next(notification, cancellationToken);
            stopwatch.Stop();
            
            // Update metrics
            var elapsed = stopwatch.ElapsedMilliseconds;
            _metrics.AddOrUpdate(notificationType, 
                (1, elapsed, elapsed, elapsed),
                (key, existing) => (
                    existing.Count + 1, 
                    existing.TotalMs + elapsed, 
                    Math.Min(existing.MinMs, elapsed), 
                    Math.Max(existing.MaxMs, elapsed)
                ));
            
            var current = _metrics[notificationType];
            var avgMs = current.TotalMs / current.Count;
            
            logger.LogDebug(">> [MetricsMiddleware] {NotificationType}: {ElapsedMs}ms (Avg: {AvgMs}ms, Count: {Count})", 
                notificationType, elapsed, avgMs, current.Count);
                
            Console.WriteLine($">> [MetricsMiddleware] {notificationType}: {elapsed}ms (Avg: {avgMs}ms, Total: {current.Count})");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogError(ex, ">> [MetricsMiddleware] {NotificationType} failed after {ElapsedMs}ms", 
                notificationType, stopwatch.ElapsedMilliseconds);
                
            Console.WriteLine($">> [MetricsMiddleware] {notificationType} failed after {stopwatch.ElapsedMilliseconds}ms");
            throw;
        }
    }

    /// <summary>
    /// Gets current performance metrics for all processed notifications.
    /// </summary>
    public static IReadOnlyDictionary<string, (long Count, long TotalMs, long MinMs, long MaxMs, double AvgMs)> GetMetrics()
    {
        return _metrics.ToDictionary(
            kvp => kvp.Key, 
            kvp => (kvp.Value.Count, kvp.Value.TotalMs, kvp.Value.MinMs, kvp.Value.MaxMs, (double)kvp.Value.TotalMs / kvp.Value.Count)
        );
    }
}