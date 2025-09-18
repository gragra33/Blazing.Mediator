using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get live telemetry metrics from the database.
/// </summary>
public sealed class GetLiveMetricsQuery : IRequest<LiveMetricsDto>
{
    public int MaxRecords { get; set; } = 100;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);
}