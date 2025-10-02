namespace AnalyzerExample.Orders.Queries;

public class GetOrderStatsQuery : IOrderQuery<OrderStatsDto>
{
    public int OrderId { get; set; }
    public bool IncludeCancelled { get; set; } = false;
}