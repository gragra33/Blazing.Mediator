using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to process a complete order including creation, validation, and payment processing.
/// </summary>
public class ProcessOrderCommand : IRequest<OperationResult<ProcessOrderResponse>>
{
    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer email address.
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

    /// <summary>
    /// Gets or sets the payment method for the order.
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;
}