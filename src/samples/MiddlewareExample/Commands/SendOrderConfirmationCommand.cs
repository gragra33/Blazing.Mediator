namespace MiddlewareExample.Commands;

/// <summary>
/// Command to send order confirmation email to customer.
/// </summary>
public class SendOrderConfirmationCommand : IRequest
{
    /// <summary>
    /// Gets or sets the order ID for the confirmation.
    /// </summary>
    public required string OrderId { get; set; }
    
    /// <summary>
    /// Gets or sets the customer email address.
    /// </summary>
    public required string CustomerEmail { get; set; }
}
