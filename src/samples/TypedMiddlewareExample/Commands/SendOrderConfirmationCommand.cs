namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command to send order confirmation email.
/// Uses custom IOrderRequest interface to demonstrate type constraints.
/// </summary>
public class SendOrderConfirmationCommand : IOrderRequest
{
    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the customer email address.
    /// </summary>
    public required string CustomerEmail { get; set; }
}