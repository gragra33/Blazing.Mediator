using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class GetOrdersByStatusQuery : IOrderQuery<List<OrderSummaryDto>>
{
    public OrderStatus Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeCancelled { get; set; } = false;
}