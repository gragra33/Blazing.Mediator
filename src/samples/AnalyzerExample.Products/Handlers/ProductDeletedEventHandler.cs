using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for product deleted events
/// </summary>
public class ProductDeletedEventHandler : INotificationHandler<ProductDeletedEvent>
{
    private readonly ILogger<ProductDeletedEventHandler> _logger;

    public ProductDeletedEventHandler(ILogger<ProductDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductDeletedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Product {ProductId} ({ProductName}) deleted by {DeletedBy}. Soft delete: {IsSoftDelete}",
            notification.ProductId, notification.ProductName, notification.DeletedBy, notification.IsSoftDelete);

        await UpdateRelatedOrders(notification, cancellationToken);
        await RemoveFromRecommendations(notification, cancellationToken);
        await ArchiveProductData(notification, cancellationToken);
    }

    private async Task UpdateRelatedOrders(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating related orders for deleted product {ProductId}", notification.ProductId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task RemoveFromRecommendations(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Removing product {ProductId} from recommendation systems", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task ArchiveProductData(ProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.IsSoftDelete)
        {
            _logger.LogDebug("Archiving data for permanently deleted product {ProductId}", notification.ProductId);
            await Task.Delay(50, cancellationToken);
        }
    }
}