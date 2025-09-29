namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command to update customer details.
/// Uses custom ICustomerRequest interface with response to demonstrate type constraints.
/// </summary>
public class UpdateCustomerDetailsCommand : ICustomerRequest<bool>
{
    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public required string CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer's full name.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the preferred contact method.
    /// </summary>
    public required string ContactMethod { get; set; }
}