using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Product notification handlers
/// </summary>
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? [Products] Product created event received: {ProductName} (ID: {ProductId})", 
            notification.ProductName, notification.ProductId);
        
        // Simulate additional processing like updating search index, cache invalidation, etc.
        await Task.Delay(50, cancellationToken);
        
        _logger.LogInformation("?? [Products] Product creation processing completed for {ProductId}", notification.ProductId);
    }
}