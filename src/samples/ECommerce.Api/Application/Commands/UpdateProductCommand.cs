using Blazing.Mediator;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to update an existing product.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class UpdateProductCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the product to update.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the updated name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the updated stock quantity of the product.
    /// </summary>
    public int StockQuantity { get; set; }
}