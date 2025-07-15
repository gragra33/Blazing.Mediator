using Blazing.Mediator;

namespace ECommerce.Api.Application.Notifications;

/// <summary>
/// Notification published when a product goes out of stock.
/// This allows various services to react to stock depletion events.
/// </summary>
public class ProductOutOfStockNotification : INotification
{
    /// <summary>
    /// Gets the ID of the product that is out of stock.
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the name of the product that is out of stock.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the price of the product that is out of stock.
    /// </summary>
    public decimal Price { get; }

    /// <summary>
    /// Gets the last known stock quantity before depletion.
    /// </summary>
    public int LastKnownStock { get; }

    /// <summary>
    /// Gets the recommended reorder quantity.
    /// </summary>
    public int ReorderQuantity { get; }

    /// <summary>
    /// Gets the timestamp when the out-of-stock condition was detected.
    /// </summary>
    public DateTime DetectedAt { get; }

    /// <summary>
    /// Initializes a new instance of the ProductOutOfStockNotification.
    /// </summary>
    /// <param name="productId">The ID of the product that is out of stock.</param>
    /// <param name="productName">The name of the product that is out of stock.</param>
    /// <param name="price">The price of the product that is out of stock.</param>
    /// <param name="lastKnownStock">The last known stock quantity before depletion.</param>
    /// <param name="reorderQuantity">The recommended reorder quantity.</param>
    public ProductOutOfStockNotification(int productId, string productName, decimal price, int lastKnownStock, int reorderQuantity)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        LastKnownStock = lastKnownStock;
        ReorderQuantity = reorderQuantity;
        DetectedAt = DateTime.UtcNow;
    }
}
