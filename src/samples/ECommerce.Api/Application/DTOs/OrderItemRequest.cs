namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Request object for creating an order item.
/// Used when submitting order information from API consumers.
/// </summary>
public class OrderItemRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the product to order.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity of the product to order.
    /// </summary>
    public int Quantity { get; set; }
}