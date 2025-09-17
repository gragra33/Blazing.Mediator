using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Command to update a user.
/// </summary>
public sealed class UpdateUserCommand : IRequest
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}