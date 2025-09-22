using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get a user by ID.
/// </summary>
public sealed class GetUserQuery : IRequest<UserDto>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user to retrieve.
    /// </summary>
    public int UserId { get; set; }
}