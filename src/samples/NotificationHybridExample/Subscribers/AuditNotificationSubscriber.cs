namespace NotificationHybridExample.Subscribers;

/// <summary>
/// Audit notification subscriber that implements INotificationSubscriber.
/// This subscriber handles compliance logging and audit trail management.
/// Part of the MANUAL SUBSCRIBERS in the hybrid approach - requires manual subscription.
/// </summary>
public class AuditNotificationSubscriber(ILogger<AuditNotificationSubscriber> logger)
    : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate audit processing delay
            await Task.Delay(80, cancellationToken);

            logger.LogInformation("[MANUAL-SUBSCRIBER] AUDIT TRAIL LOGGING");
            logger.LogInformation("   Event Type: ORDER_CREATED");
            logger.LogInformation("   Order ID: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName} ({CustomerEmail})", 
                notification.CustomerName, notification.CustomerEmail);
            logger.LogInformation("   Order Total: ${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("   Timestamp: {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC", notification.CreatedAt);
            
            // Generate audit ID
            var auditId = $"AUDIT-{DateTime.UtcNow:yyyyMMdd}-{notification.OrderId}-{Random.Shared.Next(1000, 9999)}";
            logger.LogInformation("   Audit ID: {AuditId}", auditId);
            
            // Log compliance information
            logger.LogInformation("   Compliance Check: PASSED");
            logger.LogInformation("   Data Retention: 7 years (regulatory requirement)");
            logger.LogInformation("   Privacy Status: Customer data encrypted and secured");
            
            // Detailed item audit
            logger.LogInformation("   Items Audit:");
            foreach (var item in notification.Items)
            {
                logger.LogInformation("     - Product: {ProductName}, Qty: {Quantity}, Unit Price: ${UnitPrice:F2}",
                    item.ProductName, item.Quantity, item.UnitPrice);
            }

            logger.LogInformation("   Audit Trail: Saved to compliance database");
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Audit logging completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to create audit trail for order {OrderId}", notification.OrderId);
            // Don't rethrow - audit failure shouldn't break the entire notification pipeline
        }
    }
}