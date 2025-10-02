using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

/// <summary>
/// Order queries demonstrating various patterns
/// </summary>
public class GetOrderByIdQuery : IOrderQuery<OrderDetailDto?>
{
    public int OrderId { get; set; }
    public bool IncludeCancelled { get; set; } = false;
    public bool IncludeItems { get; set; } = true;
    public bool IncludeAddresses { get; set; } = true;
    public bool IncludeStatusHistory { get; set; } = true;
}