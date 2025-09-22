using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents recent trace data for the dashboard with pagination support.
/// </summary>
public sealed class RecentTracesDto
{
    /// <summary>
    /// Gets or sets the timestamp when the trace data was retrieved.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the list of trace entries for the current page.
    /// </summary>
    [JsonPropertyName("traces")]
    public List<TraceDto> Traces { get; set; } = new();

    /// <summary>
    /// Gets or sets a message describing the trace data or any relevant status.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of traces in the selected timeframe.
    /// </summary>
    [JsonPropertyName("totalTracesInTimeframe")]
    public int TotalTracesInTimeframe { get; set; }

    /// <summary>
    /// Gets or sets the pagination information for the traces.
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; set; } = new();
}

/// <summary>
/// Contains pagination metadata for trace results.
/// </summary>
public sealed class PaginationInfo
{
    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of filtered items (matching current filters).
    /// </summary>
    [JsonPropertyName("totalFilteredCount")]
    public int TotalFilteredCount { get; set; }

    /// <summary>
    /// Gets the total number of pages for filtered results.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalFilteredCount / PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets the 1-based index of the first item on the current page.
    /// </summary>
    [JsonPropertyName("startIndex")]
    public int StartIndex => PageSize > 0 ? ((Page - 1) * PageSize) + 1 : 0;

    /// <summary>
    /// Gets the 1-based index of the last item on the current page.
    /// </summary>
    [JsonPropertyName("endIndex")]
    public int EndIndex => Math.Min(StartIndex + ItemCount - 1, TotalFilteredCount);

    /// <summary>
    /// Gets or sets the number of items returned in the current page.
    /// </summary>
    [JsonPropertyName("itemCount")]
    public int ItemCount { get; set; }
}