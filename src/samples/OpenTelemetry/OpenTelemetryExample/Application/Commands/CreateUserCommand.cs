using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
public sealed class CreateUserCommand : IRequest<int>
{
    /// <summary>
    /// Gets or sets the name of the user to create.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user to create.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}