using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handles inventory updates for products in the system.
/// Returns the new stock count after the inventory change.
/// </summary>
public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, int>
{
    private readonly ILogger<UpdateInventoryCommandHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("TypedMiddlewareExample.UpdateInventoryCommandHandler", "2.0.0");

    public UpdateInventoryCommandHandler(ILogger<UpdateInventoryCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<int> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Handle_UpdateInventoryCommand");
        activity?.SetTag("inventory.product_id", request.ProductId);
        activity?.SetTag("inventory.change", request.InventoryChange);

        _logger.LogInformation("?? [UpdateInventoryHandler] Updating inventory for product: {ProductId}, change: {Change}", 
            request.ProductId, request.InventoryChange);

        // Simulate inventory lookup and update
        await Task.Delay(50, cancellationToken); // Simulate database call

        // For demo purposes, simulate current stock
        const int currentStock = 25;
        var newStock = Math.Max(0, currentStock + request.InventoryChange); // Ensure we don't go negative

        activity?.SetTag("inventory.new_stock", newStock);

        _logger.LogInformation("? [UpdateInventoryHandler] Inventory updated for {ProductId}. New stock count: {NewStock}", 
            request.ProductId, newStock);

        return newStock;
    }
}