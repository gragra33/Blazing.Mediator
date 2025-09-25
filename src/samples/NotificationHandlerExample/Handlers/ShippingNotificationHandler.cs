namespace NotificationHandlerExample.Handlers;

/// <summary>
/// Shipping notification handler that implements INotificationHandler for automatic discovery.
/// This handler manages shipping and fulfillment processes when orders are created.
/// Demonstrates fourth handler for the same notification, showing scalable handler architecture.
/// </summary>
public class ShippingNotificationHandler(ILogger<ShippingNotificationHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>
{
    private static readonly string[] ShippingMethods = ["Standard", "Express", "Next Day", "Same Day"];
    private static readonly string[] Carriers = ["FedEx", "UPS", "DHL", "USPS"];

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("[SHIPPING] SHIPPING PROCESSING STARTED");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);

            // Determine shipping method based on order value and item count
            var shippingMethod = DetermineShippingMethod(notification);
            var carrier = SelectCarrier(shippingMethod);
            var estimatedDelivery = CalculateDeliveryDate(shippingMethod);

            logger.LogInformation("   [INFO] Shipping Details:");
            logger.LogInformation("      Method: {ShippingMethod}", shippingMethod);
            logger.LogInformation("      Carrier: {Carrier}", carrier);
            logger.LogInformation("      Est. Delivery: {EstimatedDelivery:yyyy-MM-dd}", estimatedDelivery);

            // Process shipping label creation
            await CreateShippingLabel(notification, shippingMethod, carrier, cancellationToken);

            // Update shipping status
            await UpdateShippingStatus(notification.OrderId, "PROCESSING", cancellationToken);

            logger.LogInformation("[+] Shipping processing completed for Order #{OrderId}", notification.OrderId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] Failed to process shipping for order {OrderId}", notification.OrderId);
            // Shipping failures might need special handling - could trigger manual review
            // For this demo, we'll continue with other handlers
        }
    }

    private string DetermineShippingMethod(OrderCreatedNotification notification)
    {
        // Business logic to determine shipping method
        return notification.TotalAmount switch
        {
            > 500 => "Express", // Free express shipping for high-value orders
            > 100 => "Standard", // Standard shipping for medium orders
            _ when notification.Items.Count > 5 => "Standard", // Bulk orders get standard
            _ => "Standard" // Default to standard
        };
    }

    private string SelectCarrier(string shippingMethod)
    {
        // Simple carrier selection logic
        return shippingMethod switch
        {
            "Same Day" => "DHL",
            "Next Day" => "FedEx",
            "Express" => "UPS",
            "Standard" => Carriers[Random.Shared.Next(Carriers.Length)],
            _ => "USPS"
        };
    }

    private DateTime CalculateDeliveryDate(string shippingMethod)
    {
        var baseDays = shippingMethod switch
        {
            "Same Day" => 0,
            "Next Day" => 1,
            "Express" => 2,
            "Standard" => Random.Shared.Next(3, 8), // 3-7 business days
            _ => 5
        };

        var deliveryDate = DateTime.Now.AddDays(baseDays);
        
        // Skip weekends for business days calculation
        while (deliveryDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            deliveryDate = deliveryDate.AddDays(1);
        }

        return deliveryDate;
    }

    private async Task CreateShippingLabel(OrderCreatedNotification notification, string shippingMethod, 
        string carrier, CancellationToken cancellationToken)
    {
        // Simulate shipping label creation time
        await Task.Delay(75, cancellationToken);

        var trackingNumber = GenerateTrackingNumber(carrier);
        
        logger.LogInformation("   [LABEL] Shipping label created:");
        logger.LogInformation("      Tracking: {TrackingNumber}", trackingNumber);
        logger.LogInformation("      Dimensions: {Dimensions}", CalculatePackageDimensions(notification.Items));
        logger.LogInformation("      Weight: {Weight} lbs", CalculatePackageWeight(notification.Items));
        
        // Simulate printing/digital delivery of label
        logger.LogInformation("   [OK] Label ready for pickup/dropoff");
    }

    private async Task UpdateShippingStatus(string orderId, string status, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        logger.LogInformation("   [STATUS] Shipping status updated: {Status}", status);
    }

    private static string GenerateTrackingNumber(string carrier)
    {
        var prefix = carrier switch
        {
            "FedEx" => "1Z",
            "UPS" => "1Z",
            "DHL" => "DH",
            "USPS" => "US",
            _ => "TK"
        };
        
        var randomNumber = Random.Shared.Next(100000000, 999999999);
        return $"{prefix}{randomNumber}";
    }

    private static string CalculatePackageDimensions(List<OrderItem> items)
    {
        // Simulate package dimension calculation based on items
        var length = Math.Min(24, items.Count * 2 + 6);
        var width = Math.Min(18, items.Count * 1.5 + 4);
        var height = Math.Min(12, items.Sum(i => i.Quantity) + 2);
        
        return $"{length}x{width}x{height}";
    }

    private static double CalculatePackageWeight(List<OrderItem> items)
    {
        // Simulate weight calculation - assume average item weight
        var baseWeight = 0.5; // Base packaging weight
        var itemWeight = items.Sum(i => i.Quantity * 0.3); // Assume 0.3 lbs per item
        
        return Math.Round(baseWeight + itemWeight, 1);
    }
}