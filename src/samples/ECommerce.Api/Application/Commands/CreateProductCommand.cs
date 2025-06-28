using Blazing.Mediator;

namespace ECommerce.Api.Application.Commands;

/// <summary>
/// Command to create a new product in the e-commerce system.
/// This is a CQRS command that represents a write operation and returns the product ID.
/// </summary>
// Product Commands
public class CreateProductCommand : IRequest<int>
{
    /// <summary>
    /// Gets or sets the name of the product to be created.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product to be created.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product to be created.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the initial stock quantity of the product to be created.
    /// </summary>
    public int StockQuantity { get; set; }
}

// Order Commands