namespace MiddlewareExample.Handlers;

/// <summary>
/// Handles <see cref="UpdateInventoryCommand"/> requests and returns the new stock count.
/// </summary>
public class UpdateInventoryCommandHandler(ILogger<UpdateInventoryCommandHandler> logger)
    : IRequestHandler<UpdateInventoryCommand, int>
{
    /// <inheritdoc />
    public Task<int> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogDebug(".. Updating inventory for product: {ProductId}, change: {QuantityChange}",
            request.ProductId, request.QuantityChange);

        // Simulate inventory update - starting with 25, applying the change
        var newStockCount = 25 + request.QuantityChange;

        logger.LogInformation("-- Inventory updated for {ProductId}. New stock count: {StockCount}",
            request.ProductId, newStockCount);

        return Task.FromResult(newStockCount);
    }
}
