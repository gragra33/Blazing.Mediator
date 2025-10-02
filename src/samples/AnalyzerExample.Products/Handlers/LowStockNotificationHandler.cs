using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

public class LowStockNotificationHandler : INotificationHandler<LowStockNotification>
{
    private readonly ILogger<LowStockNotificationHandler> _logger;

    public LowStockNotificationHandler(ILogger<LowStockNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(LowStockNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning("?? [Products] Low stock alert: {ProductName} (ID: {ProductId}) has {CurrentStock} units remaining (threshold: {Threshold})", 
            notification.ProductName, notification.ProductId, notification.CurrentStock, notification.Threshold);
        
        // Simulate sending notification to procurement team
        await Task.Delay(25, cancellationToken);
        
        _logger.LogInformation("?? [Products] Low stock notification sent for product {ProductId}", notification.ProductId);
    }
}