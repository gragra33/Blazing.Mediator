namespace ECommerce.Api.Application.DTOs;

public class OrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<ProductSalesDto> TopProducts { get; set; } = [];
}