using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get recent telemetry traces from the database.
/// </summary>
public sealed class GetRecentTracesQuery : IRequest<RecentTracesDto>
{
    /// <summary>
    /// The maximum number of records to return.
    /// </summary>
    public int MaxRecords { get; set; } = 10;

    /// <summary>
    /// The time window to look back for traces.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// If true, only return traces related to the Mediator.
    /// </summary>
    public bool MediatorOnly { get; set; } = false;

    /// <summary>
    /// If true, only return traces related to the Example application.
    /// </summary>
    public bool ExampleAppOnly { get; set; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of records per page.
    /// </summary>
    public int PageSize { get; set; } = 10;
}