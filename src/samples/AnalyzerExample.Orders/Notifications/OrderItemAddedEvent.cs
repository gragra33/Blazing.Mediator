using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Notifications;

public class OrderItemAddedEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string AddedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderItemAdded";
    public int Version => 1;
}