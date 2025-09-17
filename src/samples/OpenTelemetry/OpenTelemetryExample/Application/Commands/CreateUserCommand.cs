using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
public sealed class CreateUserCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}