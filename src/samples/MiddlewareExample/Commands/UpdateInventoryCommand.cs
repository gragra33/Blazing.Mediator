namespace MiddlewareExample.Commands;

/// <summary>
/// Command to update product inventory and returns the new stock count.
/// </summary>
public class UpdateInventoryCommand : IRequest<int>
{
    /// <summary>
    /// Gets or sets the product ID to update.
    /// </summary>
    public required string ProductId { get; set; }
    
    /// <summary>
    /// Gets or sets the quantity to add or subtract from inventory.
    /// </summary>
    public required int QuantityChange { get; set; }
}
