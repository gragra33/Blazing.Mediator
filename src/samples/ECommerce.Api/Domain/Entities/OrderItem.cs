namespace ECommerce.Api.Domain.Entities;

/// <summary>
/// Represents an item within an order.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the order item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the product identifier.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product at the time of order.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the order that this item belongs to.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Gets or sets the product associated with this order item.
    /// </summary>
    public Product Product { get; set; } = null!;

    /// <summary>
    /// Gets the total price for this order item (quantity × unit price).
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;
}