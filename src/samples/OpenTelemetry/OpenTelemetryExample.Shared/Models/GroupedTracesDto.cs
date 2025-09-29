using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents grouped trace data for the dashboard display mode with pagination support.
/// </summary>
public sealed class GroupedTracesDto
{
    /// <summary>
    /// Gets or sets the timestamp for the grouped trace data.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the list of trace groups.
    /// </summary>
    [JsonPropertyName("traceGroups")]
    public List<TraceGroupDto> TraceGroups { get; set; } = new();

    /// <summary>
    /// Gets or sets the message associated with the grouped traces.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of traces in the specified timeframe.
    /// </summary>
    [JsonPropertyName("totalTracesInTimeframe")]
    public int TotalTracesInTimeframe { get; set; }

    /// <summary>
    /// Gets or sets the total number of trace groups.
    /// </summary>
    [JsonPropertyName("totalTraceGroups")]
    public int TotalTraceGroups { get; set; }

    /// <summary>
    /// Gets or sets the pagination information for the trace groups.
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();
}