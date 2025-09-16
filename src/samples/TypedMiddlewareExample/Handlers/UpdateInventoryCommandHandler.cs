using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for updating inventory levels.
/// </summary>
public class UpdateInventoryCommandHandler : ICommandHandler<UpdateInventoryCommand, int>
{
    private readonly ILogger<UpdateInventoryCommandHandler> _logger;
    private static readonly Dictionary<string, int> _inventory = new()
    {
        { "WIDGET-001", 25 }
    };

    public UpdateInventoryCommandHandler(ILogger<UpdateInventoryCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<int> Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(".. Updating inventory for product: {ProductId}, change: {InventoryChange}",
            request.ProductId, request.InventoryChange);

        // Simulate inventory update processing
        await Task.Delay(25, cancellationToken);

        if (_inventory.ContainsKey(request.ProductId))
        {
            _inventory[request.ProductId] += request.InventoryChange;
        }
        else
        {
            _inventory[request.ProductId] = Math.Max(0, request.InventoryChange);
        }

        var newCount = _inventory[request.ProductId];
        _logger.LogInformation("-- Inventory updated for {ProductId}. New stock count: {NewCount}",
            request.ProductId, newCount);

        return newCount;
    }
}