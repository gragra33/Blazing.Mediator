namespace TypedNotificationHybridExample.Subscribers;

/// <summary>
/// Integration notification subscriber that implements INotificationSubscriber.
/// Handles external system integrations with manual subscription control.
/// Part of the MANUAL SUBSCRIBERS in the typed hybrid approach.
/// </summary>
public class IntegrationNotificationSubscriber(ILogger<IntegrationNotificationSubscriber> logger)
    : INotificationSubscriber<OrderCreatedNotification>,
      INotificationSubscriber<CustomerRegisteredNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(200, cancellationToken); // Simulate external API call delay

            logger.LogInformation("[MANUAL-SUBSCRIBER] EXTERNAL INTEGRATION - ORDER SYNC");
            logger.LogInformation("   Integration Type: E-Commerce Platform Sync");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
            
            // Simulate integration with external systems
            logger.LogInformation("   - Syncing with ERP System...");
            logger.LogInformation("   - Updating CRM Database...");
            logger.LogInformation("   - Notifying Fulfillment Center...");
            logger.LogInformation("   - Updating Analytics Dashboard...");
            
            // Simulate different integration responses based on order value
            if (notification.TotalAmount > 150)
            {
                logger.LogInformation("   - HIGH VALUE: Flagged in fraud detection system");
                logger.LogInformation("   - PRIORITY PROCESSING: Added to premium fulfillment queue");
            }

            var integrationId = $"INT-ORDER-{notification.OrderId}-{DateTime.UtcNow:HHmmss}";
            logger.LogInformation("   Integration ID: {IntegrationId}", integrationId);
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Order integration completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to integrate order {OrderId}", notification.OrderId);
            // Don't rethrow - integration failures shouldn't break the main pipeline
        }
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(180, cancellationToken); // Simulate external API call delay

            logger.LogInformation("[MANUAL-SUBSCRIBER] EXTERNAL INTEGRATION - CUSTOMER SYNC");
            logger.LogInformation("   Integration Type: CRM and Marketing Platform Sync");
            logger.LogInformation("   Customer: {CustomerName} ({CustomerId})", notification.CustomerName, notification.CustomerId);
            logger.LogInformation("   Email: {CustomerEmail}", notification.CustomerEmail);
            logger.LogInformation("   Source: {RegistrationSource}", notification.RegistrationSource);
            
            // Simulate integration with multiple external systems
            logger.LogInformation("   - Adding to CRM System...");
            logger.LogInformation("   - Creating Marketing Profile...");
            logger.LogInformation("   - Setting up Email Automation...");
            logger.LogInformation("   - Initializing Loyalty Program...");
            
            // Source-specific integrations
            switch (notification.RegistrationSource.ToLower())
            {
                case "website":
                    logger.LogInformation("   - WEB INTEGRATION: Adding to web analytics tracking");
                    break;
                case "mobile":
                    logger.LogInformation("   - MOBILE INTEGRATION: Enabling push notifications");
                    break;
                case "referral":
                    logger.LogInformation("   - REFERRAL INTEGRATION: Processing referral rewards");
                    logger.LogInformation("   - AFFILIATE INTEGRATION: Tracking referral source");
                    break;
            }

            var integrationId = $"INT-CUSTOMER-{notification.CustomerId}-{DateTime.UtcNow:HHmmss}";
            logger.LogInformation("   Integration ID: {IntegrationId}", integrationId);
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Customer integration completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to integrate customer {CustomerId}", notification.CustomerId);
            // Don't rethrow - integration failures shouldn't break the main pipeline
        }
    }
}