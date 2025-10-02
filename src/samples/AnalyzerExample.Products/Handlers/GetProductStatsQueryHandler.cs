using AnalyzerExample.Products.Domain;
using AnalyzerExample.Products.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for retrieving product statistics
/// </summary>
public class GetProductStatsQueryHandler : IRequestHandler<GetProductStatsQuery, ProductStatsDto>
{
    public async Task<ProductStatsDto> Handle(GetProductStatsQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(55, cancellationToken);
        
        return new ProductStatsDto
        {
            ProductId = 1,
            ProductName = "Sample Product",
            TotalReviews = 25,
            AverageRating = 4.2,
            TotalSales = 150,
            TotalRevenue = 4495.50m,
            CurrentStock = 45,
            LastSaleDate = DateTime.UtcNow.AddDays(-2),
            LastRestockDate = DateTime.UtcNow.AddDays(-10)
        };
    }
}