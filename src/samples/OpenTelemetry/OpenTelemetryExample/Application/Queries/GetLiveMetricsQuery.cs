using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get live telemetry metrics from the database.
/// </summary>
public sealed class GetLiveMetricsQuery : IRequest<LiveMetricsDto>
{
    /// <summary>
    /// Gets or sets the maximum number of records to retrieve.
    /// </summary>
    public int MaxRecords { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window for which to retrieve metrics.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);
}