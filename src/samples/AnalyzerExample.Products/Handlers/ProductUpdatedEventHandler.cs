using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for product updated events
/// </summary>
public class ProductUpdatedEventHandler : INotificationHandler<ProductUpdatedEvent>
{
    private readonly ILogger<ProductUpdatedEventHandler> _logger;

    public ProductUpdatedEventHandler(ILogger<ProductUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductUpdatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Product {ProductId} updated", notification.ProductId);

        await InvalidateProductCache(notification, cancellationToken);
        await UpdateSearchIndex(notification, cancellationToken);
        await NotifySubscribers(notification, cancellationToken);
    }

    private async Task InvalidateProductCache(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating cache for product {ProductId}", notification.ProductId);
        await Task.Delay(10, cancellationToken);
    }

    private async Task UpdateSearchIndex(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating search index for product {ProductId}", notification.ProductId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task NotifySubscribers(ProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying subscribers of product {ProductId} update", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }
}