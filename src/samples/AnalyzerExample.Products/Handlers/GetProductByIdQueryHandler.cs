using AnalyzerExample.Products.Domain;
using AnalyzerExample.Products.Queries;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Product query handlers
/// </summary>
public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDetailDto?>
{
    private readonly ILogger<GetProductByIdQueryHandler> _logger;

    public GetProductByIdQueryHandler(ILogger<GetProductByIdQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<ProductDetailDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? [Products] Getting product details for ID: {ProductId}", request.ProductId);
        
        // Simulate database lookup
        await Task.Delay(50, cancellationToken);
        
        if (request.ProductId <= 0)
            return null;

        return new ProductDetailDto
        {
            Id = request.ProductId,
            Name = $"Product {request.ProductId}",
            Description = $"Detailed description for product {request.ProductId}",
            SKU = $"SKU-{request.ProductId:D6}",
            Price = 99.99m + request.ProductId,
            Category = "Electronics",
            InStock = true,
            StockQuantity = 100 - request.ProductId,
            CreatedAt = DateTime.UtcNow.AddDays(-request.ProductId),
            Images = request.IncludeImages ? GenerateProductImages(request.ProductId) : new(),
            Reviews = request.IncludeReviews ? GenerateProductReviews(request.ProductId) : new(),
            AverageRating = 4.2 + (request.ProductId % 3) * 0.3,
            ReviewCount = 10 + request.ProductId % 20
        };
    }

    private static List<ProductImageDto> GenerateProductImages(int productId)
    {
        return new List<ProductImageDto>
        {
            new() { Id = productId * 10, ImageUrl = $"/images/product-{productId}-main.jpg", IsPrimary = true, DisplayOrder = 1 },
            new() { Id = productId * 10 + 1, ImageUrl = $"/images/product-{productId}-alt1.jpg", IsPrimary = false, DisplayOrder = 2 }
        };
    }

    private static List<ProductReviewDto> GenerateProductReviews(int productId)
    {
        return new List<ProductReviewDto>
        {
            new() { Id = productId * 100, UserId = 1, UserName = "Alice", Rating = 5, Title = "Great product!", Comment = "Really satisfied with this purchase.", IsVerifiedPurchase = true, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = productId * 100 + 1, UserId = 2, UserName = "Bob", Rating = 4, Title = "Good value", Comment = "Works as expected.", IsVerifiedPurchase = true, CreatedAt = DateTime.UtcNow.AddDays(-5) }
        };
    }
}