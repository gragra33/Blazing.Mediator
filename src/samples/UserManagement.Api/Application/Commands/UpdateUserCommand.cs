using Blazing.Mediator;

namespace UserManagement.Api.Application.Commands;

/// <summary>
/// Command to update an existing user's personal information.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class UpdateUserCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the user to update.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the updated first name of the user.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated last name of the user.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}