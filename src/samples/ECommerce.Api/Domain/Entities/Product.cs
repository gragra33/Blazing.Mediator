namespace ECommerce.Api.Domain.Entities;

/// <summary>
/// Represents a product in the e-commerce system.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the available stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of order items associated with this product.
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Creates a new product with the specified details.
    /// </summary>
    /// <param name="name">The name of the product.</param>
    /// <param name="description">The description of the product.</param>
    /// <param name="price">The price of the product.</param>
    /// <param name="stockQuantity">The initial stock quantity.</param>
    /// <returns>A new product instance.</returns>
    public static Product Create(string name, string description, decimal price, int stockQuantity)
    {
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the stock quantity of the product.
    /// </summary>
    /// <param name="quantity">The new stock quantity.</param>
    public void UpdateStock(int quantity)
    {
        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the product has sufficient stock for the requested quantity.
    /// </summary>
    /// <param name="requestedQuantity">The requested quantity.</param>
    /// <returns>True if sufficient stock is available; otherwise, false.</returns>
    public bool HasSufficientStock(int requestedQuantity)
    {
        return IsActive && StockQuantity >= requestedQuantity;
    }

    /// <summary>
    /// Reserves the specified quantity from the product's stock.
    /// </summary>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <exception cref="InvalidOperationException">Thrown when insufficient stock is available.</exception>
    public void ReserveStock(int quantity)
    {
        if (!HasSufficientStock(quantity))
            throw new InvalidOperationException($"Insufficient stock for product {Name}");

        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}