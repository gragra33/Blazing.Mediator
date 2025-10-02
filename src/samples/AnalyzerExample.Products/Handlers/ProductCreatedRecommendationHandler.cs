using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Recommendation engine handler for product created events
/// </summary>
public class ProductCreatedRecommendationHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedRecommendationHandler> _logger;

    public ProductCreatedRecommendationHandler(ILogger<ProductCreatedRecommendationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing recommendations for new product {ProductId} ({ProductName}) in category {Category}", 
            notification.ProductId, notification.ProductName, notification.Category);

        await UpdateRecommendationEngine(notification, cancellationToken);
        await GenerateRelatedProductSuggestions(notification, cancellationToken);
        await UpdateCustomerSegmentTargeting(notification, cancellationToken);
    }

    private async Task UpdateRecommendationEngine(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating recommendation engine with new product {ProductId}", notification.ProductId);
        await Task.Delay(40, cancellationToken);
    }

    private async Task GenerateRelatedProductSuggestions(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Generating related product suggestions for {ProductId}", notification.ProductId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task UpdateCustomerSegmentTargeting(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating customer segment targeting for product {ProductId}", notification.ProductId);
        await Task.Delay(25, cancellationToken);
    }
}