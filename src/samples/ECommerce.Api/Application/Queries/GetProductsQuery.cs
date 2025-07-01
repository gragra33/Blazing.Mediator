using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve a paginated list of products with optional filtering.
/// </summary>
public class GetProductsQuery : IRequest<PagedResult<ProductDto>>
{
    /// <summary>
    /// Gets or sets the page number for pagination (default: 1).
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the number of items per page (default: 10).
    /// </summary>
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// Gets or sets the search term to filter products by name or description.
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether to return only products that are in stock.
    /// </summary>
    public bool InStockOnly { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to return only active products (default: true).
    /// </summary>
    public bool ActiveOnly { get; set; } = true;
}