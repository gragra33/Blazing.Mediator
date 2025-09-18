using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get recent telemetry traces from the database.
/// </summary>
public sealed class GetRecentTracesQuery : IRequest<RecentTracesDto>
{
    public int MaxRecords { get; set; } = 10;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);
    public bool FilterBlazingMediatorOnly { get; set; } = false;
    public bool BlazingMediatorOnly // alias for compatibility with new param
    {
        get => FilterBlazingMediatorOnly;
        set => FilterBlazingMediatorOnly = value;
    }
}