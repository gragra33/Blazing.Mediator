namespace TypedNotificationSubscriberExample.Subscribers;

/// <summary>
/// Business operations handler that processes order notifications for business logic.
/// This handler demonstrates cross-cutting business concerns.
/// </summary>
public class BusinessOperationsHandler(ILogger<BusinessOperationsHandler> logger) :
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<CustomerRegisteredNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(30, cancellationToken); // Simulate business processing

        logger.LogInformation("* BUSINESS OPERATIONS UPDATE");
        logger.LogInformation("   Order #{OrderId} processed for business metrics", notification.OrderId);
        logger.LogInformation("   Revenue: ${TotalAmount:F2} recorded", notification.TotalAmount);

        // Calculate some business metrics
        var averageItemValue = notification.TotalAmount / notification.Items.Count;
        logger.LogInformation("   Average item value: ${AverageValue:F2}", averageItemValue);
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(30, cancellationToken); // Simulate business processing

        logger.LogInformation("* NEW CUSTOMER ONBOARDING");
        logger.LogInformation("   Customer: {CustomerName} ({CustomerEmail})",
            notification.CustomerName, notification.CustomerEmail);
        logger.LogInformation("   Customer database updated for analytics");
    }
}