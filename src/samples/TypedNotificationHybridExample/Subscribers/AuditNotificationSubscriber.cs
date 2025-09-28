namespace TypedNotificationHybridExample.Subscribers;

/// <summary>
/// Audit notification subscriber that implements INotificationSubscriber.
/// Handles audit logging for multiple notification types with explicit subscription control.
/// Part of the MANUAL SUBSCRIBERS in the typed hybrid approach.
/// </summary>
public class AuditNotificationSubscriber(ILogger<AuditNotificationSubscriber> logger)
    : INotificationSubscriber<OrderCreatedNotification>,
      INotificationSubscriber<CustomerRegisteredNotification>,
      INotificationSubscriber<InventoryUpdatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(60, cancellationToken);

            logger.LogInformation("[MANUAL-SUBSCRIBER] AUDIT TRAIL - ORDER EVENT");
            logger.LogInformation("   Event Type: ORDER_CREATED");
            logger.LogInformation("   Order ID: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName} ({CustomerEmail})", 
                notification.CustomerName, notification.CustomerEmail);
            logger.LogInformation("   Order Total: ${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("   Items Count: {ItemCount}", notification.Items.Count);
            
            var auditId = GenerateAuditId("ORDER", notification.OrderId);
            logger.LogInformation("   Audit ID: {AuditId}", auditId);
            logger.LogInformation("   Compliance: Financial transaction recorded");
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Order audit completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to audit order {OrderId}", notification.OrderId);
        }
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(50, cancellationToken);

            logger.LogInformation("[MANUAL-SUBSCRIBER] AUDIT TRAIL - CUSTOMER EVENT");
            logger.LogInformation("   Event Type: CUSTOMER_REGISTERED");
            logger.LogInformation("   Customer ID: {CustomerId}", notification.CustomerId);
            logger.LogInformation("   Customer: {CustomerName} ({CustomerEmail})", 
                notification.CustomerName, notification.CustomerEmail);
            logger.LogInformation("   Registration Source: {RegistrationSource}", notification.RegistrationSource);
            logger.LogInformation("   Registration Time: {RegisteredAt:yyyy-MM-dd HH:mm:ss} UTC", notification.RegisteredAt);
            
            var auditId = GenerateAuditId("CUSTOMER", notification.CustomerId);
            logger.LogInformation("   Audit ID: {AuditId}", auditId);
            logger.LogInformation("   Compliance: GDPR consent and data protection applied");
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Customer audit completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to audit customer {CustomerId}", notification.CustomerId);
        }
    }

    public async Task OnNotification(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(40, cancellationToken);

            logger.LogInformation("[MANUAL-SUBSCRIBER] AUDIT TRAIL - INVENTORY EVENT");
            logger.LogInformation("   Event Type: INVENTORY_UPDATED");
            logger.LogInformation("   Product ID: {ProductId}", notification.ProductId);
            logger.LogInformation("   Product: {ProductName}", notification.ProductName);
            logger.LogInformation("   Quantity Change: {PreviousQuantity} ? {NewQuantity}", 
                notification.PreviousQuantity, notification.NewQuantity);
            logger.LogInformation("   Update Reason: {UpdateReason}", notification.UpdateReason);
            logger.LogInformation("   Update Time: {UpdatedAt:yyyy-MM-dd HH:mm:ss} UTC", notification.UpdatedAt);
            
            var auditId = GenerateAuditId("INVENTORY", notification.ProductId);
            logger.LogInformation("   Audit ID: {AuditId}", auditId);
            logger.LogInformation("   Compliance: Inventory valuation and tax implications tracked");
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Inventory audit completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to audit inventory {ProductId}", notification.ProductId);
        }
    }

    private static string GenerateAuditId(string eventType, string entityId)
    {
        return $"AUDIT-{eventType}-{DateTime.UtcNow:yyyyMMdd}-{entityId}-{Random.Shared.Next(1000, 9999)}";
    }
}