using Blazing.Mediator;

namespace ECommerce.Api.Application.Notifications;

/// <summary>
/// Notification published when a product's stock level becomes low.
/// This allows inventory management services to react to stock shortages.
/// </summary>
public class ProductStockLowNotification : INotification
{
    /// <summary>
    /// Gets the ID of the product with low stock.
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the name of the product with low stock.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the current stock quantity.
    /// </summary>
    public int CurrentStock { get; }

    /// <summary>
    /// Gets the minimum stock threshold that was crossed.
    /// </summary>
    public int MinimumThreshold { get; }

    /// <summary>
    /// Gets the recommended reorder quantity.
    /// </summary>
    public int ReorderQuantity { get; }

    /// <summary>
    /// Gets the timestamp when the low stock was detected.
    /// </summary>
    public DateTime DetectedAt { get; }

    /// <summary>
    /// Initializes a new instance of the ProductStockLowNotification.
    /// </summary>
    /// <param name="productId">The ID of the product with low stock.</param>
    /// <param name="productName">The name of the product with low stock.</param>
    /// <param name="currentStock">The current stock quantity.</param>
    /// <param name="minimumThreshold">The minimum stock threshold that was crossed.</param>
    /// <param name="reorderQuantity">The recommended reorder quantity.</param>
    public ProductStockLowNotification(int productId, string productName, int currentStock, int minimumThreshold, int reorderQuantity)
    {
        ProductId = productId;
        ProductName = productName;
        CurrentStock = currentStock;
        MinimumThreshold = minimumThreshold;
        ReorderQuantity = reorderQuantity;
        DetectedAt = DateTime.UtcNow;
    }
}
