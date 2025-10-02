using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Queries;

public class GetOrderByNumberQuery : IOrderQuery<OrderDetailDto?>, ICacheableQuery<OrderDetailDto?>
{
    public string OrderNumber { get; set; } = string.Empty;
    public bool IncludeCancelled { get; set; } = false;
    
    public string GetCacheKey() => $"order_number_{OrderNumber.ToUpperInvariant()}";
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}