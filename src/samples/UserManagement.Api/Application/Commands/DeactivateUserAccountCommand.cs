using Blazing.Mediator;

namespace UserManagement.Api.Application.Commands;

/// <summary>
/// Command to deactivate a user account.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class DeactivateUserAccountCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the user account to deactivate.
    /// </summary>
    public int UserId { get; set; }
}