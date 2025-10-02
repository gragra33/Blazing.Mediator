using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Notifications;

public class OrderCancelledEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string CancellationReason { get; set; } = string.Empty;
    public bool RefundRequested { get; set; }
    public string CancelledBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderCancelled";
    public int Version => 1;
}