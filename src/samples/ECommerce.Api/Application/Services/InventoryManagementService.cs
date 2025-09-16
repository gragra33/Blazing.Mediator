using Blazing.Mediator;
using ECommerce.Api.Application.Notifications;

namespace ECommerce.Api.Application.Services;

/// <summary>
/// Background service that handles inventory management notifications.
/// This service monitors stock levels and handles reorder alerts, demonstrating
/// the observer pattern for inventory management in an e-commerce system.
/// </summary>
public class InventoryManagementService : BackgroundService,
    INotificationSubscriber<ProductStockLowNotification>,
    INotificationSubscriber<ProductOutOfStockNotification>,
    INotificationSubscriber<OrderCreatedNotification>
{
    private readonly ILogger<InventoryManagementService> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the InventoryManagementService.
    /// </summary>
    /// <param name="logger">The logger instance for logging inventory operations.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public InventoryManagementService(ILogger<InventoryManagementService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Handles low stock notifications by triggering reorder alerts.
    /// </summary>
    /// <param name="notification">The low stock notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnNotification(ProductStockLowNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate inventory system processing delay
            await Task.Delay(50, cancellationToken);

            _logger.LogWarning("⚠️  LOW STOCK ALERT");
            _logger.LogWarning("   Product: {ProductName} (ID: {ProductId})", notification.ProductName, notification.ProductId);
            _logger.LogWarning("   Current Stock: {CurrentStock}", notification.CurrentStock);
            _logger.LogWarning("   Minimum Threshold: {MinimumThreshold}", notification.MinimumThreshold);
            _logger.LogWarning("   Recommended Reorder: {ReorderQuantity}", notification.ReorderQuantity);
            _logger.LogWarning("   Detected: {DetectedAt:yyyy-MM-dd HH:mm:ss} UTC", notification.DetectedAt);

            // Calculate suggested reorder quantity
            var suggestedReorderQuantity = CalculateReorderQuantity(notification.CurrentStock, notification.MinimumThreshold);
            _logger.LogWarning("   Suggested Reorder Quantity: {ReorderQuantity}", suggestedReorderQuantity);

            // Send mock reorder notification to purchasing department
            await SendReorderNotificationToPurchasing(notification, suggestedReorderQuantity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process low stock alert for product {ProductId}", notification.ProductId);
        }
    }

    /// <summary>
    /// Handles out of stock notifications by triggering urgent reorder alerts.
    /// </summary>
    /// <param name="notification">The out of stock notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnNotification(ProductOutOfStockNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate inventory system processing delay
            await Task.Delay(50, cancellationToken);

            _logger.LogError("🚨 OUT OF STOCK ALERT - URGENT");
            _logger.LogError("   Product: {ProductName} (ID: {ProductId})", notification.ProductName, notification.ProductId);
            _logger.LogError("   Price: ${Price:F2}", notification.Price);
            _logger.LogError("   Last Known Stock: {LastKnownStock}", notification.LastKnownStock);
            _logger.LogError("   Detected: {DetectedAt:yyyy-MM-dd HH:mm:ss} UTC", notification.DetectedAt);

            // Calculate urgent reorder quantity
            var urgentReorderQuantity = CalculateUrgentReorderQuantity(notification.ProductName, notification.Price);
            _logger.LogError("   URGENT Reorder Quantity: {ReorderQuantity}", urgentReorderQuantity);

            // Send urgent reorder notification
            await SendUrgentReorderNotification(notification, urgentReorderQuantity, cancellationToken);

            // Notify customer service about potential order fulfillment issues
            await NotifyCustomerServiceAboutStockOut(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process out of stock alert for product {ProductId}", notification.ProductId);
        }
    }

    /// <summary>
    /// Handles order created notifications to track inventory consumption.
    /// </summary>
    /// <param name="notification">The order created notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate inventory tracking delay
            await Task.Delay(25, cancellationToken);

            _logger.LogInformation("📦 INVENTORY TRACKING - Order #{OrderId}", notification.OrderId);
            _logger.LogInformation("   Customer: {CustomerEmail}", notification.CustomerEmail);
            _logger.LogInformation("   Items Reserved:");

            foreach (var item in notification.Items)
            {
                _logger.LogInformation("   - {ProductName}: {Quantity} units reserved",
                    item.ProductName, item.Quantity);

                // In a real system, this would update actual inventory levels
                // For demo purposes, we'll just log the inventory impact
                await LogInventoryImpact(item, cancellationToken);
            }

            _logger.LogInformation("   Total Items Reserved: {TotalItems}",
                notification.Items.Sum(i => i.Quantity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track inventory for order {OrderId}", notification.OrderId);
        }
    }

    /// <summary>
    /// Calculates the suggested reorder quantity based on current stock and threshold.
    /// </summary>
    /// <param name="currentStock">The current stock level.</param>
    /// <param name="threshold">The low stock threshold.</param>
    /// <returns>The suggested reorder quantity.</returns>
    private static int CalculateReorderQuantity(int currentStock, int threshold)
    {
        // Simple reorder logic: order enough to reach 3x the threshold
        var targetStock = threshold * 3;
        return Math.Max(targetStock - currentStock, threshold);
    }

    /// <summary>
    /// Calculates the urgent reorder quantity for out-of-stock items.
    /// </summary>        /// <param name="productName">The product name.</param>
    /// <param name="price">The product price.</param>
    /// <returns>The urgent reorder quantity.</returns>
    private static int CalculateUrgentReorderQuantity(string productName, decimal price)
    {
        // Urgent reorder logic based on product name and price
        var lowerName = productName.ToLower();

        if (lowerName.Contains("electronic") || lowerName.Contains("phone") || lowerName.Contains("computer"))
            return price > 500 ? 5 : 10;

        if (lowerName.Contains("clothing") || lowerName.Contains("shirt") || lowerName.Contains("dress"))
            return price > 100 ? 20 : 50;

        if (lowerName.Contains("book"))
            return 100;

        if (lowerName.Contains("home") || lowerName.Contains("furniture"))
            return price > 200 ? 10 : 25;

        return 20;
    }

    /// <summary>
    /// Sends a reorder notification to the purchasing department.
    /// </summary>
    /// <param name="notification">The low stock notification.</param>
    /// <param name="reorderQuantity">The suggested reorder quantity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendReorderNotificationToPurchasing(
        ProductStockLowNotification notification,
        int reorderQuantity,
        CancellationToken cancellationToken)
    {
        // Simulate sending notification to purchasing system
        await Task.Delay(25, cancellationToken);

        _logger.LogInformation("📋 REORDER NOTIFICATION SENT TO PURCHASING");
        _logger.LogInformation("   Product: {ProductName} (ID: {ProductId})",
            notification.ProductName, notification.ProductId);
        _logger.LogInformation("   Reorder Quantity: {ReorderQuantity}", reorderQuantity);
        _logger.LogInformation("   Estimated Cost: Calculated based on reorder quantity");
        _logger.LogInformation("   Priority: NORMAL");
    }

    /// <summary>
    /// Sends an urgent reorder notification for out-of-stock items.
    /// </summary>
    /// <param name="notification">The out of stock notification.</param>
    /// <param name="reorderQuantity">The urgent reorder quantity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task SendUrgentReorderNotification(
        ProductOutOfStockNotification notification,
        int reorderQuantity,
        CancellationToken cancellationToken)
    {
        // Simulate sending urgent notification to purchasing system
        await Task.Delay(25, cancellationToken);

        _logger.LogError("🚨 URGENT REORDER NOTIFICATION SENT TO PURCHASING");
        _logger.LogError("   Product: {ProductName} (ID: {ProductId})",
            notification.ProductName, notification.ProductId);
        _logger.LogError("   URGENT Reorder Quantity: {ReorderQuantity}", reorderQuantity);
        _logger.LogError("   Estimated Cost: ${EstimatedCost:F2}",
            notification.Price * reorderQuantity);
        _logger.LogError("   Priority: URGENT - STOCK OUT");
        _logger.LogError("   Action Required: Expedite purchase order");
    }

    /// <summary>
    /// Notifies customer service about potential order fulfillment issues.
    /// </summary>
    /// <param name="notification">The out of stock notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task NotifyCustomerServiceAboutStockOut(
        ProductOutOfStockNotification notification,
        CancellationToken cancellationToken)
    {
        // Simulate sending notification to customer service
        await Task.Delay(25, cancellationToken);

        _logger.LogWarning("📞 CUSTOMER SERVICE NOTIFICATION");
        _logger.LogWarning("   Product Out of Stock: {ProductName} (ID: {ProductId})",
            notification.ProductName, notification.ProductId);
        _logger.LogWarning("   Action Required: Update website availability");
        _logger.LogWarning("   Customer Impact: Potential order delays or cancellations");
    }

    /// <summary>
    /// Logs the inventory impact of an order item.
    /// </summary>
    /// <param name="item">The order item.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task LogInventoryImpact(OrderItemNotification item, CancellationToken cancellationToken)
    {
        // Simulate inventory system update
        await Task.Delay(10, cancellationToken);

        // In a real system, this would:
        // 1. Reduce available inventory
        // 2. Check if stock falls below threshold
        // 3. Trigger reorder if necessary
        // 4. Update inventory tracking systems

        _logger.LogDebug("     Inventory Impact: {ProductName} stock reduced by {Quantity}",
            item.ProductName, item.Quantity);
    }

    /// <summary>
    /// Executes the background service. This method subscribes to notifications via the mediator.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inventory Management Service started");

        // Subscribe to notifications through the mediator
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        mediator.Subscribe<ProductStockLowNotification>(this);
        mediator.Subscribe<ProductOutOfStockNotification>(this);
        mediator.Subscribe<OrderCreatedNotification>(this);

        _logger.LogInformation("Inventory Management Service subscribed to inventory notifications");

        // Keep the service running
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Inventory Management Service is stopping");
        }
    }

    /// <summary>
    /// Disposes the service and unsubscribes from notifications.
    /// </summary>
    public override void Dispose()
    {
        try
        {
            // Unsubscribe from notifications
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            mediator.Unsubscribe<ProductStockLowNotification>(this);
            mediator.Unsubscribe<ProductOutOfStockNotification>(this);
            mediator.Unsubscribe<OrderCreatedNotification>(this);

            _logger.LogInformation("Inventory Management Service unsubscribed from notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Inventory Management Service disposal");
        }
        finally
        {
            base.Dispose();
        }
    }
}
