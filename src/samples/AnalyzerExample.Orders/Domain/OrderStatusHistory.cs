using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Orders.Domain;

public class OrderStatusHistory : BaseEntity
{
    public int OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}