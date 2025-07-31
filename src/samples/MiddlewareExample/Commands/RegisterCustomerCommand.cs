namespace MiddlewareExample.Commands;

/// <summary>
/// Command for registering a new customer account in the system.
/// </summary>
public class RegisterCustomerCommand : IRequest
{
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
