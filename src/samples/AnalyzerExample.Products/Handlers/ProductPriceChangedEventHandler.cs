using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for product price changes
/// </summary>
public class ProductPriceChangedEventHandler : INotificationHandler<ProductPriceChangedEvent>
{
    private readonly ILogger<ProductPriceChangedEventHandler> _logger;

    public ProductPriceChangedEventHandler(ILogger<ProductPriceChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Product {ProductId} price changed from {OldPrice:C} to {NewPrice:C}", 
            notification.ProductId, notification.OldPrice, notification.NewPrice);

        await UpdatePricingTiers(notification, cancellationToken);
        await RecalculatePromotions(notification, cancellationToken);
        await NotifyPriceWatchers(notification, cancellationToken);
    }

    private async Task UpdatePricingTiers(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating pricing tiers for product {ProductId}", notification.ProductId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task RecalculatePromotions(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Recalculating promotions for product {ProductId}", notification.ProductId);
        await Task.Delay(35, cancellationToken);
    }

    private async Task NotifyPriceWatchers(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying price watchers for product {ProductId}", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }
}