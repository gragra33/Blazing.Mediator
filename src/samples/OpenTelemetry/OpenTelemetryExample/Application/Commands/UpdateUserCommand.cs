using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Command to update a user.
/// </summary>
public sealed class UpdateUserCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the user to update.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the new name for the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new email address for the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}