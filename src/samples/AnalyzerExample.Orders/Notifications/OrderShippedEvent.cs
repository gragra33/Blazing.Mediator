using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Notifications;

public class OrderShippedEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
    public string Carrier { get; set; } = string.Empty;
    public DateTime ShippedAt { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderShipped";
    public int Version => 1;
}