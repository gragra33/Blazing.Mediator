namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command for updating customer details in the system.
/// </summary>
public class UpdateCustomerDetailsCommand : ICommand<bool>
{
    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's preferred contact method.
    /// </summary>
    public string ContactMethod { get; set; } = "Email";
}