namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Request object for creating a new order.
/// Used when submitting order creation requests from API consumers.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer placing the order.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the customer placing the order.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shipping address for the order.
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of items to include in the order.
    /// </summary>
    public List<OrderItemRequest> Items { get; set; } = [];
}