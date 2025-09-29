namespace TypedNotificationHybridExample.Middleware;

/// <summary>
/// Customer-specific notification middleware that only processes ICustomerNotification types.
/// Demonstrates type-constrained middleware in the hybrid pattern.
/// </summary>
public class CustomerNotificationMiddleware(ILogger<CustomerNotificationMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 200; // Execute after order middleware

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Runtime check for ICustomerNotification interface
        if (notification is ICustomerNotification)
        {
            logger.LogInformation(">>> [CUSTOMER-MIDDLEWARE] Processing customer notification");
            
            if (notification is CustomerRegisteredNotification customerRegistered)
            {
                logger.LogInformation("   Customer ID: {CustomerId}", customerRegistered.CustomerId);
                logger.LogInformation("   Customer Name: {CustomerName}", customerRegistered.CustomerName);
                logger.LogInformation("   Email: {CustomerEmail}", customerRegistered.CustomerEmail);
                logger.LogInformation("   Registration Source: {RegistrationSource}", customerRegistered.RegistrationSource);
                
                // Customer validation logic
                if (string.IsNullOrWhiteSpace(customerRegistered.CustomerEmail))
                {
                    logger.LogWarning("   VALIDATION WARNING: Customer has no email address");
                }
                
                if (string.IsNullOrWhiteSpace(customerRegistered.CustomerName))
                {
                    logger.LogWarning("   VALIDATION WARNING: Customer has no name");
                }
            }

            var startTime = DateTime.UtcNow;
            try
            {
                await next(notification, cancellationToken);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogInformation(">>> [CUSTOMER-MIDDLEWARE] Completed successfully in {Duration:F2}ms", duration);
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogError(ex, ">>> [CUSTOMER-MIDDLEWARE] Failed after {Duration:F2}ms: {ErrorMessage}", duration, ex.Message);
                throw;
            }
        }
        else
        {
            // Not a customer notification, pass through without processing
            await next(notification, cancellationToken);
        }
    }
}