using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents grouped trace data for the dashboard display mode with pagination support.
/// </summary>
public sealed class GroupedTracesDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("traceGroups")]
    public List<TraceGroupDto> TraceGroups { get; set; } = new();

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("totalTracesInTimeframe")]
    public int TotalTracesInTimeframe { get; set; }

    [JsonPropertyName("totalTraceGroups")]
    public int TotalTraceGroups { get; set; }

    /// <summary>
    /// Gets or sets the pagination information for the trace groups.
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();
}