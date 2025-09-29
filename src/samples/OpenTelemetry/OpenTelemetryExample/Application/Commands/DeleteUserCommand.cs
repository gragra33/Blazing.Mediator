using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Command to delete a user.
/// </summary>
public sealed class DeleteUserCommand : IRequest
{
    /// <summary>
    /// Gets or sets the ID of the user to delete.
    /// </summary>
    public int UserId { get; set; }
}