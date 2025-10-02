using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Products.Domain;

namespace AnalyzerExample.Products.Queries;

public class GetProductsByCategoryQuery : IProductQuery<List<ProductSummaryDto>>, ICacheableQuery<List<ProductSummaryDto>>
{
    public string Category { get; set; } = string.Empty;
    public bool InStockOnly { get; set; } = true;
    public bool IncludeDeleted { get; set; } = false;
    
    public string GetCacheKey() => $"products_category_{Category}_instock_{InStockOnly}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(15);
}