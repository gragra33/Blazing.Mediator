namespace NotificationHandlerExample.Handlers;

/// <summary>
/// Audit notification handler that implements INotificationHandler for automatic discovery.
/// This handler logs all order creation events for compliance and auditing purposes.
/// Demonstrates a third handler for the same notification, showing multiple handlers working together.
/// </summary>
public class AuditNotificationHandler(ILogger<AuditNotificationHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate audit logging processing time
            await Task.Delay(30, cancellationToken);

            logger.LogInformation("[AUDIT] AUDIT LOG: Order Created");
            logger.LogInformation("   Event: ORDER_CREATED");
            logger.LogInformation("   Timestamp: {CreatedAt:yyyy-MM-dd HH:mm:ss.fff}", notification.CreatedAt);
            logger.LogInformation("   Order ID: {OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName} ({CustomerEmail})", 
                notification.CustomerName, notification.CustomerEmail);
            logger.LogInformation("   Total Amount: ${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("   Items Count: {ItemCount}", notification.Items.Count);
            
            // Log item details for audit trail
            var itemIndex = 1;
            foreach (var item in notification.Items)
            {
                logger.LogInformation("   Item {ItemIndex}: {ProductName} | Qty: {Quantity} | Price: ${UnitPrice:F2} | Total: ${TotalPrice:F2}",
                    itemIndex++, item.ProductName, item.Quantity, item.UnitPrice, item.TotalPrice);
            }

            // Simulate compliance checks
            await PerformComplianceChecks(notification, cancellationToken);
            
            logger.LogInformation("[+] Audit logging completed for Order #{OrderId}", notification.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] Failed to process audit log for order {OrderId}", notification.OrderId);
            // Critical audit failures should be handled carefully
            // For demo purposes, we'll log and continue, but in real scenarios you might want different handling
        }
    }

    private async Task PerformComplianceChecks(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        await Task.Delay(20, cancellationToken);

        var checks = new List<string>();

        // Simulate various compliance checks
        if (notification.TotalAmount > 1000)
        {
            checks.Add("HIGH_VALUE_ORDER");
            logger.LogInformation("   [CHECK] Compliance: High value order flagged for review");
        }

        if (notification.Items.Count > 10)
        {
            checks.Add("BULK_ORDER");
            logger.LogInformation("   [CHECK] Compliance: Bulk order requires additional verification");
        }

        if (notification.CustomerEmail.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
            checks.Add("TEST_ORDER");
            logger.LogInformation("   [CHECK] Compliance: Test order identified");
        }

        // Log compliance summary
        if (checks.Count > 0)
        {
            logger.LogInformation("   [STATS] Compliance flags: [{Flags}]", string.Join(", ", checks));
        }
        else
        {
            logger.LogInformation("   [+] No compliance issues detected");
        }
    }
}