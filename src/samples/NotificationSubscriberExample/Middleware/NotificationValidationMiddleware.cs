using Blazing.Mediator.Abstractions;

namespace NotificationSubscriberExample.Middleware;

/// <summary>
/// Middleware that validates notifications before processing.
/// This demonstrates security and validation concerns in notification processing.
/// </summary>
public class NotificationValidationMiddleware(ILogger<NotificationValidationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 5; // Execute before logging

    public async Task InvokeAsync<TNotification>(TNotification? notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        logger.LogInformation("* Validating notification: {NotificationType}", typeof(TNotification).Name);

        // Simulate validation logic
        if (notification == null)
        {
            logger.LogError("! Validation failed: Notification is null");
            throw new ArgumentNullException(nameof(notification), "Notification cannot be null");
        }

        // Add specific validation for OrderCreatedNotification
        if (notification is OrderCreatedNotification orderNotification)
        {
            if (orderNotification.OrderId <= 0)
            {
                logger.LogError("! Validation failed: OrderId must be positive");
                throw new InvalidOperationException("OrderId must be greater than zero");
            }

            if (orderNotification.TotalAmount <= 0)
            {
                logger.LogError("! Validation failed: TotalAmount must be positive");
                throw new InvalidOperationException("TotalAmount must be greater than zero");
            }

            if (string.IsNullOrWhiteSpace(orderNotification.CustomerEmail))
            {
                logger.LogError("! Validation failed: CustomerEmail is required");
                throw new InvalidOperationException("CustomerEmail is required for OrderCreatedNotification");
            }
        }

        logger.LogInformation("# Validation passed for: {NotificationType}", typeof(TNotification).Name);
        await next(notification, cancellationToken);
    }
}
