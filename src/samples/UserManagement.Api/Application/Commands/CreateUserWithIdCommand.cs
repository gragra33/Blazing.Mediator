using Blazing.Mediator;

namespace UserManagement.Api.Application.Commands;

/// <summary>
/// Command to create a new user and return the generated ID.
/// This is a CQRS command that represents a write operation and returns the user ID.
/// </summary>
public class CreateUserWithIdCommand : IRequest<int>
{
    /// <summary>
    /// Gets or sets the first name of the user to be created.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the user to be created.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user to be created.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of birth of the user to be created.
    /// </summary>
    public DateTime DateOfBirth { get; set; }
}