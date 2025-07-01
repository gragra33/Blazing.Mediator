namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Data transfer object representing product sales metrics.
/// Used for transferring product sales analytics data to API consumers.
/// </summary>
public class ProductSalesDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total quantity of this product sold.
    /// </summary>
    public int TotalQuantitySold { get; set; }

    /// <summary>
    /// Gets or sets the total revenue generated from this product.
    /// </summary>
    public decimal TotalRevenue { get; set; }
}