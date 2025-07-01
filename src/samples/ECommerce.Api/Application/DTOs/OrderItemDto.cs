namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Data transfer object representing an item within an order.
/// Used for transferring order item data between layers and to API consumers.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the order item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product at the time of ordering.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the total price for this order item (quantity × unit price).
    /// </summary>
    public decimal TotalPrice { get; set; }
}