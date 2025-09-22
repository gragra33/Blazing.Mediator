using Blazing.Mediator;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Query to get recent telemetry traces from the database organized in hierarchical groups.
/// </summary>
public sealed class GetGroupedTracesQuery : IRequest<GroupedTracesDto>
{
    /// <summary>
    /// Gets or sets the maximum number of records to return.
    /// </summary>
    public int MaxRecords { get; set; } = 10;

    /// <summary>
    /// Gets or sets the time window for which to retrieve traces.
    /// </summary>
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets or sets a value indicating whether to include only mediator traces.
    /// </summary>
    public bool MediatorOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include only example app traces.
    /// </summary>
    public bool ExampleAppOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to hide packet traces.
    /// </summary>
    public bool HidePackets { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the number of trace groups per page.
    /// </summary>
    public int PageSize { get; set; } = 10;
}