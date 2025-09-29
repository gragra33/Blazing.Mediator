using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get recent telemetry activities from the database.
/// </summary>
public sealed class GetRecentActivitiesQuery : IRequest<RecentActivitiesDto>
{
    /// <summary>
    /// Gets or sets the maximum number of activity records to retrieve.
    /// </summary>
    public int MaxRecords { get; set; } = 50;

    /// <summary>
    /// Gets or sets the time window for which to retrieve activities.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);
}