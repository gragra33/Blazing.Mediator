using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for product review events
/// </summary>
public class ProductReviewAddedEventHandler : INotificationHandler<ProductReviewAddedEvent>
{
    private readonly ILogger<ProductReviewAddedEventHandler> _logger;

    public ProductReviewAddedEventHandler(ILogger<ProductReviewAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductReviewAddedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("New review added for product {ProductId} with rating {Rating}", 
            notification.ProductId, notification.Rating);

        await RecalculateAverageRating(notification, cancellationToken);
        await CheckForReviewMilestones(notification, cancellationToken);
        await ModerateReviewContent(notification, cancellationToken);
    }

    private async Task RecalculateAverageRating(ProductReviewAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Recalculating average rating for product {ProductId}", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task CheckForReviewMilestones(ProductReviewAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking review milestones for product {ProductId}", notification.ProductId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task ModerateReviewContent(ProductReviewAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Moderating review content for review {ReviewId}", notification.ReviewId);
        await Task.Delay(30, cancellationToken);
    }
}