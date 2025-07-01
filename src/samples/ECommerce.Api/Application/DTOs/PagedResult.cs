namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Generic data transfer object for representing paginated results.
/// Provides pagination metadata along with the requested data items.
/// </summary>
/// <typeparam name="T">The type of items contained in the paged result.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Gets or sets the list of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages based on the total count and page size.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a next page available.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}