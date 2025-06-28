using Blazing.Mediator;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to update the stock quantity of a product.
/// This is a CQRS command that represents a write operation.
/// </summary>
public class UpdateProductStockCommand : IRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the new stock quantity for the product.
    /// </summary>
    public int StockQuantity { get; set; }
}