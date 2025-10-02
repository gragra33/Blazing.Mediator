using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Domain;
using AnalyzerExample.Products.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for retrieving product reviews
/// </summary>
public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, PagedResult<ProductReviewDto>>
{
    public async Task<PagedResult<ProductReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(35, cancellationToken);
        
        var reviews = new List<ProductReviewDto>
        {
            new ProductReviewDto
            {
                Id = 1,
                UserId = 123,
                UserName = "John Doe",
                Rating = 5,
                Title = "Great product!",
                Comment = "Really love this product, highly recommended!",
                IsVerifiedPurchase = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new ProductReviewDto
            {
                Id = 2,
                UserId = 456,
                UserName = "Jane Smith",
                Rating = 4,
                Title = "Good quality",
                Comment = "Good quality product, worth the price.",
                IsVerifiedPurchase = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };
        
        return new PagedResult<ProductReviewDto>
        {
            Items = reviews.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            TotalCount = reviews.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}