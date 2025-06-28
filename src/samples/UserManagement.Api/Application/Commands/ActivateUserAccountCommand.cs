using Blazing.Mediator;

namespace UserManagement.Api.Application.Commands;

/// <summary>
/// Command to activate a user account.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class ActivateUserAccountCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the user account to activate.
    /// </summary>
    public int UserId { get; set; }
}