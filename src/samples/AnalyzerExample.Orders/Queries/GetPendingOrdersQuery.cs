using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class GetPendingOrdersQuery : IOrderQuery<List<OrderSummaryDto>>
{
    public int MaxAgeInHours { get; set; } = 24;
    public bool IncludeCancelled { get; set; } = false;
}