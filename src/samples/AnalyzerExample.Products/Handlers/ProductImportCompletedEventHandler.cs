using AnalyzerExample.Products.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for product import completed events
/// </summary>
public class ProductImportCompletedEventHandler : INotificationHandler<ProductImportCompletedEvent>
{
    private readonly ILogger<ProductImportCompletedEventHandler> _logger;

    public ProductImportCompletedEventHandler(ILogger<ProductImportCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductImportCompletedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Product import completed: {FileName}. Total: {TotalRecords}, Successful: {SuccessfulImports}, Failed: {FailedImports}",
            notification.FileName, notification.TotalRecords, notification.SuccessfulImports, notification.FailedImports);

        await GenerateImportReport(notification, cancellationToken);
        await NotifyImportUser(notification, cancellationToken);
        await UpdateSearchIndex(notification, cancellationToken);
    }

    private async Task GenerateImportReport(ProductImportCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Generating import report for import {ImportId}", notification.ImportId);
        await Task.Delay(40, cancellationToken);
    }

    private async Task NotifyImportUser(ProductImportCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying {ImportedBy} of import completion for {ImportId}", 
            notification.ImportedBy, notification.ImportId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateSearchIndex(ProductImportCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating search index with {SuccessfulImports} imported products", 
            notification.SuccessfulImports);
        await Task.Delay(60, cancellationToken);
    }
}