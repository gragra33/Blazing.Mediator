using Blazing.Mediator;
using OpenTelemetryExample.Shared.DTOs;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get all users.
/// </summary>
public sealed class GetUsersQuery : IRequest<List<UserDto>>
{
    public bool IncludeInactive { get; set; }
    public string? SearchTerm { get; set; }
}