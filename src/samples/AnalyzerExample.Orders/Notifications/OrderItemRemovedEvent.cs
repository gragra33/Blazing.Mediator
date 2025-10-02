using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Notifications;

public class OrderItemRemovedEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RemovedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderItemRemoved";
    public int Version => 1;
}