using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for product stock changes
/// </summary>
public class ProductStockChangedEventHandler : INotificationHandler<ProductStockChangedEvent>
{
    private readonly ILogger<ProductStockChangedEventHandler> _logger;

    public ProductStockChangedEventHandler(ILogger<ProductStockChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductStockChangedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Product {ProductId} stock changed from {OldQuantity} to {NewQuantity}", 
            notification.ProductId, notification.OldQuantity, notification.NewQuantity);

        await UpdateAvailabilityStatus(notification, cancellationToken);
        await CheckReorderThresholds(notification, cancellationToken);
        await UpdateDisplayStatus(notification, cancellationToken);
    }

    private async Task UpdateAvailabilityStatus(ProductStockChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating availability status for product {ProductId}", notification.ProductId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task CheckReorderThresholds(ProductStockChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking reorder thresholds for product {ProductId}", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateDisplayStatus(ProductStockChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating display status for product {ProductId}", notification.ProductId);
        await Task.Delay(10, cancellationToken);
    }
}