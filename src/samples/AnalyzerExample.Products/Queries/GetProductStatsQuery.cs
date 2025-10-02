namespace AnalyzerExample.Products.Queries;

public class GetProductStatsQuery : IProductQuery<ProductStatsDto>
{
    public int ProductId { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}