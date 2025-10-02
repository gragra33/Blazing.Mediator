using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Domain;
using AnalyzerExample.Products.Queries;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagedResult<ProductSummaryDto>>
{
    private readonly ILogger<GetProductsQueryHandler> _logger;

    public GetProductsQueryHandler(ILogger<GetProductsQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<PagedResult<ProductSummaryDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? [Products] Getting products page {Page}, size {PageSize}", request.Page, request.PageSize);
        
        await Task.Delay(100, cancellationToken);
        
        var products = GenerateProducts(request.PageSize, (request.Page - 1) * request.PageSize);
        
        return new PagedResult<ProductSummaryDto>
        {
            Items = products,
            TotalCount = 250, // Simulated total
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private static List<ProductSummaryDto> GenerateProducts(int count, int offset)
    {
        return Enumerable.Range(offset + 1, count)
            .Select(i => new ProductSummaryDto
            {
                Id = i,
                Name = $"Product {i}",
                SKU = $"SKU-{i:D6}",
                Price = 50m + (i % 100),
                Category = i % 2 == 0 ? "Electronics" : "Clothing",
                InStock = i % 10 != 0,
                StockQuantity = 100 - (i % 50),
                AverageRating = 3.0 + (i % 5) * 0.5,
                ReviewCount = i % 25,
                PrimaryImageUrl = $"/images/product-{i}-thumb.jpg"
            }).ToList();
    }
}