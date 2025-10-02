using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class OrderStatsDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public OrderStatus CurrentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int DaysToShip { get; set; }
    public int DaysToDeliver { get; set; }
    public List<OrderStatusHistoryDto> StatusTransitions { get; set; } = new();
}