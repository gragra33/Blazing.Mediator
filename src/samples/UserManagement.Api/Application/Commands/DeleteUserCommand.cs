using Blazing.Mediator;

namespace UserManagement.Api.Application.Commands;

/// <summary>
/// Command to delete a user from the system.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class DeleteUserCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the user to delete.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the reason for deleting the user.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}