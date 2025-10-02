using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Marketing handler for product price changed events
/// </summary>
public class ProductPriceChangedMarketingHandler : INotificationHandler<ProductPriceChangedEvent>
{
    private readonly ILogger<ProductPriceChangedMarketingHandler> _logger;

    public ProductPriceChangedMarketingHandler(ILogger<ProductPriceChangedMarketingHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing marketing updates for product {ProductId} price change: {OldPrice:C} ? {NewPrice:C}", 
            notification.ProductId, notification.OldPrice, notification.NewPrice);

        await UpdatePriceAlerts(notification, cancellationToken);
        await TriggerPromotionalCampaigns(notification, cancellationToken);
        await NotifySubscribedCustomers(notification, cancellationToken);
    }

    private async Task UpdatePriceAlerts(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating price alerts for product {ProductId}", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task TriggerPromotionalCampaigns(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.NewPrice < notification.OldPrice)
        {
            _logger.LogDebug("Triggering promotional campaigns for price reduction on product {ProductId}", notification.ProductId);
            await Task.Delay(35, cancellationToken);
        }
    }

    private async Task NotifySubscribedCustomers(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying subscribed customers of price change for product {ProductId}", notification.ProductId);
        await Task.Delay(30, cancellationToken);
    }
}