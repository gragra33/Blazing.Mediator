using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Orders.Domain;

namespace AnalyzerExample.Orders.Notifications;

public class OrderProcessingCompletedEvent : IIntegrationEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source => "OrderService";
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public OrderStatus FinalStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public List<int> ProcessedItemIds { get; set; } = new();
}