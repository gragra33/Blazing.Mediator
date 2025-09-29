using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get all users.
/// </summary>
public sealed class GetUsersQuery : IRequest<List<UserDto>>
{
    /// <summary>
    /// Gets or sets a value indicating whether to include inactive users in the result.
    /// </summary>
    public bool IncludeInactive { get; set; }

    /// <summary>
    /// Gets or sets the search term to filter users by name or other criteria.
    /// </summary>
    public string? SearchTerm { get; set; }
}