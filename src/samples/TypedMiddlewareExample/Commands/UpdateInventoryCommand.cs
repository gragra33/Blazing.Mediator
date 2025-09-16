namespace TypedMiddlewareExample.Commands;

/// <summary>
/// Command for updating inventory levels for a product.
/// </summary>
public class UpdateInventoryCommand : ICommand<int>
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public required string ProductId { get; set; }

    /// <summary>
    /// Gets or sets the change in inventory (positive for increase, negative for decrease).
    /// </summary>
    public int InventoryChange { get; set; }
}