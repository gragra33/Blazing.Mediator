using Blazing.Mediator;

namespace ECommerce.Api.Application.Notifications;

/// <summary>
/// Notification published when a product is updated in the e-commerce system.
/// This follows the observer pattern to notify interested parties about product updates.
/// </summary>
public class ProductUpdatedNotification : INotification
{
    /// <summary>
    /// Gets the ID of the updated product.
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the name of the updated product.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the new price of the updated product.
    /// </summary>
    public decimal NewPrice { get; }

    /// <summary>
    /// Gets the previous price of the product.
    /// </summary>
    public decimal? OldPrice { get; }

    /// <summary>
    /// Gets the timestamp when the product was updated.
    /// </summary>
    public DateTime UpdatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the ProductUpdatedNotification.
    /// </summary>
    /// <param name="productId">The ID of the updated product.</param>
    /// <param name="productName">The name of the updated product.</param>
    /// <param name="newPrice">The new price of the updated product.</param>
    /// <param name="oldPrice">The previous price of the product.</param>
    public ProductUpdatedNotification(int productId, string productName, decimal newPrice, decimal? oldPrice = null)
    {
        ProductId = productId;
        ProductName = productName;
        NewPrice = newPrice;
        OldPrice = oldPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
