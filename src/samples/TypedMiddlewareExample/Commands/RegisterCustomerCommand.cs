namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command to register a new customer.
/// Uses custom ICustomerRequest interface to demonstrate type constraints.
/// </summary>
public class RegisterCustomerCommand : ICustomerRequest
{
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