using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Products.Domain;

namespace AnalyzerExample.Products.Queries;

public class SearchProductsQuery : IProductQuery<PagedResult<ProductSummaryDto>>, IPaginatedQuery<PagedResult<ProductSummaryDto>>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<string> Categories { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinRating { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}