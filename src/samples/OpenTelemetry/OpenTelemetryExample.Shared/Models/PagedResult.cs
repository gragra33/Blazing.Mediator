using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a paginated result set with metadata about the current page and total count.
/// </summary>
/// <typeparam name="T">The type of items in the result set</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the items for the current page.
    /// </summary>
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

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
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

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
    public int EndIndex => Math.Min(StartIndex + Items.Count - 1, TotalCount);

    /// <summary>
    /// Creates a paginated result from a list of items and pagination metadata.
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="page">The current page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="totalCount">The total number of items across all pages</param>
    /// <returns>A new paginated result</returns>
    public static PagedResult<T> Create(List<T> items, int page, int pageSize, int totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = Math.Max(1, page),
            PageSize = Math.Max(1, pageSize),
            TotalCount = Math.Max(0, totalCount)
        };
    }
}