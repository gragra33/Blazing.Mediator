using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Stream request to get users with real-time streaming capability.
/// Demonstrates streaming with OpenTelemetry tracing integration.
/// </summary>
public class StreamUsersQuery : IStreamRequest<UserDto>
{
    /// <summary>
    /// Optional search term to filter users.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Number of items to stream (for demo purposes).
    /// </summary>
    public int Count { get; set; } = 10;

    /// <summary>
    /// Delay between items in milliseconds (for demo purposes).
    /// </summary>
    public int DelayMs { get; set; } = 500;

    /// <summary>
    /// Whether to include inactive users.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}