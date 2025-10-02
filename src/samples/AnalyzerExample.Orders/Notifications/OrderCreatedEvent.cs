using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Orders.Notifications;

/// <summary>
/// Order domain events and notifications
/// </summary>
public class OrderCreatedEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "OrderCreated";
    public int Version => 1;
}