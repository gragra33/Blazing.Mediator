namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command to update product inventory.
/// Uses custom IInventoryRequest interface to demonstrate type constraints.
/// </summary>
public class UpdateInventoryCommand : IInventoryRequest<int>
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public required string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the inventory change amount (positive for increase, negative for decrease).
    /// </summary>
    public required int InventoryChange { get; set; }
}