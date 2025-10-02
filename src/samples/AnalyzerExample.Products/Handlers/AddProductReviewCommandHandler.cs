using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for adding product reviews
/// </summary>
public class AddProductReviewCommandHandler : IRequestHandler<AddProductReviewCommand, OperationResult<int>>
{
    public async Task<OperationResult<int>> Handle(AddProductReviewCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate review creation
        await Task.Delay(50, cancellationToken);
        
        // Return success with new review ID
        var reviewId = Random.Shared.Next(1000, 9999);
        return OperationResult<int>.Success(reviewId);
    }
}