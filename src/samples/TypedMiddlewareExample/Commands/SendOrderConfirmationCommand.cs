namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command for sending order confirmation emails.
/// </summary>
public class SendOrderConfirmationCommand : ICommand
{
    /// <summary>
    /// Gets or sets the order ID.
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public required string CustomerEmail { get; set; }
}