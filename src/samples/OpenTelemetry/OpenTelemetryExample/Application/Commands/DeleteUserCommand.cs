using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Command to delete a user.
/// </summary>
public sealed class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }
}