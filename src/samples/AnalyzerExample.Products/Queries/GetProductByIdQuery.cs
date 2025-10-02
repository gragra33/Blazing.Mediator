using AnalyzerExample.Products.Domain;

namespace AnalyzerExample.Products.Queries;

/// <summary>
/// Product queries demonstrating various patterns
/// </summary>
public class GetProductByIdQuery : IProductQuery<ProductDetailDto?>
{
    public int ProductId { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public bool IncludeReviews { get; set; } = true;
    public bool IncludeImages { get; set; } = true;
}