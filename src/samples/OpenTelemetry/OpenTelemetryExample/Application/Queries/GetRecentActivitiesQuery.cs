using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get recent telemetry activities from the database.
/// </summary>
public sealed class GetRecentActivitiesQuery : IRequest<RecentActivitiesDto>
{
    public int MaxRecords { get; set; } = 50;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);
}