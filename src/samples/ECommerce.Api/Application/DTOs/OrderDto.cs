using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Data transfer object representing an order in the e-commerce system.
/// Used for transferring order data between layers and to API consumers.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the order.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer who placed the order.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the customer who placed the order.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the shipping address for the order.
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name of the order status.
    /// </summary>
    public string StatusName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of items included in the order.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = [];
}