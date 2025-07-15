using Blazing.Mediator;

namespace ECommerce.Api.Application.Notifications;

/// <summary>
/// Notification published when a new product is created in the e-commerce system.
/// This follows the observer pattern to notify interested parties about product creation.
/// </summary>
public class ProductCreatedNotification : INotification
{
    /// <summary>
    /// Gets the ID of the newly created product.
    /// </summary>
    public int ProductId { get; }

    /// <summary>
    /// Gets the name of the newly created product.
    /// </summary>
    public string ProductName { get; }

    /// <summary>
    /// Gets the price of the newly created product.
    /// </summary>
    public decimal Price { get; }

    /// <summary>
    /// Gets the initial stock quantity of the newly created product.
    /// </summary>
    public int StockQuantity { get; }

    /// <summary>
    /// Gets the timestamp when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// Initializes a new instance of the ProductCreatedNotification.
    /// </summary>
    /// <param name="productId">The ID of the newly created product.</param>
    /// <param name="productName">The name of the newly created product.</param>
    /// <param name="price">The price of the newly created product.</param>
    /// <param name="stockQuantity">The initial stock quantity of the newly created product.</param>
    public ProductCreatedNotification(int productId, string productName, decimal price, int stockQuantity)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        StockQuantity = stockQuantity;
        CreatedAt = DateTime.UtcNow;
    }
}
