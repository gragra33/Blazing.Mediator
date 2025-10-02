using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Domain;
using AnalyzerExample.Products.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for retrieving products by category
/// </summary>
public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductSummaryDto>>
{
    public async Task<List<ProductSummaryDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(45, cancellationToken);
        
        return new List<ProductSummaryDto>
        {
            new ProductSummaryDto
            {
                Id = 1,
                Name = "Sample Product 1",
                SKU = "PROD-001",
                Price = 29.99m,
                Category = request.Category,
                InStock = true,
                StockQuantity = 100,
                AverageRating = 4.2,
                ReviewCount = 15
            },
            new ProductSummaryDto
            {
                Id = 2,
                Name = "Sample Product 2", 
                SKU = "PROD-002",
                Price = 39.99m,
                Category = request.Category,
                InStock = true,
                StockQuantity = 50,
                AverageRating = 4.5,
                ReviewCount = 8
            }
        };
    }
}