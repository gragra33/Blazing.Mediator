using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get recent telemetry traces from the database organized in hierarchical groups.
/// </summary>
public sealed class GetGroupedTracesQuery : IRequest<GroupedTracesDto>
{
    public int MaxRecords { get; set; } = 10;
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);
    public bool MediatorOnly { get; set; } = false;
    public bool ExampleAppOnly { get; set; } = false;
    public bool HidePackets { get; set; } = false;
    
    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Number of trace groups per page.
    /// </summary>
    public int PageSize { get; set; } = 10;
}