namespace ECommerce.Api.Application.DTOs;

/// <summary>
/// Data transfer object containing order statistics and metrics.
/// Used for transferring order analytics data to API consumers.
/// </summary>
public class OrderStatisticsDto
{
    /// <summary>
    /// Gets or sets the total number of orders in the system.
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Gets or sets the total revenue generated from all orders.
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Gets or sets the number of orders with pending status.
    /// </summary>
    public int PendingOrders { get; set; }

    /// <summary>
    /// Gets or sets the number of orders with completed status.
    /// </summary>
    public int CompletedOrders { get; set; }

    /// <summary>
    /// Gets or sets the average value per order.
    /// </summary>
    public decimal AverageOrderValue { get; set; }

    /// <summary>
    /// Gets or sets the list of top-selling products.
    /// </summary>
    public List<ProductSalesDto> TopProducts { get; set; } = [];
}