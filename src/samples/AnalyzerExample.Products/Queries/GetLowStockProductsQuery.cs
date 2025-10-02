using AnalyzerExample.Products.Domain;

namespace AnalyzerExample.Products.Queries;

public class GetLowStockProductsQuery : IProductQuery<List<ProductSummaryDto>>
{
    public int Threshold { get; set; } = 10;
    public bool IncludeDeleted { get; set; } = false;
}