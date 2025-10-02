using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Products.Domain;

namespace AnalyzerExample.Products.Queries;

public class GetProductsQuery : IProductQuery<PagedResult<ProductSummaryDto>>, IPaginatedQuery<PagedResult<ProductSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? CategoryFilter { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public string? SearchTerm { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
    public bool SortDescending { get; set; } = false;
    public bool IncludeDeleted { get; set; } = false;
}