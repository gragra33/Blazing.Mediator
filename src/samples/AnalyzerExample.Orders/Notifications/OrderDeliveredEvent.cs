using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Notifications;

public class OrderDeliveredEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime DeliveredAt { get; set; }
    public string? DeliveryNotes { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderDelivered";
    public int Version => 1;
}