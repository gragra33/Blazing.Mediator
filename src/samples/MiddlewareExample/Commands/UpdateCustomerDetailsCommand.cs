namespace MiddlewareExample.Commands;

/// <summary>
/// Command for updating customer details in the system with a response indicating success/failure.
/// </summary>
public class UpdateCustomerDetailsCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the customer's ID.
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
