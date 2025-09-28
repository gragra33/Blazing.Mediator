namespace TypedNotificationHybridExample.Middleware;

/// <summary>
/// Order-specific notification middleware that only processes IOrderNotification types.
/// Demonstrates type-constrained middleware in the hybrid pattern.
/// </summary>
public class OrderNotificationMiddleware(ILogger<OrderNotificationMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 100; // Execute first for order notifications

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Runtime check for IOrderNotification interface
        if (notification is IOrderNotification)
        {
            logger.LogInformation(">>> [ORDER-MIDDLEWARE] Processing order notification");
            
            if (notification is OrderCreatedNotification orderCreated)
            {
                logger.LogInformation("   Order ID: #{OrderId}", orderCreated.OrderId);
                logger.LogInformation("   Customer: {CustomerName}", orderCreated.CustomerName);
                logger.LogInformation("   Total: ${TotalAmount:F2}", orderCreated.TotalAmount);
                
                // Order validation logic
                if (orderCreated.TotalAmount <= 0)
                {
                    logger.LogWarning("   VALIDATION WARNING: Order has invalid total amount");
                }
                
                if (orderCreated.Items.Count == 0)
                {
                    logger.LogWarning("   VALIDATION WARNING: Order has no items");
                }
            }

            var startTime = DateTime.UtcNow;
            try
            {
                await next(notification, cancellationToken);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogInformation(">>> [ORDER-MIDDLEWARE] Completed successfully in {Duration:F2}ms", duration);
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogError(ex, ">>> [ORDER-MIDDLEWARE] Failed after {Duration:F2}ms: {ErrorMessage}", duration, ex.Message);
                throw;
            }
        }
        else
        {
            // Not an order notification, pass through without processing
            await next(notification, cancellationToken);
        }
    }
}