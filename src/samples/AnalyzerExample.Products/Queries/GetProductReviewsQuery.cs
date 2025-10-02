using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Products.Domain;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Queries;

public class GetProductReviewsQuery : IQuery<PagedResult<ProductReviewDto>>, IPaginatedQuery<PagedResult<ProductReviewDto>>
{
    public int ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? MinRating { get; set; }
    public bool VerifiedPurchasesOnly { get; set; } = false;
}